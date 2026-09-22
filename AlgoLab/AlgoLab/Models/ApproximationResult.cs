namespace AlgoLab.Models;

public sealed class ApproximationResult
{
    public double Coefficient { get; init; }

    public double R2 { get; init; }

    public List<BenchmarkPoint> Points { get; init; } = new();
}