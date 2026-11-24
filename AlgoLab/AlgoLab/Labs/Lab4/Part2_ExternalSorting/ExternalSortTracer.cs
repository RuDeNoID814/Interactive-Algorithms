namespace AlgoLab.Labs.Lab4.Part2_ExternalSorting;

/// <summary>
/// Метод внешней сортировки, выбранный пользователем.
/// </summary>
public enum ExternalSortMethod
{
    /// <summary>Прямое слияние: файл режется на отрезки по размеру памяти.</summary>
    Direct,

    /// <summary>Естественное слияние: границами отрезков служат уже упорядоченные участки.</summary>
    Natural,

    /// <summary>Многопутевое слияние: за один проход сливаются k файлов, а не два.</summary>
    Multiway
}

/// <summary>
/// «Файл» во время демонстрации: то, что алгоритм читает и пишет.
/// </summary>
/// <remarks>
/// Настоящие файлы в браузере не нужны и даже вредны: чтобы показать каждый шаг,
/// пришлось бы сотни раз обращаться к диску. Поэтому содержимое файлов живёт
/// в памяти, а интерфейс рисует их как карточки с записями. Логика сортировки
/// при этом остаётся честной: файл читается последовательно, запись идёт только
/// в конец, а слияние сравнивает лишь текущие головы файлов.
/// </remarks>
public sealed class ExternalVirtualFile
{
    private readonly List<ExternalSortRecord> _records;

    public ExternalVirtualFile(string name, string role, IEnumerable<ExternalSortRecord> records, bool showTail)
    {
        Name = name;
        Role = role;
        _records = new List<ExternalSortRecord>(records);
        ShowTail = showTail;
    }

    /// <summary>Имя файла на схеме.</summary>
    public string Name { get; }

    /// <summary>Роль файла: исходный, отрезок, слияние, результат.</summary>
    public string Role { get; private set; }

    /// <summary>Объявляет файл результатом сортировки: роль меняется на «результат».</summary>
    public void MarkAsResult() => Role = "результат";

    /// <summary>Содержимое файла целиком.</summary>
    public IReadOnlyList<ExternalSortRecord> Records => _records;

    /// <summary>Сколько записей уже прочитано (остальные ещё лежат в файле).</summary>
    public int ReadPosition { get; set; }

    /// <summary>Показывать начало файла (отрезки) или его конец (растущая запись).</summary>
    public bool ShowTail { get; }

    /// <summary>Файл прочитан до конца.</summary>
    public bool Completed { get; set; }

    /// <summary>Файл участвует в текущем действии.</summary>
    public bool IsActive { get; set; }

    /// <summary>Записи, которые ещё не прочитаны.</summary>
    public int Remaining => Records.Count - ReadPosition;

    /// <summary>
    /// Дописывает запись в конец файла.
    /// </summary>
    /// <remarks>
    /// Снимки шагов хранят ссылку на <see cref="ExternalFileView"/>, а не на сам файл,
    /// поэтому дописывание в конец безопасно: уже сохранённые шаги не меняются.
    /// </remarks>
    public void Append(ExternalSortRecord record) => _records.Add(record);
}

/// <summary>
/// «Приборная панель» внешней сортировки: файлы, счётчики и лог шагов.
/// </summary>
/// <remarks>
/// Идея та же, что у <see cref="Sorting.SortTracer"/> во внутренних сортировках:
/// алгоритм ничего не знает о визуализации и только сообщает трассировщику,
/// что прочитал запись, что записал и с чем сравнил. Трассировщик уже сам
/// запоминает снимок всех файлов, поэтому любой шаг можно показать и отмотать назад.
/// </remarks>
public sealed class ExternalSortTracer
{
    /// <summary>Предохранитель от лавины шагов на больших таблицах.</summary>
    public const int MaxSteps = 4000;

    /// <summary>Сколько записей одного файла показывать на шаге.</summary>
    private const int MaxRecordsShown = 12;

