namespace AlgoLab.Labs.Lab4.Words;

/// <summary>
/// Одно слово и сколько раз оно встретилось в тексте.
/// </summary>
/// <remarks>
/// Тип неизменяемый: записи частотной таблицы только читаются интерфейсом,
/// поэтому их можно безопасно передавать в разметку и в отчёт.
/// </remarks>
public sealed record WordFrequency(string Word, int Count)
{
    /// <summary>Доля слова в тексте в процентах (для наглядности в таблице).</summary>
    public double Percent { get; init; }
}

/// <summary>
/// Итог подсчёта частот: полный список «слово — количество» и короткая сводка.
/// </summary>
/// <remarks>
/// Отдельная сводка нужна потому, что полный список может содержать сотни строк,
/// а для отчёта и для ответа на вопрос «сколько различных слов в тексте» достаточно
/// нескольких чисел. Счётчик сравнений показывает, что подсчёт по отсортированному
/// массиву — это один проход и n−1 сравнение соседей, без хеш-таблиц.
/// </remarks>
public sealed record WordFrequencyReport
{
    /// <summary>Пары «слово — количество» в лексикографическом порядке слов.</summary>
    public required IReadOnlyList<WordFrequency> Items { get; init; }

    /// <summary>Сколько всего слов в тексте.</summary>
    public int TotalWords { get; init; }

    /// <summary>Сколько различных слов в тексте.</summary>
    public int DistinctWords { get; init; }

    /// <summary>Самое частое слово (пусто, если слов нет).</summary>
    public string MostFrequentWord { get; init; } = "";

    /// <summary>Сколько раз встретилось самое частое слово.</summary>
    public int MostFrequentCount { get; init; }

    /// <summary>Сравнений при подсчёте: по одному на каждую пару соседних слов.</summary>
    public long Comparisons { get; init; }
}

/// <summary>
/// Подсчёт частот слов по уже отсортированному массиву.
/// </summary>
/// <remarks>
/// Главная идея задания: сортировка нужна не сама по себе, а чтобы одинаковые слова
/// встали рядом. Тогда подсчёт превращается в один проход: пока текущее слово совпадает
/// с предыдущим — увеличиваем счётчик, иначе заводим новую строку результата.
/// Альтернатива — словарь (HashMap), но он требует хеширования и не даёт
/// упорядоченного вывода без дополнительной сортировки ключей.
/// </remarks>
public static class WordFrequencyCounter
{
    /// <summary>
    /// Группирует одинаковые слова в отсортированном массиве и считает их количество.
    /// </summary>
    /// <param name="sortedWords">
    /// Массив слов в лексикографическом порядке. Если порядок нарушен, результат
    /// будет содержать повторы одинаковых слов — это честный признак неверной сортировки.
    /// </param>
    public static WordFrequencyReport Count(IReadOnlyList<string> sortedWords)
    {
        var items = new List<WordFrequency>();
        long comparisons = 0;

        if (sortedWords.Count == 0)
        {
            return new WordFrequencyReport
            {
                Items = items,
                TotalWords = 0,
                DistinctWords = 0,
                Comparisons = 0
            };
        }

        string current = sortedWords[0];
        int count = 1;

        for (int i = 1; i < sortedWords.Count; i++)
        {
            // Одно сравнение на каждую пару соседей — вся «цена» подсчёта.
            comparisons++;

            if (string.Equals(sortedWords[i], current, StringComparison.Ordinal))
            {
                count++;
            }
            else
            {
                items.Add(new WordFrequency(current, count));
                current = sortedWords[i];
                count = 1;
            }
        }

        items.Add(new WordFrequency(current, count));

        // Проценты считаем после группировки: для каждой записи нужно общее число слов.
        int total = sortedWords.Count;
        var withPercent = new List<WordFrequency>(items.Count);
        string mostWord = items[0].Word;
        int mostCount = items[0].Count;

        foreach (var item in items)
        {
            withPercent.Add(item with { Percent = total > 0 ? 100.0 * item.Count / total : 0 });

            if (item.Count > mostCount)
            {
                mostCount = item.Count;
                mostWord = item.Word;
            }
        }

        return new WordFrequencyReport
        {
            Items = withPercent,
            TotalWords = total,
            DistinctWords = withPercent.Count,
            MostFrequentWord = mostWord,
            MostFrequentCount = mostCount,
            Comparisons = comparisons
        };
    }

    /// <summary>
    /// Сверяет результат сортировки с эталоном — <see cref="Array.Sort{T}(T[], IComparer{T})"/>
    /// с ординальным сравнением строк.
    /// </summary>
    /// <remarks>
    /// Эталон намеренно взят из стандартной библиотеки: он заведомо правильный и не зависит
    /// от нашего кода. Пустой список расхождений означает, что алгоритм дал тот же порядок,
    /// что и .NET, — именно это и проверяет индикатор «проверка пройдена».
    /// </remarks>
    public static bool MatchesReference(IReadOnlyList<string> actual, IReadOnlyList<string> reference)
    {
        if (actual.Count != reference.Count) return false;

        for (int i = 0; i < actual.Count; i++)
            if (!string.Equals(actual[i], reference[i], StringComparison.Ordinal))
                return false;

        return true;
    }

    /// <summary>Эталонная сортировка для проверки корректности.</summary>
    public static string[] BuildReference(IEnumerable<string> words)
    {
        var copy = words.ToArray();
        Array.Sort(copy, StringComparer.Ordinal);
        return copy;
    }
}
