using System.Diagnostics;

namespace AlgoLab.Labs.Lab3.Queue;

/// <summary>Очередь готовых процессов (хранит номера процессов).</summary>
public interface IReadyQueue
{
    void Enqueue(int id);
    bool TryDequeue(out int id);
}

/// <summary>Своя очередь на связном списке.</summary>
public sealed class LinkedReadyQueue : IReadyQueue
{
    private readonly LinkedQueue<int> _q = new();
    public void Enqueue(int id) => _q.Enqueue(id);
    public bool TryDequeue(out int id) => _q.TryDequeue(out id);
}

/// <summary>Стандартная Queue&lt;T&gt;.</summary>
public sealed class StdReadyQueue : IReadyQueue
{
    private readonly global::System.Collections.Generic.Queue<int> _q = new();
    public void Enqueue(int id) => _q.Enqueue(id);
    public bool TryDequeue(out int id) => _q.TryDequeue(out id);
}

public sealed record RoundRobinBenchRow(
    string Mix, int Processes, long Quanta, double OwnMs, double StdMs);

public static class RoundRobinBench
{
    public static readonly int[] Quanta = { 1, 4, 16 };

    public static readonly string[] MixNames =
    {
        "Квант 1 (очень частые переключения)",
        "Квант 4",
        "Квант 16 (редкие переключения)"
    };

    /// <summary>Времена выполнения процессов от 1 до 20 (детерминированный seed).</summary>
    public static int[] GenerateBursts(int count)
    {
        var rnd = new Random(777 + count);
        var bursts = new int[count];
        for (int i = 0; i < count; i++) bursts[i] = rnd.Next(1, 21);
        return bursts;
    }

    /// <summary>Round Robin: взять процесс из начала, дать квант, при необходимости вернуть в хвост.</summary>
    public static (long Quanta, long TotalTime) Simulate(int[] bursts, int quantum, IReadyQueue ready)
    {
        var remaining = (int[])bursts.Clone();
        for (int i = 0; i < remaining.Length; i++) ready.Enqueue(i);

        long time = 0, quanta = 0;
        while (ready.TryDequeue(out var id))
        {
            var duration = Math.Min(quantum, remaining[id]);
            time += duration;
            remaining[id] -= duration;
            quanta++;
            if (remaining[id] > 0) ready.Enqueue(id);
        }
        return (quanta, time);
    }

    /// <summary>Замер на обеих очередях для одних и тех же процессов, среднее по runs.</summary>
    public static RoundRobinBenchRow Measure(int mix, int processes, int runs)
    {
        var bursts = GenerateBursts(processes);
        var quantum = Quanta[mix];
        long quanta = 0;
        double own = 0, std = 0;

        for (int r = 0; r < runs; r++)
        {
            var sw = Stopwatch.StartNew();
            quanta = Simulate(bursts, quantum, new LinkedReadyQueue()).Quanta;
            sw.Stop();
            own += sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            Simulate(bursts, quantum, new StdReadyQueue());
            sw.Stop();
            std += sw.Elapsed.TotalMilliseconds;
        }
        return new RoundRobinBenchRow(MixNames[mix], processes, quanta, own / runs, std / runs);
    }
}