    private readonly ExternalSortColumn _column;
    private readonly List<ExternalVirtualFile> _files = new();
    private readonly List<ExternalSortStep> _steps = new();
    private readonly HashSet<int> _activeNumbers = new();

    public ExternalSortTracer(ExternalSortTable table, ExternalSortColumn column)
    {
        _column = column;

        var source = new ExternalVirtualFile("source.dat", "исходный файл", table.Records, showTail: false);
        _files.Add(source);

        Note($"Прочитали файл: {table.Records.Count} записей, {table.Headers.Length} полей, "
           + $"разделитель — {table.Separator}.");
        Note($"Сортируем по полю «{column.Name}» ({column.KindLabel}), {column.DirectionLabel}.");
    }

    /// <summary>Исходный файл таблицы.</summary>
    public ExternalVirtualFile Source => _files[0];

    /// <summary>Выбранный ключ сортировки.</summary>
    public ExternalSortColumn Column => _column;

    /// <summary>Лог шагов.</summary>
    public IReadOnlyList<ExternalSortStep> Steps => _steps;

    /// <summary>Признак того, что лог обрезан предохранителем.</summary>
    public bool Truncated { get; private set; }

    /// <summary>
    /// Тихий режим: шаги не запоминаются, считаются только счётчики.
    /// Используется в замерах, где нужны сравнения и проходы, а не картинка.
    /// </summary>
    public bool Silent { get; init; }

    /// <summary>Сколько раз сравнивали ключи.</summary>
    public int Comparisons { get; private set; }

    /// <summary>Сколько записей прочитано.</summary>
    public int Reads { get; private set; }

    /// <summary>Сколько записей записано.</summary>
    public int Writes { get; private set; }

    /// <summary>Сколько отрезков создано.</summary>
    public int RunsCreated { get; private set; }

    /// <summary>Сколько проходов слияния выполнено.</summary>
    public int Passes { get; private set; }

    // =========================================
    // ФАЙЛЫ
    // =========================================

    /// <summary>Создаёт файл и сразу попадает в лог.</summary>
    public ExternalVirtualFile CreateFile(string name, string role, bool showTail)
    {
        var file = new ExternalVirtualFile(name, role, Array.Empty<ExternalSortRecord>(), showTail);
        _files.Add(file);
        return file;
    }
    /// <summary>Убирает файл со схемы: он своё дело сделал.</summary>
    /// <remarks>
    /// Из уже сохранённых шагов файл не исчезает — там лежат готовые снимки.
    /// Удаление влияет только на следующие шаги и не даёт схеме разрастись.
    /// </remarks>
    public void RemoveFile(ExternalVirtualFile file) => _files.Remove(file);

    /// <summary>Отмечает файл прочитанным до конца.</summary>
    public void MarkCompleted(ExternalVirtualFile file) => file.Completed = true;

    // =========================================
    // ШАГИ
    // =========================================

    /// <summary>Пояснение, которое не меняет файлы.</summary>
    public void Note(string message) => Capture(ExternalStepKind.Note, message, Array.Empty<ExternalVirtualFile>());

    /// <summary>Начало нового этапа работы.</summary>
    public void Phase(string message) => Capture(ExternalStepKind.Phase, message, Array.Empty<ExternalVirtualFile>());

    /// <summary>Создан новый файл-отрезок.</summary>
    public void RunCreated(ExternalVirtualFile file, int comparisonsInMemory, string message)
    {
        Comparisons += comparisonsInMemory;
        RunsCreated++;
        Capture(ExternalStepKind.CreateRun, message, new[] { file });
    }

    /// <summary>Читает очередную запись файла.</summary>
    public ExternalSortRecord ReadNext(ExternalVirtualFile file, string message)
    {
        if (file.ReadPosition >= file.Records.Count)
            throw new InvalidOperationException($"Файл {file.Name} уже прочитан до конца.");

        var record = file.Records[file.ReadPosition];
        file.ReadPosition++;
        Reads++;

        if (file.Remaining == 0) file.Completed = true;

        Capture(ExternalStepKind.Read, message, new[] { file }, record.Number);
        return record;
    }

