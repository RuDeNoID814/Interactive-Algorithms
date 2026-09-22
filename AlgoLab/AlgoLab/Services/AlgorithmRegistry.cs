using AlgoLab.Models;

namespace AlgoLab.Services;

public static class AlgorithmRegistry
{
    public static IReadOnlyList<AlgorithmInfo> All { get; } =
    [
        new()
        {
            Name = "f(n) = 1",
            Description = "Константная функция",
            Complexity = "O(1)",
            ComplexityType = ComplexityType.Constant,
            Slug = "const",
            Category = "Векторы"
        },

        new()
        {
            Name = "Сумма элементов",
            Description = "Линейный проход по массиву",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "sum",
            Category = "Векторы"
        },

        new()
        {
            Name = "Произведение элементов",
            Description = "Линейный проход по массиву",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "product",
            Category = "Векторы"
        },

        new()
        {
            Name = "Наивный полином",
            Description = "Прямое вычисление полинома",
            Complexity = "O(n²)",
            ComplexityType = ComplexityType.Quadratic,
            Slug = "poly-naive",
            Category = "Полиномы"
        },

        new()
        {
            Name = "Метод Горнера",
            Description = "Эффективное вычисление полинома",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "horner",
            Category = "Полиномы"
        },

        new()
        {
            Name = "Пузырьковая сортировка",
            Description = "Сравнение соседних элементов",
            Complexity = "O(n²)",
            ComplexityType = ComplexityType.Quadratic,
            Slug = "bubble",
            Category = "Сортировки"
        },

        new()
        {
            Name = "Быстрая сортировка",
            Description = "Divide & Conquer",
            Complexity = "O(n log n)",
            ComplexityType = ComplexityType.Linearithmic,
            Slug = "quick",
            Category = "Сортировки"
        },

        new()
        {
            Name = "TimSort",
            Description = "Гибридный алгоритм сортировки",
            Complexity = "O(n log n)",
            ComplexityType = ComplexityType.Linearithmic,
            Slug = "timsort",
            Category = "Сортировки"
        },

        new()
        {
            Name = "Возведение в степень",
            Description = "Последовательное умножение",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "pow-simple",
            Category = "Степень"
        },

        new()
        {
            Name = "Степень рекурсивно",
            Description = "Рекурсивное быстрое возведение",
            Complexity = "O(log n)",
            ComplexityType = ComplexityType.Logarithmic,
            Slug = "pow-recursive",
            Category = "Степень"
        },

        new()
        {
            Name = "Быстрое возведение",
            Description = "Бинарный алгоритм",
            Complexity = "O(log n)",
            ComplexityType = ComplexityType.Logarithmic,
            Slug = "pow-fast",
            Category = "Степень"
        },

        new()
        {
            Name = "Классическое быстрое возведение",
            Description = "QuickPow из задания",
            Complexity = "O(log n)",
            ComplexityType = ComplexityType.Logarithmic,
            Slug = "pow-classic",
            Category = "Степень"
        },

        new()
        {
            Name = "Умножение матриц",
            Description = "Классический алгоритм с тремя циклами",
            Complexity = "O(n³)",
            ComplexityType = ComplexityType.Cubic,
            Slug = "matrix",
            Category = "Матрицы"
        }
    ];

    public static AlgorithmInfo? Find(string slug)
    {
        return All.FirstOrDefault(
            algorithm =>
                algorithm.Slug == slug);
    }
}