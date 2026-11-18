using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace AlgoLab.Labs.Lab3.Queue;

public enum QueueOp { Enqueue = 1, Dequeue = 2, Peek = 3, IsEmpty = 4, Print = 5 }

public readonly record struct QueueCommand(QueueOp Op, string? Arg);

public interface IQueueAdapter
{
    void Enqueue(string value);
    string Dequeue();
    string Peek();
    bool IsEmpty();
    string Print();
}

/// <summary>Своя очередь на связном списке (LinkedQueue).</summary>
public sealed class MyQueueAdapter : IQueueAdapter
{
    private readonly LinkedQueue<string> _q = new();

    public void Enqueue(string value) => _q.Enqueue(value);

    public string Dequeue() =>
        _q.TryDequeue(out var value) ? value : throw new InvalidOperationException(QueueCommands.EmptyMessage);

    public string Peek() =>
        _q.TryPeek(out var value) ? value : throw new InvalidOperationException(QueueCommands.EmptyMessage);

    public bool IsEmpty() => _q.IsEmpty;

    /// <summary>O(n): элементы от начала очереди к концу.</summary>
    public string Print()
    {
        var sb = new StringBuilder();
        foreach (var item in _q)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(item);
        }
        return sb.ToString();
    }
}

/// <summary>Стандартная Queue&lt;T&gt;.</summary>
public sealed class StdQueueAdapter : IQueueAdapter
{
    private readonly global::System.Collections.Generic.Queue<string> _q = new();

    public void Enqueue(string value) => _q.Enqueue(value);
    public string Dequeue() => _q.Dequeue();
    public string Peek() => _q.Peek();
    public bool IsEmpty() => _q.Count == 0;

    // Queue<T> перечисляется от начала к концу — тот же порядок, что у LinkedQueue.
    public string Print()
    {
        var sb = new StringBuilder();
        foreach (var item in _q)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(item);
        }
        return sb.ToString();
    }
}

public sealed record QueueBenchRow(
    string Mix, int Ops, double ReadMs, double OwnMs, double StdMs);

public static class QueueCommands
{
    public const string EmptyMessage = "Очередь пуста";

    /// <summary>Разбор содержимого input.txt: «3 4 1,56 1,cat 2 5 4».</summary>
    public static List<QueueCommand> Parse(string text)
    {
        var result = new List<QueueCommand>();
        foreach (var token in text.Split(
                     new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var comma = token.IndexOf(',');
            var codeStr = comma < 0 ? token : token[..comma];
            if (!int.TryParse(codeStr, NumberStyles.None, CultureInfo.InvariantCulture, out var code)
                || code < 1 || code > 5)
                throw new FormatException($"Неизвестная команда «{token}». Допустимо 1–5.");

            var op = (QueueOp)code;
            if (op == QueueOp.Enqueue)
            {
                if (comma < 0 || comma == token.Length - 1)
                    throw new FormatException($"Для вставки нужно значение: «1,значение» (получено «{token}»).");
                result.Add(new(op, token[(comma + 1)..]));
            }
            else
            {
                if (comma >= 0)
                    throw new FormatException($"У команды {code} не должно быть аргумента («{token}»).");
                result.Add(new(op, null));
            }
        }
        return result;
    }

    /// <summary>Выполняет команды, результат каждой передаётся в sink.</summary>
    public static void Execute(IReadOnlyList<QueueCommand> commands, IQueueAdapter queue, Action<string> sink)
    {
        foreach (var cmd in commands)
        {
            switch (cmd.Op)
            {
                case QueueOp.Enqueue:
                    queue.Enqueue(cmd.Arg!);
                    sink($"Enqueue({cmd.Arg})");
                    break;
                case QueueOp.Dequeue:
                    sink(queue.IsEmpty() ? $"Dequeue: {EmptyMessage}" : $"Dequeue → {queue.Dequeue()}");
                    break;
                case QueueOp.Peek:
                    sink(queue.IsEmpty() ? $"Peek: {EmptyMessage}" : $"Peek → {queue.Peek()}");
                    break;
                case QueueOp.IsEmpty:
                    sink($"isEmpty → {(queue.IsEmpty() ? "да" : "нет")}");
                    break;
                case QueueOp.Print:
                    sink(queue.IsEmpty() ? $"Print: {EmptyMessage}" : $"Print → {queue.Print()}");
                    break;
            }
        }
    }

    // ───────────── Генерация наборов для замеров ─────────────

    public static readonly string[] MixNames =
    {
        "Только вставка",
        "Случайный набор (Print ≈ 1%)",
        "Много Print (≈ 5%)"
    };

    /// <summary>Текст input.txt заданного размера и состава (детерминированный seed).</summary>
    public static string GenerateInput(int mix, int ops)
    {
        var rnd = new Random(12345 + ops);
        var sb = new StringBuilder(ops * 6);
        for (int i = 0; i < ops; i++)
        {
            if (i > 0) sb.Append(' ');
            int roll = rnd.Next(100);
            var op = mix switch
            {
                0 => QueueOp.Enqueue,
                1 => roll switch { < 45 => QueueOp.Enqueue, < 75 => QueueOp.Dequeue, < 87 => QueueOp.Peek,
                                   < 99 => QueueOp.IsEmpty, _ => QueueOp.Print },
                _ => roll switch { < 55 => QueueOp.Enqueue, < 75 => QueueOp.Dequeue, < 85 => QueueOp.Peek,
                                   < 95 => QueueOp.IsEmpty, _ => QueueOp.Print },
            };
            if (op == QueueOp.Enqueue)
            {
                sb.Append("1,");
                if (rnd.Next(3) == 0) sb.Append("word").Append(rnd.Next(1000));
                else sb.Append(rnd.Next(10000));
            }
            else sb.Append((int)op);
        }
        return sb.ToString();
    }

    /// <summary>Замер чтения (разбор) и выполнения на обеих реализациях, среднее по runs.</summary>
    public static QueueBenchRow Measure(int mix, int ops, int runs)
    {
        var text = GenerateInput(mix, ops);
        long outLen = 0;
        void Sink(string s) => outLen += s.Length;

        double read = 0, own = 0, std = 0;
        for (int r = 0; r < runs; r++)
        {
            var sw = Stopwatch.StartNew();
            var cmds = Parse(text);
            sw.Stop();
            read += sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            Execute(cmds, new MyQueueAdapter(), Sink);
            sw.Stop();
            own += sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            Execute(cmds, new StdQueueAdapter(), Sink);
            sw.Stop();
            std += sw.Elapsed.TotalMilliseconds;
        }
        return new QueueBenchRow(MixNames[mix], ops, read / runs, own / runs, std / runs);
    }
}
