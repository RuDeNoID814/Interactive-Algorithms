namespace AlgoLab.Labs.Lab4.Part2_ExternalSorting;

/// <summary>
/// Настройки запуска внешней сортировки.
/// </summary>
public sealed class ExternalSortSettings
{
    /// <summary>Выбранный метод слияния.</summary>
    public ExternalSortMethod Method { get; init; } = ExternalSortMethod.Direct;

    /// <summary>Сколько записей помещается в оперативную память за один раз.</summary>
    public int MemorySize { get; init; } = 4;

    /// <summary>Сколько файлов сливается за один проход (для многопутевого метода).</summary>
    public int Ways { get; init; } = 3;
}

/// <summary>
/// Методы внешней сортировки слиянием: прямое, естественное и многопутевое.
/// </summary>
/// <remarks>
/// Все три метода устроены одинаково в главном: файл, который не помещается в память,
/// режут на части, каждую часть сортируют отдельно, а потом части сливаются. Отличие
/// только в том, как выбирается размер части и сколько частей сливается за один проход.
/// <list type="bullet">
/// <item>Прямое слияние: части по <see cref="ExternalSortSettings.MemorySize"/> записей.</item>
/// <item>Естественное слияние: части — это уже упорядоченные участки файла.</item>
/// <item>Многопутевое: за проход сливается k файлов, поэтому проходов нужно в log(k) раз меньше.</item>
/// </list>
/// </remarks>
public static class ExternalSortAlgorithms
{
    /// <summary>Выполняет внешнюю сортировку, записывая каждый шаг в трассировщик.</summary>
    public static void Run(ExternalSortTracer tracer, ExternalSortSettings settings)
    {
        if (tracer.Source.Records.Count < 2)
        {
            tracer.Phase("В файле меньше двух записей — сортировать нечего.");
            tracer.Done(tracer.Source, "Результат совпадает с исходным файлом.");
            return;
        }

        var runs = settings.Method == ExternalSortMethod.Natural
            ? BuildNaturalRuns(tracer)
            : BuildFixedRuns(tracer, settings.MemorySize);

        if (runs.Count == 1)
        {
            tracer.Phase("Получился всего один отрезок: файл целиком уместился в память. "
                       + "Слияние не требуется, файл уже упорядочен.");
            tracer.Done(runs[0], $"{runs[0].Name} уже содержит все записи в нужном порядке.");
            return;
        }

        int ways = settings.Method == ExternalSortMethod.Multiway ? Math.Max(2, settings.Ways) : 2;
        var current = runs;
        int pass = 0;

        while (current.Count > 1)
        {
            pass++;
            tracer.Phase($"Проход слияния №{pass}: сливаем по {ways} файлов. "
                       + $"Сейчас файлов: {current.Count}.");

            var next = new List<ExternalVirtualFile>();
            int groupIndex = 0;

            for (int start = 0; start < current.Count; start += ways)
            {
                var group = current.Skip(start).Take(ways).ToList();
                groupIndex++;

                // Последнему файлу может не хватить пары: переносим его в следующий проход как есть.
                if (group.Count == 1)
                {
                    tracer.Note($"Файл {group[0].Name} остался без пары — переносим его в следующий проход целиком.");
                    next.Add(group[0]);
                    continue;
                }

                var output = tracer.CreateFile(
                    $"merge-p{pass}-{groupIndex:00}.dat",
                    $"слияние, проход {pass}",
                    showTail: true);

                MergeGroup(tracer, group, output);

                foreach (var input in group)
                {
                    tracer.MarkCompleted(input);
                    tracer.RemoveFile(input);
                }

                next.Add(output);
            }

            current = next;
        }

        tracer.Done(current[0], $"Результат готов: файл {current[0].Name} содержит все {current[0].Records.Count} "
                              + "записей в порядке возрастания ключа.");    }

    // =========================================
    // ЭТАП 1: СОЗДАНИЕ ОТРЕЗКОВ
    // =========================================

