using System.Diagnostics;

namespace AlgoLab.Labs.Lab1.Matrix;

internal static class MatrixBenchmark
{
    internal static double MeasureMultiplication(double[,] a, double[,] b)
    {
        var stopwatch = Stopwatch.StartNew();
        MatrixOperations.Multiply(a, b);
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalMilliseconds;
    }

    internal static List<int> BuildValues(int maximum, int step)
    {
        var values = new List<int>();

        for (int value = 1; value <= maximum; value += step)
        {
            values.Add(value);
        }

        if (values.Count == 0 || values[^1] != maximum)
        {
            values.Add(maximum);
        }

        return values;
    }
}
