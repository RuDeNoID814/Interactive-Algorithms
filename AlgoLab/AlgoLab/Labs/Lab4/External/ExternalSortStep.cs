namespace AlgoLab.Labs.Lab4.External;

/// <summary>
/// Вид события в логе внешней сортировки.
/// </summary>
public enum ExternalStepKind
{
    /// <summary>Пояснение: ничего не читали и не писали.</summary>
    Note,

    /// <summary>Этап работы: подготовка, создание отрезков, слияние, результат.</summary>
    Phase,

    /// <summary>Создан новый файл-отрезок.</summary>
    CreateRun,

    /// <summary>Запись прочитана из файла в буфер.</summary>
    Read,

    /// <summary>Запись дописана в файл.</summary>
    Write,

    /// <summary>Сравнение ключей двух записей.</summary>
    Compare,

    /// <summary>Слияние пары файлов.</summary>
    Merge,

    /// <summary>Сортировка завершена.</summary>
    Done
}

/// <summary>
/// Одна запись глазами зрителя: что показывать в прямоугольнике на схеме файлов.
/// </summary>
public sealed class ExternalRecordView
{
    /// <summary>Номер записи в исходном файле.</summary>
    public int Number { get; init; }

    /// <summary>Значение ключевого поля.</summary>
    public string Key { get; init; } = "";

    /// <summary>Короткая подпись записи для наглядности.</summary>
    public string Label { get; init; } = "";

    /// <summary>Участвует ли запись в текущем действии (сравнение, чтение).</summary>
    public bool Active { get; init; }

    /// <summary>Запись уже прочитана из файла.</summary>
    public bool Consumed { get; init; }
}

/// <summary>
/// Снимок одного файла на конкретном шаге.
/// </summary>
public sealed class ExternalFileView
{
    /// <summary>Имя файла, например <c>run-01.dat</c>.</summary>
    public string Name { get; init; } = "";

    /// <summary>Роль файла: исходный, отрезок или результат.</summary>
    public string Role { get; init; } = "";

    /// <summary>Записи файла (возможно, показаны не все).</summary>
    public IReadOnlyList<ExternalRecordView> Records { get; init; } = Array.Empty<ExternalRecordView>();

    /// <summary>Сколько записей в файле всего.</summary>
    public int TotalRecords { get; init; }

    /// <summary>Участвует ли файл в текущем действии.</summary>
    public bool Active { get; init; }

    /// <summary>Файл уже прочитан до конца.</summary>
    public bool Completed { get; init; }

    /// <summary>Сколько записей ещё не прочитано (для файлов-источников).</summary>
    public int Remaining { get; init; }

    /// <summary>Сколько записей не поместилось в снимок.</summary>
    public int Hidden => Math.Max(0, TotalRecords - Records.Count);
}

/// <summary>
/// Один шаг внешней сортировки: пояснение, снимок всех файлов и счётчики.
/// </summary>
public sealed class ExternalSortStep
{
    /// <summary>Что произошло.</summary>
    public ExternalStepKind Kind { get; init; }

    /// <summary>Пояснение для человека.</summary>
    public string Message { get; init; } = "";

    /// <summary>Снимок файлов после шага.</summary>
    public IReadOnlyList<ExternalFileView> Files { get; init; } = Array.Empty<ExternalFileView>();

    /// <summary>Сколько раз сравнивали ключи.</summary>
    public int Comparisons { get; init; }

    /// <summary>Сколько записей прочитано из файлов.</summary>
    public int RecordsRead { get; init; }

    /// <summary>Сколько записей записано в файлы.</summary>
    public int RecordsWritten { get; init; }

    /// <summary>Сколько файлов-отрезков создано.</summary>
    public int RunsCreated { get; init; }

    /// <summary>Сколько проходов слияния выполнено.</summary>
    public int Passes { get; init; }
}
