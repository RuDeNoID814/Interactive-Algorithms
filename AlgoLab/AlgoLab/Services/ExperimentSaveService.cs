using AlgoLab.Models;

namespace AlgoLab.Services;

public class ExperimentSaveService
{
    private readonly ExperimentHistoryService _historyService;

    public ExperimentSaveService(
        ExperimentHistoryService historyService)
    {
        _historyService = historyService;
    }


    public async Task<ExperimentDto?> SaveAsync(
        string slug,
        string name,
        string complexity,
        int maxN,
        int step,
        int runs,
        IReadOnlyList<BenchmarkPoint> points,
        ApproximationResult approximation,
        IReadOnlyList<BenchmarkPoint>? individualRuns = null)
    {
        var experiment = new ExperimentDto
        {
            AlgorithmSlug = slug,
            AlgorithmName = name,
            Complexity = complexity,
            MaxN = maxN,
            Step = step,
            Runs = runs,
            ApproximationCoefficient =
                approximation.Coefficient,
            R2 = approximation.R2
        };


        // Средние точки — используются обычным 2D-графиком.

        foreach (var point in points)
        {
            var approximate =
                approximation.Points
                    .FirstOrDefault(
                        p => p.N == point.N);

            experiment.Points.Add(
                new ExperimentPointDto
                {
                    N = point.N,
                    Run = 0,
                    TimeMs = point.TimeMs,
                    ApproximationTimeMs =
                        approximate?.TimeMs ?? 0,
                    IsAverage = true
                });
        }


        // Отдельные запуски.
        // Нужны, например, для 3D матриц.

        if (individualRuns is not null)
        {
            foreach (var point in individualRuns)
            {
                experiment.Points.Add(
                    new ExperimentPointDto
                    {
                        N = point.N,
                        Run = point.Run,
                        TimeMs = point.TimeMs,
                        ApproximationTimeMs = 0,
                        IsAverage = false
                    });
            }
        }


        return await _historyService
            .SaveAsync(experiment);
    }
}