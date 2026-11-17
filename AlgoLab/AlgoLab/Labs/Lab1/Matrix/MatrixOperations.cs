namespace AlgoLab.Labs.Lab1.Matrix;

internal static class MatrixOperations
{
    internal static double[,] Generate(int rows, int columns, Random random)
    {
        var result = new double[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                result[row, column] = random.NextDouble() * 100;
            }
        }

        return result;
    }

    internal static double[,] Multiply(double[,] a, double[,] b)
    {
        int rows = a.GetLength(0);
        int common = a.GetLength(1);

        if (common != b.GetLength(0))
        {
            throw new ArgumentException("Несовместимые размеры матриц.");
        }

        int columns = b.GetLength(1);
        var result = new double[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                double sum = 0;

                for (int k = 0; k < common; k++)
                {
                    sum += a[row, k] * b[k, column];
                }

                result[row, column] = sum;
            }
        }

        return result;
    }
}
