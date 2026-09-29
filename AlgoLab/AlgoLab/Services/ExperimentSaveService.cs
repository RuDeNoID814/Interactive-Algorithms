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
        IReadOnlyList<BenchmarkPoint>? individualRuns = null,
        IReadOnlyList<MatrixSurfacePoint>? surfacePoints = null)
    {
        var experiment =
            new ExperimentDto
            {
                AlgorithmSlug = slug,
                AlgorithmName = name,
                Complexity = complexity,
                MaxN = maxN,
                Step = step,
                Runs = runs,

                ApproximationCoefficient =
                    approximation.Coefficient,

                R2 =
                    approximation.R2
            };


        // =========================================
        // ОБЫЧНЫЕ 2D-ТОЧКИ
        // =========================================

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
                    M = 0,
                    Run = 0,

                    TimeMs =
                        point.TimeMs,

                    ApproximationTimeMs =
                        approximate?.TimeMs ?? 0,

                    IsAverage = true,
                    Is3D = false
                });
        }


        // =========================================
        // ОТДЕЛЬНЫЕ ЗАПУСКИ
        // =========================================

        if (individualRuns is not null)
        {
            foreach (var point in individualRuns)
            {
                experiment.Points.Add(
                    new ExperimentPointDto
                    {
                        N = point.N,
                        M = 0,

                        Run = point.Run,

                        TimeMs =
                            point.TimeMs,

                        ApproximationTimeMs = 0,

                        IsAverage = false,
                        Is3D = false
                    });
            }
        }


        // =========================================
        // MATRIX SURFACE
        // =========================================

        if (surfacePoints is not null)
        {
            foreach (var point in surfacePoints)
            {
                experiment.Points.Add(
                    new ExperimentPointDto
                    {
                        N = point.N,

                        M = point.M,

                        Run = 0,

                        TimeMs =
                            point.TimeMs,

                        ApproximationTimeMs = 0,

                        IsAverage = false,

                        Is3D = true
                    });
            }
        }


        return await _historyService
            .SaveAsync(experiment);
    }

    // хз чо это

    public async Task<ExperimentDto?> CheckCacheAsync(
        string slug, int maxN, int step, int runs)
    {
        return await _historyService.FindByParametersAsync(slug, maxN, step, runs);
    }
}