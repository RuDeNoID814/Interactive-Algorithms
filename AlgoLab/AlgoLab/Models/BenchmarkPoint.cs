namespace AlgoLab.Models;

public sealed class BenchmarkPoint
{
    public int N { get; set; }

    public double TimeMs { get; set; }

    public int Run { get; set; }
}