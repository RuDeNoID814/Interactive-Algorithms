namespace AlgoLab.Models;

public class ExperimentPointDto
{
    public int Id { get; set; }

    public int ExperimentId { get; set; }

    public int N { get; set; }

    public int Run { get; set; }

    public double TimeMs { get; set; }

    public double ApproximationTimeMs { get; set; }

    public bool IsAverage { get; set; }
}