    /// <summary>Дописывает запись в создаваемый файл.</summary>
    public void WriteRecord(ExternalVirtualFile file, ExternalSortRecord record, string message)
    {
        file.Append(record);
        Writes++;

        Capture(ExternalStepKind.Write, message, new[] { file }, record.Number);
    }

    /// <summary>Сравнивает ключи двух записей.</summary>
    public int CompareKeys(ExternalSortRecord left, ExternalSortRecord right, string message)
    {
        Comparisons++;
        Capture(ExternalStepKind.Compare, message,
            Array.Empty<ExternalVirtualFile>(), left.Number, right.Number);

        return _column.Compare(left, right);
    }

    /// <summary>Фиксирует начало слияния группы файлов.</summary>
    public void MergeStarted(IReadOnlyList<ExternalVirtualFile> inputs, ExternalVirtualFile output, string message)
    {
        Passes++;
        Capture(ExternalStepKind.Merge, message, inputs.Append(output).ToArray());
    }

    /// <summary>Сортировка завершена: показываем результат на отдельном файле.</summary>
    public void Done(ExternalVirtualFile result, string message)
    {
        result.MarkAsResult();
        Result = result;
        Capture(ExternalStepKind.Done, message, new[] { result });
    }

    /// <summary>Файл с результатом сортировки или <c>null</c>, если сортировка не завершена.</summary>
    public ExternalVirtualFile? Result { get; private set; }

    /// <summary>Сохраняет снимок состояния файлов в лог.</summary>
    private void Capture(
        ExternalStepKind kind,
        string message,
        ExternalVirtualFile[] active,
        params int[] activeNumbers)
    {
        if (Silent)
            return;

        if (_steps.Count >= MaxSteps)
        {
            Truncated = true;
            return;
        }

        _activeNumbers.Clear();
        foreach (int number in activeNumbers) _activeNumbers.Add(number);

        foreach (var file in _files) file.IsActive = active.Contains(file);

        var views = _files.Select(BuildView).ToArray();

        foreach (var file in _files) file.IsActive = false;

        _steps.Add(new ExternalSortStep
        {
            Kind = kind,
            Message = message,
            Files = views,
            Comparisons = Comparisons,
            RecordsRead = Reads,
            RecordsWritten = Writes,
            RunsCreated = RunsCreated,
            Passes = Passes
        });
    }

    /// <summary>Строит отображение одного файла для текущего шага.</summary>
    private ExternalFileView BuildView(ExternalVirtualFile file)
    {
        int total = file.Records.Count;
        int shown = Math.Min(total, MaxRecordsShown);

        // Растущий файл интересен своим концом, а читаемый — окрестностью курсора.
        int start = file.ShowTail
            ? Math.Max(0, total - shown)
            : Math.Clamp(file.ReadPosition - 2, 0, Math.Max(0, total - shown));

        var records = new List<ExternalRecordView>(shown);
        for (int i = start; i < total && records.Count < shown; i++)
        {
            var record = file.Records[i];
            records.Add(new ExternalRecordView
            {
                Number = record.Number,
                Key = _column.ValueOf(record),
                Label = BuildLabel(record),
                Active = file.IsActive && _activeNumbers.Contains(record.Number),
                Consumed = !file.ShowTail && i < file.ReadPosition
            });
        }

        return new ExternalFileView
        {
            Name = file.Name,
            Role = file.Role,
            Records = records,
            TotalRecords = total,
            Active = file.IsActive,
            Completed = file.Completed,
            Remaining = file.ShowTail ? 0 : file.Remaining
        };
    }

    /// <summary>Короткая подпись записи: первое поле, не совпадающее с ключом.</summary>
    private string BuildLabel(ExternalSortRecord record)
    {
        for (int i = 0; i < record.Fields.Length; i++)
        {
            if (i == _column.Index) continue;

            string value = record.Fields[i];
            if (string.IsNullOrWhiteSpace(value)) continue;

            return value.Length <= 24 ? value : value[..23] + "…";
        }

        return record.Number.ToString();
    }
}
