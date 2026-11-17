using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace AlgoLab.Services.Lab3;

public enum StackOp { Push = 1, Pop = 2, Top = 3, IsEmpty = 4, Print = 5 }

public readonly record struct StackCommand(StackOp Op, string? Arg);

public interface IStackAdapter
{
    void Push(string value);
    string Pop();
    string Top();
    bool IsEmpty();
    string Print();
}

public sealed class MyStackAdapter : IStackAdapter
{
    private readonly MyStack<string> _s = new();
    public void Push(string value) => _s.Push(value);
    public string Pop() => _s.Pop();
    public string Top() => _s.Top();
    public bool IsEmpty() => _s.IsEmpty();
    public string Print() => _s.Print();
}

public sealed class StdStackAdapter : IStackAdapter
{
    private readonly Stack<string> _s = new();
    public void Push(string value) => _s.Push(value);
    public string Pop() => _s.Pop();
    public string Top() => _s.Peek();
    public bool IsEmpty() => _s.Count == 0;
    // Stack<T> перечисляется от вершины к основанию — тот же порядок, что у MyStack.
    public string Print() => string.Join(' ', _s);
}

public sealed record StackBenchRow(
    string Mix, int Ops, double ReadMs, double OwnMs, double StdMs);

public static class StackCommands
{
    public const string EmptyMessage = "Стек пуст";

    /// <summary>Разбор содержимого input.txt: «3 4 1,56 1,cat 2 5 4».</summary>
    public static List<StackCommand> Parse(string text)
    {
        var result = new List<StackCommand>();
        foreach (var token in text.Split(
                     new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var comma = token.IndexOf(',');
            var codeStr = comma < 0 ? token : token[..comma];
            if (!int.TryParse(codeStr, NumberStyles.None, CultureInfo.InvariantCulture, out var code)
                || code < 1 || code > 5)
                throw new FormatException($"Неизвестная команда «{token}». Допустимо 1–5.");

            var op = (StackOp)code;
            if (op == StackOp.Push)
            {
                if (comma < 0 || comma == token.Length - 1)
                    throw new FormatException($"Для Push нужно значение: «1,значение» (получено «{token}»).");
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
    public static void Execute(IReadOnlyList<StackCommand> commands, IStackAdapter stack, Action<string> sink)
    {
        foreach (var cmd in commands)
        {
            switch (cmd.Op)
            {
                case StackOp.Push:
                    stack.Push(cmd.Arg!);
                    sink($"Push({cmd.Arg})");
                    break;
                case StackOp.Pop:
                    sink(stack.IsEmpty() ? $"Pop: {EmptyMessage}" : $"Pop → {stack.Pop()}");
                    break;
                case StackOp.Top:
                    sink(stack.IsEmpty() ? $"Top: {EmptyMessage}" : $"Top → {stack.Top()}");
                    break;
                case StackOp.IsEmpty:
                    sink($"isEmpty → {(stack.IsEmpty() ? "да" : "нет")}");
                    break;
                case StackOp.Print:
                    sink(stack.IsEmpty() ? $"Print: {EmptyMessage}" : $"Print → {stack.Print()}");
                    break;
            }
        }
    }

    // ───────────── Генерация наборов для замеров ─────────────

    public static readonly string[] MixNames =
    {
        "Только Push",
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
                0 => StackOp.Push,
                1 => roll switch { < 45 => StackOp.Push, < 75 => StackOp.Pop, < 87 => StackOp.Top,
                                   < 99 => StackOp.IsEmpty, _ => StackOp.Print },
                _ => roll switch { < 55 => StackOp.Push, < 75 => StackOp.Pop, < 85 => StackOp.Top,
                                   < 95 => StackOp.IsEmpty, _ => StackOp.Print },
            };
            if (op == StackOp.Push)
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
    public static StackBenchRow Measure(int mix, int ops, int runs)
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
            Execute(cmds, new MyStackAdapter(), Sink);
            sw.Stop();
            own += sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            Execute(cmds, new StdStackAdapter(), Sink);
            sw.Stop();
            std += sw.Elapsed.TotalMilliseconds;
        }
        return new StackBenchRow(MixNames[mix], ops, read / runs, own / runs, std / runs);
    }
}
