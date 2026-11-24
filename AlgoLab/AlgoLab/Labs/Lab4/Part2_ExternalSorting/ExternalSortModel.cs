using System.Globalization;

namespace AlgoLab.Labs.Lab4.Part2_ExternalSorting;

/// <summary>
/// Вид ключа: от него зависит, как программа сравнивает значения поля.
/// </summary>
public enum ExternalKeyKind
{
    /// <summary>Число: сравнение по величине (население, площадь, масса).</summary>
    Number,

    /// <summary>Текст: лексикографическое сравнение (название, столица, класс вещества).</summary>
    Text
}

/// <summary>
/// Поле таблицы, по которому можно сортировать записи.
/// </summary>
/// <remarks>
/// Тип ключа определяется автоматически при разборе файла: если во всех строках поле
/// удаётся прочитать как число, поле считается числовым. Ручного выбора типа нет
/// намеренно — так программа не может «перепутать» население со столицей.
/// </remarks>
public sealed class ExternalSortColumn
{
    /// <summary>Номер поля в строке таблицы (с нуля).</summary>
    public int Index { get; init; }

    /// <summary>Название поля из заголовка таблицы.</summary>
    public string Name { get; init; } = "";

    /// <summary>Вид ключа.</summary>
    public ExternalKeyKind Kind { get; init; }

    /// <summary>Направление: истина — по убыванию.</summary>
    public bool Descending { get; set; }

    /// <summary>Подпись вида ключа для интерфейса.</summary>
    public string KindLabel => Kind == ExternalKeyKind.Number ? "число" : "текст";

    /// <summary>Подпись направления для интерфейса.</summary>
    public string DirectionLabel => Descending ? "по убыванию" : "по возрастанию";

    /// <summary>Значение поля у записи — ровно то, что попадает в файл.</summary>
    public string ValueOf(ExternalSortRecord record)
    {
        int index = Index;
        return index >= 0 && index < record.Fields.Length ? record.Fields[index] : "";
    }

    /// <summary>
    /// Сравнивает две записи по этому полю. Ноль означает «ключи равны»,
    /// и тогда сортировка обязана сохранить исходный порядок записей (устойчивость).
    /// </summary>
    public int Compare(ExternalSortRecord a, ExternalSortRecord b)
    {
        int result = CompareValues(ValueOf(a), ValueOf(b));
        return Descending ? -result : result;
    }

    /// <summary>Сравнивает два значения поля.</summary>
    /// <remarks>
    /// Числовое поле сравнивается по величине. Если значение не разбирается как число
    /// (пустая ячейка, прочерк, «нет данных»), такие записи уходят в конец: для отчёта
    /// удобнее, когда строки с испорченным ключом не мешают упорядоченной части.
    /// </remarks>
    public int CompareValues(string left, string right)
    {
        if (Kind == ExternalKeyKind.Number)
        {
            bool leftIsNumber = TryParseNumber(left, out double leftNumber);
            bool rightIsNumber = TryParseNumber(right, out double rightNumber);

            if (leftIsNumber && rightIsNumber) return leftNumber.CompareTo(rightNumber);
            if (leftIsNumber) return -1;
            if (rightIsNumber) return 1;
        }

        // Текстовый ключ сравниваем посимвольно (Ordinal): результат не зависит
        // от языковых настроек браузера и всегда воспроизводим.
        return string.Compare(left, right, StringComparison.Ordinal);
    }

    /// <summary>Копия поля с другим направлением сортировки.</summary>
    public ExternalSortColumn WithDirection(bool descending) => new()
    {
        Index = Index,
        Name = Name,
        Kind = Kind,
        Descending = descending
    };

    /// <summary>
    /// Пробует прочитать значение как число: допускаются пробелы между разрядами,
    /// запятая или точка как десятичный разделитель.
    /// </summary>
    public static bool TryParseNumber(string value, out double number)
    {
        number = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;

        string cleaned = value
            .Replace("\u00A0", "")
            .Replace("\u202F", "")
            .Replace(" ", "")
            .Replace(',', '.');

        return double.TryParse(
            cleaned,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture,
            out number);
    }
}

/// <summary>
/// Одна запись таблицы: значения полей и номер строки в исходном файле.
/// </summary>
/// <remarks>
/// Номер строки нужен только для наглядности: в пояснениях и логе видно, откуда
/// запись «приехала» в результат, и можно убедиться, что слияние ничего не потеряло.
/// </remarks>
public sealed class ExternalSortRecord
{
    public ExternalSortRecord(int number, string[] fields)
    {
        Number = number;
        Fields = fields;
    }

    /// <summary>Номер записи в исходном файле (с единицы).</summary>
    public int Number { get; }

    /// <summary>Значения полей в порядке заголовка.</summary>
    public string[] Fields { get; }

    /// <summary>Запись в виде строки файла с указанным разделителем.</summary>
    public string ToLine(string separator) => string.Join(separator, Fields);
}