    /// <summary>
    /// Прямое слияние: читает файл подряд и режет его на отрезки ровно по размеру памяти.
    /// </summary>
    private static List<ExternalVirtualFile> BuildFixedRuns(ExternalSortTracer tracer, int memorySize)
    {
        tracer.Phase($"Этап 1. Читаем source.dat и режем файл на отрезки по {memorySize} записей — "
                   + "столько помещается в память за один раз.");

        var source = tracer.Source;
        var runs = new List<ExternalVirtualFile>();
        var buffer = new List<ExternalSortRecord>();
        int runNumber = 0;

        while (source.Remaining > 0)
        {
            var next = source.Records[source.ReadPosition];
            var record = tracer.ReadNext(source,
                $"Читаем запись со строки {next.Number} из source.dat в память: ключ {tracer.Column.ValueOf(next)}.");

            buffer.Add(record);

            if (buffer.Count < memorySize && source.Remaining > 0) continue;

            runNumber++;
            runs.Add(WriteRun(tracer, buffer, runNumber, "прямое слияние"));
            buffer = new List<ExternalSortRecord>();
        }

        if (buffer.Count > 0)
        {
            runNumber++;
            runs.Add(WriteRun(tracer, buffer, runNumber, "прямое слияние"));
        }

        tracer.Note($"Отрезков получилось {runs.Count}. Теперь будем сливать их попарно.");
        return runs;
    }

    /// <summary>
    /// Естественное слияние: границами отрезков служат уже упорядоченные участки файла.
    /// </summary>
    /// <remarks>
    /// Метод сначала «прощупывает» файл и находит места, где порядок нарушается. Если файл
    /// почти отсортирован, участков окажется мало, и вся работа сведётся к одному-двум
    /// проходам слияния. На случайных данных упорядоченные участки короткие (в среднем
    /// по два элемента), и выигрыша не будет — это честный результат эксперимента.
    /// </remarks>
    private static List<ExternalVirtualFile> BuildNaturalRuns(ExternalSortTracer tracer)
    {
        tracer.Phase("Этап 1. Ищем в source.dat уже упорядоченные участки: границей отрезка "
                   + "служит первое место, где порядок нарушается.");

        var source = tracer.Source;
        var runs = new List<ExternalVirtualFile>();
        int runNumber = 0;
        var buffer = new List<ExternalSortRecord>();
        ExternalSortRecord? previous = null;

        while (source.Remaining > 0)
        {
            var next = source.Records[source.ReadPosition];
            var record = tracer.ReadNext(source,
                $"Читаем строку {next.Number} (ключ {tracer.Column.ValueOf(next)}) и смотрим, "
                + "продолжается ли возрастание.");

            if (previous is not null)
            {
                int comparison = tracer.CompareKeys(previous, record,
                    $"Сравниваем ключи {tracer.Column.ValueOf(previous)} и {tracer.Column.ValueOf(record)}: "
                    + "продолжается ли возрастание?");

                if (comparison > 0)
                {
                    // Порядок нарушился — предыдущий отрезок закончился.
                    runNumber++;
                    runs.Add(WriteRun(tracer, buffer, runNumber, "естественное слияние"));
                    buffer = new List<ExternalSortRecord>();

                    tracer.Note($"Порядок нарушен — отрезок №{runNumber} закрыт, начинаем новый.");
                }
            }

            buffer.Add(record);
            previous = record;
        }

        runNumber++;
        runs.Add(WriteRun(tracer, buffer, runNumber, "естественное слияние"));

        tracer.Note($"Упорядоченных участков нашлось {runs.Count}.");
        return runs;
    }

    /// <summary>
    /// Сортирует буфер в памяти, записывает его в новый файл-отрезок и сообщает трассировщику
    /// число сравнений, потраченных на сортировку.
    /// </summary>
    private static ExternalVirtualFile WriteRun(
        ExternalSortTracer tracer,
        List<ExternalSortRecord> buffer,
        int runNumber,
        string methodName)
    {
        var file = tracer.CreateFile($"run-{runNumber:00}.dat", $"отрезок №{runNumber}", showTail: true);

        int comparisons = SortInMemory(buffer, tracer.Column);

        foreach (var record in buffer)
        {
            tracer.WriteRecord(file, record,
                $"Записываем ключ {tracer.Column.ValueOf(record)} (строка исходного файла {record.Number}) "
                + $"в {file.Name}.");
        }

        string first = buffer.Count > 0 ? tracer.Column.ValueOf(buffer[0]) : "—";
        string last = buffer.Count > 0 ? tracer.Column.ValueOf(buffer[^1]) : "—";

        tracer.RunCreated(file, comparisons,
            $"Отрезок №{runNumber} готов ({methodName}): {buffer.Count} записей, "
            + $"ключи от {first} до {last}. Сравнений при сортировке в памяти: {comparisons}.");

        return file;
    }

