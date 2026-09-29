using System.Text.Json.Serialization;

namespace AlgoLab.Api.Models;

public class ExperimentPoint
{
    public int Id { get; set; }

    public int ExperimentId { get; set; }

    // Основная координата.
    public int N { get; set; }

    // Вторая размерность для Matrix 3D.
    // Для обычных экспериментов = 0.
    public int M { get; set; }

    // Для старого формата отдельных запусков.
    public int Run { get; set; }

    public double TimeMs { get; set; }

    public double ApproximationTimeMs { get; set; }

    // Средняя точка обычного 2D-эксперимента.
    public bool IsAverage { get; set; }

    // Точка поверхности (n, m, time).
    public bool Is3D { get; set; }

    [JsonIgnore]
    public Experiment? Experiment { get; set; }
}