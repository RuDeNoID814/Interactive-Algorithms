namespace AlgoLab.Models;

public class ExperimentPointDto
{
    public int Id { get; set; }

    public int ExperimentId { get; set; }

    public int N { get; set; }

    // Вторая размерность для 3D Matrix.
    public int M { get; set; }

    // Номер повторного запуска.
    public int Run { get; set; }

    public double TimeMs { get; set; }

    public double ApproximationTimeMs { get; set; }

    // Обычная усреднённая точка 2D-графика.
    public bool IsAverage { get; set; }

    // Точка поверхности (n, m, time).
    public bool Is3D { get; set; }
}