    /// <summary>
    /// Сортирует небольшой набор записей, поместившийся в память.
    /// </summary>
    /// <remarks>
    /// Используется устойчивая сортировка слиянием: она предсказуемо работает за
    /// m·log(m) сравнений даже на «плохих» данных и сохраняет порядок записей с равными
    /// ключами. Число сравнений возвращается, чтобы попасть в общий счётчик.
    /// </remarks>
    private static int SortInMemory(List<ExternalSortRecord> records, ExternalSortColumn column)
    {
        if (records.Count < 2) return 0;

        int comparisons = 0;
        var buffer = new ExternalSortRecord[records.Count];
        MergeSortRange(records, buffer, 0, records.Count - 1, column, ref comparisons);
        return comparisons;
    }

    /// <summary>Рекурсивная часть сортировки слиянием в памяти.</summary>
    private static void MergeSortRange(
        List<ExternalSortRecord> records,
        ExternalSortRecord[] buffer,
        int left,
        int right,
        ExternalSortColumn column,
        ref int comparisons)
    {
        if (left >= right) return;

        int middle = left + (right - left) / 2;
        MergeSortRange(records, buffer, left, middle, column, ref comparisons);
        MergeSortRange(records, buffer, middle + 1, right, column, ref comparisons);

        int i = left, j = middle + 1, k = left;
        while (i <= middle && j <= right)
        {
            comparisons++;
            // Строгое «меньше или равно»: при равных ключах первым идёт левая запись,
            // поэтому сортировка остаётся устойчивой.
            if (column.Compare(records[i], records[j]) <= 0) buffer[k++] = records[i++];
            else buffer[k++] = records[j++];
        }

        while (i <= middle) buffer[k++] = records[i++];
        while (j <= right) buffer[k++] = records[j++];

        for (int t = left; t <= right; t++) records[t] = buffer[t];
    }

    // =========================================
    // ЭТАП 2: СЛИЯНИЕ
    // =========================================

    /// <summary>
    /// Сливает несколько упорядоченных файлов в один.
    /// </summary>
    /// <remarks>
    /// Из каждого входного файла читается только одна запись — его «голова». Затем
    /// головы сравниваются между собой, и наименьшая уходит в результат. После этого
    /// из того же файла подтягивается следующая запись. Так файлы просматриваются
    /// строго последовательно, как того и требует внешняя сортировка: вернуться назад
    /// к уже прочитанной записи нельзя.
    /// </remarks>
    private static void MergeGroup(
        ExternalSortTracer tracer,
        IReadOnlyList<ExternalVirtualFile> inputs,
        ExternalVirtualFile output)
    {
        var names = string.Join(", ", inputs.Select(file => file.Name));
        tracer.MergeStarted(inputs, output,
            $"Сливаем {inputs.Count} файла ({names}) в {output.Name}.");

        var heads = new ExternalSortRecord?[inputs.Count];

        for (int i = 0; i < inputs.Count; i++)
        {
            if (inputs[i].Remaining == 0)
            {
                heads[i] = null;
                continue;
            }

            heads[i] = tracer.ReadNext(inputs[i],
                $"Берём первую запись файла {inputs[i].Name}: ключ {tracer.Column.ValueOf(inputs[i].Records[inputs[i].ReadPosition])}.");
        }

        while (heads.Any(head => head is not null))
        {
            int best = -1;

            for (int i = 0; i < heads.Length; i++)
            {
                if (heads[i] is null) continue;

                if (best < 0)
                {
                    best = i;
                    continue;
                }

                int comparison = tracer.CompareKeys(heads[best]!, heads[i]!,
                    $"Сравниваем головы файлов {inputs[best].Name} и {inputs[i].Name}: "
                    + $"{tracer.Column.ValueOf(heads[best]!)} и {tracer.Column.ValueOf(heads[i]!)}. "
                    + "Меньшая запись уйдёт в результат.");

                if (comparison > 0) best = i;
            }

            var chosen = heads[best]!;
            tracer.WriteRecord(output, chosen,
                $"Ключ {tracer.Column.ValueOf(chosen)} — наименьший из голов, записываем его в {output.Name}.");

            if (inputs[best].Remaining > 0)
            {
                heads[best] = tracer.ReadNext(inputs[best],
                    $"Подтягиваем следующую запись из {inputs[best].Name}.");
            }
            else
            {
                heads[best] = null;
                tracer.Note($"Файл {inputs[best].Name} прочитан до конца.");
            }
        }

        tracer.Note($"Слияние в {output.Name} закончено: {output.Records.Count} записей.");
    }
}
