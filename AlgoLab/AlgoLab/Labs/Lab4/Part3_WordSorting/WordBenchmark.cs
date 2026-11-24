using System.Diagnostics;

namespace AlgoLab.Labs.Lab4.Part3_WordSorting;

/// <summary>
/// Один замер выбранного алгоритма на конкретном тексте: время, счётчики,
/// результат сортировки, частоты и признак совпадения с эталоном.
/// </summary>
/// <remarks>
/// И частоты, и проверка считаются ПОСЛЕ остановки секундомера — в таблицу попадает
/// чистое время сортировки, без подсчёта частот и сверки с эталоном.
/// </remarks>
public sealed record WordSortRun
{
    /// <summary>Название алгоритма для интерфейса.</summary>
    public required string Name { get; init; }

    /// <summary>Отсортированный массив слов.</summary>
    public required string[] Sorted { get; init; }

    /// <summary>Время сортировки в миллисекундах (среднее по запускам, если их больше одного).</summary>
    public double Milliseconds { get; init; }

    /// <summary>Совпал ли результат с эталонной сортировкой.</summary>
    public bool IsCorrect { get; init; }

    /// <summary>Счётчики операций алгоритма.</summary>
    public required WordSortResult Counters { get; init; }

    /// <summary>Итог подсчёта частот по отсортированному массиву.</summary>
    public required WordFrequencyReport Frequencies { get; init; }
}

/// <summary>
/// Строка таблицы экспериментов: один размер текста и времена всех трёх алгоритмов.
/// </summary>
/// <remarks>
/// Хранит именно измеренные величины, а не производные: отношения считаются
/// в интерфейсе, чтобы при делении на ноль не портить данные отчёта.
/// </remarks>
public sealed record WordBenchRow
{
    /// <summary>Сколько слов было в тексте.</summary>
    public int Words { get; init; }

    /// <summary>Среднее время быстрой сортировки, мс.</summary>
    public double QuickMs { get; init; }

    /// <summary>Среднее время LSD-сортировки, мс.</summary>
    public double RadixMs { get; init; }

    /// <summary>Среднее время ABC-сортировки, мс.</summary>
    public double AbcMs { get; init; }

    /// <summary>Сколько различных слов оказалось в тексте (одинаково для всех алгоритмов).</summary>
    public int DistinctWords { get; init; }

    /// <summary>Все ли три алгоритма прошли проверку с эталоном на этом тексте.</summary>
    public bool AllCorrect { get; init; }

    /// <summary>Отношение времени QuickSort ко времени LSD-сортировки.</summary>
    public double QuickToRadix => RadixMs > 0 ? QuickMs / RadixMs : 0;

    /// <summary>Отношение времени QuickSort ко времени ABC-сортировки.</summary>
    public double QuickToAbc => AbcMs > 0 ? QuickMs / AbcMs : 0;
}

/// <summary>
/// Эксперименты задания 3: замеры времени трёх алгоритмов на текстах разной длины
/// и разовый прогон всех алгоритмов для интерактивной части страницы.
/// </summary>
/// <remarks>
/// Один и тот же текст подаётся всем алгоритмам (они всё равно копируют массив внутрь),
/// поэтому времена сравнимы между собой. Замер повторяется несколько раз, а в таблицу
/// идёт среднее: в Blazor WebAssembly отдельный замер легко искажают сборка мусора
/// и работа браузера, и одно число без усреднения было бы нестабильным.
/// </remarks>
public static class WordBenchmark
{
    /// <summary>
    /// Обязательный набор размеров из задания. Эти точки присутствуют в таблице
    /// всегда (если не превышают заданный максимум) — по ним и строится график.
    /// </summary>
    public static readonly int[] RequiredSizes = [100, 500, 1000, 2000, 5000];

    /// <summary>Название алгоритма для интерфейса.</summary>
    public const string QuickName = "Быстрая сортировка (QuickSort)";

