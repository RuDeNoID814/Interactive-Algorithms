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
            Category = "Векторы",
            Formula = "f(n) = 1"
        },

        new()
        {
            Name = "Сумма элементов",
            Description = "Линейный проход по массиву",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "sum",
            Category = "Векторы",
            Formula = "Σ aᵢ"
        },

        new()
        {
            Name = "Произведение элементов",
            Description = "Линейный проход по массиву",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "product",
            Category = "Векторы",
            Formula = "Π aᵢ"
        },

        new()
        {
            Name = "Наивный полином",
            Description = "Прямое вычисление полинома",
            Complexity = "O(n²)",
            ComplexityType = ComplexityType.Quadratic,
            Slug = "poly-naive",
            Category = "Полиномы",
            Formula = "Σ aᵢxⁱ"
        },

        new()
        {
            Name = "Метод Горнера",
            Description = "Эффективное вычисление полинома",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "horner",
            Category = "Полиномы",
            Formula = "(…(aₙx+aₙ₋₁)x+…)"
        },

        new()
        {
            Name = "Пузырьковая сортировка",
            Description = "Сравнение соседних элементов",
            Complexity = "O(n²)",
            ComplexityType = ComplexityType.Quadratic,
            Slug = "bubble",
            Category = "Сортировки",
            Formula = "aᵢ ↔ aᵢ₊₁"
        },

        new()
        {
            Name = "Быстрая сортировка",
            Description = "Divide & Conquer",
            Complexity = "O(n log n)",
            ComplexityType = ComplexityType.Linearithmic,
            Slug = "quick",
            Category = "Сортировки",
            Formula = "divide & conquer"
        },

        new()
        {
            Name = "TimSort",
            Description = "Гибридный алгоритм сортировки",
            Complexity = "O(n log n)",
            ComplexityType = ComplexityType.Linearithmic,
            Slug = "timsort",
            Category = "Сортировки",
            Formula = "run + merge"
        },

        new()
        {
            Name = "Возведение в степень",
            Description = "Последовательное умножение",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "pow-simple",
            Category = "Степень",
            Formula = "x·x·…·x"
        },

        new()
        {
            Name = "Степень рекурсивно",
            Description = "Рекурсивное последовательное возведение",
            Complexity = "O(n)",
            ComplexityType = ComplexityType.Linear,
            Slug = "pow-recursive",
            Category = "Степень",
            Formula = "x·xⁿ⁻¹"
        },

        new()
        {
            Name = "Быстрое возведение",
            Description = "Бинарный алгоритм",
            Complexity = "O(log n)",
            ComplexityType = ComplexityType.Logarithmic,
            Slug = "pow-fast",
            Category = "Степень",
            Formula = "(x^(n/2))²"
        },

        new()
        {
            Name = "Умножение матриц",
            Description = "Классический алгоритм с тремя циклами",
            Complexity = "O(n³)",
            ComplexityType = ComplexityType.Cubic,
            Slug = "matrix",
            Category = "Матрицы",
            Formula = "Σ Aᵢₖ·Bₖⱼ"
        },
        new()
        {
            Name = "Задача коммивояжёра",
            Description =
                "Поиск кратчайшего маршрута полным перебором",

            Complexity = "O(n!)",

            ComplexityType =
                ComplexityType.Factorial,

            Slug = "tsp",

            Category =
                "Труднорешаемые задачи",

            Formula = "min Σ d(i,j)"
        },
        new()
        {
            Name = "Обход в ширину (BFS)",
            Description = "Обход графа уровень за уровнем через очередь",
            Complexity = "O(V+E)",
            ComplexityType = ComplexityType.Linear,
            Slug = "bfs",
            Category = "Графы",
            Formula = "queue + visited"
        },

        new()
        {
            Name = "Алгоритм Хаффмана",
            Description = "Метод построения оптимального префиксного кода",
            Complexity = "O(n log n)",
            ComplexityType = ComplexityType.Linearithmic,
            Slug = "huffman",
            Category = "Жадные алгоритмы",
            Formula = "min-heap merge"
        }
    ];

    public static AlgorithmInfo? Find(string slug)
    {
        return All.FirstOrDefault(
            algorithm =>
                algorithm.Slug == slug);
    }
}