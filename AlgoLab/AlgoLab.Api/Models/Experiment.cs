namespace AlgoLab.Api.Models;

public class Experiment
{
    public int Id { get; set; }

    public string AlgorithmSlug { get; set; } = "";

    public string AlgorithmName { get; set; } = "";

    public string Complexity { get; set; } = "";

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public int MaxN { get; set; }

    public int Step { get; set; }

    public int Runs { get; set; }

    public double ApproximationCoefficient { get; set; }

    public double R2 { get; set; }

    public List<ExperimentPoint> Points { get; set; }
        = new();
}