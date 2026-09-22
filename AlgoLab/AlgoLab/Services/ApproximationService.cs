using AlgoLab.Models;

namespace AlgoLab.Services;

public static class ApproximationService
{
    public static ApproximationResult Fit(
        IReadOnlyList<BenchmarkPoint> experimental,
        ComplexityType complexity)
    {
        if (experimental.Count == 0)
            return new ApproximationResult();

        double numerator = 0;
        double denominator = 0;

        foreach (var point in experimental)
        {
            double x = GetComplexityValue(
                point.N,
                complexity);

            numerator += point.TimeMs * x;
            denominator += x * x;
        }

        double coefficient =
            denominator == 0
                ? 0
                : numerator / denominator;

        var points = new List<BenchmarkPoint>();

        foreach (var point in experimental)
        {
            double x = GetComplexityValue(
                point.N,
                complexity);

            points.Add(new BenchmarkPoint
            {
                N = point.N,
                TimeMs = coefficient * x
            });
        }

        return new ApproximationResult
        {
            Coefficient = coefficient,
            R2 = CalculateR2(experimental, points),
            Points = points
        };
    }

    public static double GetComplexityValue(
        int n,
        ComplexityType complexity)
    {
        double value = Math.Max(1, n);

        return complexity switch
        {
            ComplexityType.Constant =>
                1,

            ComplexityType.Logarithmic =>
                Math.Log2(Math.Max(2, value)),

            ComplexityType.Linear =>
                value,

            ComplexityType.Linearithmic =>
                value * Math.Log2(Math.Max(2, value)),

            ComplexityType.Quadratic =>
                value * value,

            ComplexityType.Cubic =>
                value * value * value,

            ComplexityType.Exponential =>
                Math.Pow(2, value),

            ComplexityType.Factorial =>
                Factorial(value),

            _ => value
        };
    }

    private static double CalculateR2(
        IReadOnlyList<BenchmarkPoint> experimental,
        IReadOnlyList<BenchmarkPoint> approximation)
    {
        double average =
            experimental.Average(p => p.TimeMs);

        double total = 0;
        double residual = 0;

        for (int i = 0; i < experimental.Count; i++)
        {
            double actual =
                experimental[i].TimeMs;

            double predicted =
                approximation[i].TimeMs;

            total +=
                Math.Pow(actual - average, 2);

            residual +=
                Math.Pow(actual - predicted, 2);
        }

        if (total == 0)
            return 1;

        return 1 - residual / total;
    }

    private static double Factorial(
        double n)
    {
        if (n > 170)
            return double.MaxValue;

        double result = 1;

        for (int i = 2; i <= (int)n; i++)
            result *= i;

        return result;
    }
}