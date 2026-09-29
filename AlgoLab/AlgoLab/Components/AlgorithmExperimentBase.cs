using AlgoLab.Models;
using AlgoLab.Services;
using Microsoft.AspNetCore.Components;

namespace AlgoLab.Components;

public abstract class AlgorithmExperimentBase : ComponentBase
{
    [Inject] protected ExperimentSaveService SaveService { get; set; } = default!;

    protected abstract string AlgorithmSlug { get; }
    protected abstract string AlgorithmName { get; }
    protected abstract string AlgorithmComplexity { get; }
    protected abstract ComplexityType ComplexityType { get; }

    protected virtual int MaxN { get; set; } = 2000;
    protected virtual int Step { get; set; } = 50;
    protected virtual int Runs { get; set; } = 5;

    protected int CurrentN { get; set; }
    protected int CurrentRun { get; set; }
    protected double Progress { get; set; }
    protected bool Running { get; set; }

    protected string Error { get; set; } = "";
    protected string Saved { get; set; } = "";

    protected List<BenchmarkPoint> Points { get; set; } = new();
    protected ApproximationResult Approximation { get; set; } = new();

    protected bool ShowCachePrompt { get; set; }
    protected ExperimentDto? CachedExperiment { get; set; }
    private bool _forceRecalculate = false;

    protected string ProgressStyle =>
        $"width:{Progress.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}%;";

    protected virtual IReadOnlyList<BenchmarkPoint>? GetIndividualRuns() => null;

    protected virtual ApproximationResult ComputeApproximation(List<BenchmarkPoint> points)
        => ApproximationService.Fit(points, ComplexityType);

    protected async Task OnRunClicked()
    {
        if (Running) return;
        Error = "";
        Saved = "";

        if (MaxN < 1 || Step < 1)
        {
            Error = "n и шаг должны быть больше нуля.";
            return;
        }

        if (!_forceRecalculate)
        {
            try
            {
                var cache = await SaveService.CheckCacheAsync(AlgorithmSlug, MaxN, Step, Runs);
                if (cache != null)
                {
                    CachedExperiment = cache;
                    ShowCachePrompt = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки кэша: {ex.Message}");
            }
        }

        await ExecuteExperimentAsync();
    }

    protected virtual void UseCachedResult()
    {
        ShowCachePrompt = false;
        if (CachedExperiment == null) return;

        Points = CachedExperiment.Points
            .Where(p => p.IsAverage)
            .Select(p => new BenchmarkPoint { N = p.N, TimeMs = p.TimeMs })
            .ToList();

        var approxPoints = CachedExperiment.Points
            .Where(p => p.IsAverage)
            .Select(p => new BenchmarkPoint { N = p.N, TimeMs = p.ApproximationTimeMs })
            .ToList();

        Approximation = new ApproximationResult
        {
            Coefficient = CachedExperiment.ApproximationCoefficient,
            R2 = CachedExperiment.R2,
            Points = approxPoints
        };

        Saved = "Загружен сохраненный эксперимент.";
        StateHasChanged();
    }

    protected async Task ForceRecalculate()
    {
        ShowCachePrompt = false;
        _forceRecalculate = true;
        await ExecuteExperimentAsync();
        _forceRecalculate = false;
    }

    private async Task ExecuteExperimentAsync()
    {
        Running = true;
        Points.Clear();

        try
        {
            Points = await RunExperimentLogicAsync(MaxN, Step, Runs, (n, run, prog) =>
            {
                CurrentN = n;
                CurrentRun = run;
                Progress = prog;
                StateHasChanged();
            });

            Approximation = ComputeApproximation(Points);

            await SaveService.SaveAsync(
                AlgorithmSlug, AlgorithmName, AlgorithmComplexity,
                MaxN, Step, Runs, Points, Approximation, GetIndividualRuns());

            Saved = "Эксперимент сохранён.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            Progress = 100;
            Running = false;
            CurrentN = 0;
            CurrentRun = 0;
        }
    }

    protected abstract Task<List<BenchmarkPoint>> RunExperimentLogicAsync(
        int maxN, int step, int runs, Action<int, int, double> updateProgress);

    protected static List<int> BuildSizes(int max, int step)
    {
        var result = new List<int>();
        for (int n = 1; n <= max; n += step) result.Add(n);
        if (result.Count == 0 || result[^1] != max) result.Add(max);
        return result;
    }
}