    /// <summary>Название алгоритма для интерфейса.</summary>
    public const string RadixName = "Поразрядная LSD-сортировка";

    /// <summary>Название алгоритма для интерфейса.</summary>
    public const string AbcName = "ABC-сортировка (MSD)";

    /// <summary>
    /// Строит список размеров для экспериментов: обязательные точки плюс равномерный шаг.
    /// </summary>
    /// <remarks>
    /// Благодаря объединению двух наборов таблица всегда содержит точки 100, 500, 1000,
    /// 2000 и 5000 (таково требование задания), а при большом максимуме добавляет
    /// ровные шаги, чтобы график не «ломался» на редких точках.
    /// </remarks>
    public static IReadOnlyList<int> BuildSizes(int maxWords, int step)
    {
        var sizes = new SortedSet<int>();

        foreach (int size in RequiredSizes)
            if (size <= maxWords) sizes.Add(size);

        if (step > 0)
            for (int n = step; n < maxWords; n += step) sizes.Add(n);

        sizes.Add(maxWords);
        sizes.RemoveWhere(n => n <= 0);

        return sizes.ToArray();
    }

    /// <summary>
    /// Прогоняет все три алгоритма на готовом массиве слов и возвращает подробные замеры.
    /// </summary>
    /// <param name="words">Разобранные слова текста.</param>
    /// <param name="runs">Сколько раз повторять каждый алгоритм (в таблицу идёт среднее).</param>
    public static IReadOnlyList<WordSortRun> Compare(IReadOnlyList<string> words, int runs = 1)
    {
        var data = words.ToArray();
        var reference = WordFrequencyCounter.BuildReference(data);
        int repeats = Math.Max(1, runs);

        return
        [
            MeasureOne(QuickName, data, reference, repeats, WordSortAlgorithms.QuickSort),
            MeasureOne(RadixName, data, reference, repeats, WordSortAlgorithms.RadixLsdSort),
            MeasureOne(AbcName, data, reference, repeats, WordSortAlgorithms.AbcSort)
        ];
    }

    /// <summary>
    /// Замеряет один размер текста для таблицы экспериментов.
    /// </summary>
    /// <param name="wordCount">Длина текста в словах.</param>
    /// <param name="runs">Число запусков на точку.</param>
    /// <param name="seed">Зерно генератора текста (одно и то же для всех точек).</param>
    public static WordBenchRow Measure(int wordCount, int runs = 3, int seed = 20260404)
    {
        var text = WordTextProvider.Generate(wordCount, seed);
        var words = WordTextProvider.SplitWords(text);
        var runs_ = Compare(words, runs);

        var frequencies = runs_[0].Frequencies;

        return new WordBenchRow
        {
            Words = words.Length,
            QuickMs = runs_[0].Milliseconds,
            RadixMs = runs_[1].Milliseconds,
            AbcMs = runs_[2].Milliseconds,
            DistinctWords = frequencies.DistinctWords,
            AllCorrect = runs_.All(run => run.IsCorrect)
        };
    }

    /// <summary>Запускает один алгоритм нужное число раз и берёт среднее время.</summary>
    private static WordSortRun MeasureOne(
        string name,
        string[] data,
        string[] reference,
        int repeats,
        Func<string[], WordSortResult> algorithm)
    {
        double total = 0;
        WordSortResult last = new() { Sorted = Array.Empty<string>() };

        for (int i = 0; i < repeats; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            last = algorithm(data);
            stopwatch.Stop();

            total += stopwatch.Elapsed.TotalMilliseconds;
        }

        return new WordSortRun
        {
            Name = name,
            Sorted = last.Sorted,
            Counters = last,
            Milliseconds = total / repeats,
            IsCorrect = WordFrequencyCounter.MatchesReference(last.Sorted, reference),
            Frequencies = WordFrequencyCounter.Count(last.Sorted)
        };
    }
}