/// <summary>
/// Таблица, прочитанная из файла: заголовок, записи и список полей-кандидатов на ключ.
/// </summary>
public sealed class ExternalSortTable
{
    public ExternalSortTable(
        string[] headers,
        IReadOnlyList<ExternalSortRecord> records,
        IReadOnlyList<ExternalSortColumn> columns,
        string separator)
    {
        Headers = headers;
        Records = records;
        Columns = columns;
        Separator = separator;
    }

    /// <summary>Названия полей из первой строки файла.</summary>
    public string[] Headers { get; }

    /// <summary>Записи таблицы в исходном порядке.</summary>
    public IReadOnlyList<ExternalSortRecord> Records { get; }

    /// <summary>Поля, по которым можно сортировать.</summary>
    public IReadOnlyList<ExternalSortColumn> Columns { get; }

    /// <summary>Разделитель, найденный в файле.</summary>
    public string Separator { get; }

    /// <summary>Возвращает таблицу с подмножеством записей.</summary>
    public ExternalSortTable WithRecords(IReadOnlyList<ExternalSortRecord> records) =>
        new(Headers, records, Columns, Separator);

    /// <summary>Собирает текст файла из заголовка и записей.</summary>
    public string ToFileText(IReadOnlyList<ExternalSortRecord>? records = null)
    {
        var lines = new List<string> { string.Join(Separator, Headers) };
        foreach (var record in records ?? Records)
            lines.Add(record.ToLine(Separator));

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}

/// <summary>
/// Разбор файла с таблицей и определение типов полей.
/// </summary>
/// <remarks>
/// Формат намеренно простой, как в задании: первая строка — заголовок, дальше по строке
/// на запись. Разделителем может быть табуляция, точка с запятой, запятая или просто
/// пробелы — программа определяет его сама по заголовку. Количество полей произвольное.
/// </remarks>
public static class ExternalTableParser
{
    /// <summary>Разбирает текст файла в таблицу.</summary>
    /// <exception cref="FormatException">Файл пуст, в нём нет заголовка или нет записей.</exception>
    public static ExternalSortTable Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new FormatException("Файл пуст: нет ни заголовка, ни записей.");

        var lines = text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count < 2)
            throw new FormatException("В файле должна быть строка заголовка и хотя бы одна запись.");

        char separator = DetectSeparator(lines[0]);
        var headers = Split(lines[0], separator);
        if (headers.Length == 0)
            throw new FormatException("Не удалось прочитать заголовок таблицы.");

        var records = new List<ExternalSortRecord>();
        for (int i = 1; i < lines.Count; i++)
        {
            var cells = Split(lines[i], separator);
            var fields = new string[headers.Length];

            for (int column = 0; column < headers.Length; column++)
                fields[column] = column < cells.Length ? cells[column].Trim() : "";

            records.Add(new ExternalSortRecord(i, fields));
        }

        if (records.Count == 0)
            throw new FormatException("В файле нет ни одной записи.");

        var columns = BuildColumns(headers, records);
        return new ExternalSortTable(headers, records, columns, DisplaySeparator(separator));
    }

    /// <summary>
    /// Определяет разделитель по строке заголовка: табуляция, «;», «,» или пробелы.
    /// </summary>
    private static char DetectSeparator(string headerLine)
    {
        if (headerLine.Contains('\t')) return '\t';
        if (headerLine.Contains(';')) return ';';
        if (headerLine.Contains(',')) return ',';
        return ' ';
    }

    /// <summary>Разбивает строку на ячейки, убирая лишние пробелы.</summary>
    private static string[] Split(string line, char separator)
    {
        // Пробельный разделитель означает «одно или несколько подряд идущих пробелов»,
        // иначе названия из двух слов распались бы на отдельные поля.
        if (separator == ' ')
            return line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return line.Split(separator, StringSplitOptions.TrimEntries);
    }

    /// <summary>Разделитель в виде, пригодном для показа человеку.</summary>
    private static string DisplaySeparator(char separator) => separator switch
    {
        '\t' => "табуляция",
        ';' => ";",
        ',' => ",",
        _ => "пробелы"
    };

    /// <summary>Определяет тип каждого поля: число, если все значения читаются как числа.</summary>
    private static List<ExternalSortColumn> BuildColumns(
        string[] headers,
        IReadOnlyList<ExternalSortRecord> records)
    {
        var columns = new List<ExternalSortColumn>(headers.Length);

        for (int index = 0; index < headers.Length; index++)
        {
            bool allNumbers = true;
            bool hasValue = false;

            foreach (var record in records)
            {
                string value = index < record.Fields.Length ? record.Fields[index] : "";
                if (string.IsNullOrWhiteSpace(value)) continue;

                hasValue = true;
                if (!ExternalSortColumn.TryParseNumber(value, out _))
                {
                    allNumbers = false;
                    break;
                }
            }

            columns.Add(new ExternalSortColumn
            {
                Index = index,
                Name = headers[index],
                Kind = allNumbers && hasValue ? ExternalKeyKind.Number : ExternalKeyKind.Text
            });
        }

        return columns;
    }
}
