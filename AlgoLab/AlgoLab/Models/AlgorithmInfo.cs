namespace AlgoLab.Models;

public sealed class AlgorithmInfo
{
    public string Name { get; init; } = "";

    public string Description { get; init; } = "";

    public string Complexity { get; init; } = "";

    public ComplexityType ComplexityType { get; init; }

    public string Slug { get; init; } = "";

    public string Category { get; init; } = "";

    public int Laboratory { get; init; } = 1;

    public bool CanCompare { get; init; } = true;
}