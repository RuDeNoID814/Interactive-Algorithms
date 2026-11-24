namespace AlgoLab.Labs.Lab4.Part2_ExternalSorting;

/// <summary>
/// Одна строка таблицы замеров внешней сортировки.
/// </summary>
public sealed class ExternalBenchRow
{
    /// <summary>Сколько записей было в файле.</summary>
    public int Records { get; init; }

    /// <summary>Выбранный метод слияния.</summary>
    public string Method { get; init; } = "";

    /// <summary>Сколько файлов-отрезков пришлось создать.</summary>
    public int Runs { get; init; }

    /// <summary>Сколько проходов слияния выполнено.</summary>
    public int Passes { get; init; }

    /// <summary>Сколько раз сравнивались ключи.</summary>
    public int Comparisons { get; init; }

    /// <summary>Сколько записей прочитано из файлов.</summary>
    public int Reads { get; init; }

    /// <summary>Сколько записей записано в файлы.</summary>
    public int Writes { get; init; }

    /// <summary>Время работы, мс.</summary>
    public double TimeMs { get; init; }

    /// <summary>Результат проверен и совпал с эталоном.</summary>
    public bool Verified { get; init; }
}

/// <summary>
/// Замеры внешней сортировки: сравнения, проходы и время на разных объёмах файла.
/// </summary>
/// <remarks>
/// Замеры делаются на тех же готовых таблицах, но многократно повторённых до нужного
/// количества записей: так в файле остаётся «настоящая» структура данных, а не
/// бессмысленный поток одинаковых ключей. Каждый прогон проверяется по эталону —
/// отсортированному массиву ключей, поэтому в таблицу не попадут результаты с ошибкой.
/// </remarks>
public static class ExternalSortBenchmark
{
    /// <summary>Размеры файла для замера.</summary>
    public static readonly int[] Sizes = [50, 100, 200, 400, 800, 1600, 3200];

    /// <summary>Методы, которые сравниваются между собой.</summary>
    public static readonly ExternalSortMethod[] Methods =
    [
        ExternalSortMethod.Direct,
        ExternalSortMethod.Natural,
        ExternalSortMethod.Multiway
    ];

    /// <summary>Подпись метода для интерфейса и отчёта.</summary>
    public static string MethodName(ExternalSortMethod method) => method switch
    {
        ExternalSortMethod.Direct => "Прямое слияние",
        ExternalSortMethod.Natural => "Естественное слияние",
        _ => "Многопутевое слияние"
    };

    /// <summary>
    /// Строит таблицу нужного размера: повторяет записи образца, пока не наберётся count строк.
    /// </summary>
    public static ExternalSortTable BuildTable(ExternalSortTable sample, int count, int seed = 2024)
    {
        var records = new List<ExternalSortRecord>(count);
        var random = new Random(seed);
        var source = sample.Records;

        for (int i = 0; i < count; i++)
        {
            var original = source[i % source.Count];
            records.Add(new ExternalSortRecord(i + 1, original.Fields));
        }

        // Перемешиваем: файл для внешней сортировки не должен приходить упорядоченным.
        for (int i = records.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (records[i], records[j]) = (records[j], records[i]);
        }

        // После перемешивания нумеруем строки заново — как будто это настоящий файл.
        var renumbered = records
            .Select((record, index) => new ExternalSortRecord(index + 1, record.Fields))
            .ToList();

        return sample.WithRecords(renumbered);
    }

    /// <summary>
    /// Выполняет один замер: сортирует файл заданного размера выбранным методом.
    /// </summary>
    public static ExternalBenchRow Measure(
        ExternalSortTable sample,
        ExternalSortColumn column,
        ExternalSortMethod method,
        int records,
        int memorySize)
    {
        var table = BuildTable(sample, records);
        var tracer = new ExternalSortTracer(table, column) { Silent = true };

        var settings = new ExternalSortSettings
        {
            Method = method,
            MemorySize = memorySize,
            Ways = 3
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        ExternalSortAlgorithms.Run(tracer, settings);
        stopwatch.Stop();

        var result = tracer.Result?.Records ?? Array.Empty<ExternalSortRecord>();

        return new ExternalBenchRow
        {
            Records = records,
            Method = MethodName(method),
            Runs = tracer.RunsCreated,
            Passes = tracer.Passes,
            Comparisons = tracer.Comparisons,
            Reads = tracer.Reads,
            Writes = tracer.Writes,
            TimeMs = stopwatch.Elapsed.TotalMilliseconds,
            Verified = result.Count == records && IsSorted(result, column)
        };
    }

    /// <summary>
    /// Проверяет, что записи стоят в нужном порядке по выбранному ключу.
    /// </summary>
    public static bool IsSorted(IReadOnlyList<ExternalSortRecord> records, ExternalSortColumn column)
    {
        for (int i = 1; i < records.Count; i++)
        {
            if (column.Compare(records[i - 1], records[i]) > 0) return false;
        }

        return true;
    }
}
