using System.Text;
using System.Text.RegularExpressions;

namespace AlgoLab.Labs.Lab4.Words;

/// <summary>
/// Источник текста для задания 3: готовый осмысленный пример и генератор
/// английских текстов заданной длины с воспроизводимым «шумом» из слов.
/// </summary>
/// <remarks>
/// Английский язык выбран не случайно (так рекомендует и само задание): в нём мало
/// словоформ, поэтому различных слов в тексте немного, частоты получаются наглядными,
/// а таблица «слово — количество» — короткой. Генерация идёт от <see cref="Random"/> с явным
/// зерном, поэтому один и тот же размер текста всегда даёт один и тот же набор слов:
/// без этого повторные замеры времени нельзя было бы сравнивать между запусками.
/// </remarks>
public static partial class WordTextProvider
{
    /// <summary>Регулярное выражение «слово»: одна или несколько латинских букв подряд.</summary>
    /// <remarks>
    /// Всё, что не подходит под этот образец — цифры, знаки препинания, апострофы, буквы
    /// других алфавитов — автоматически становится разделителем: отдельные правила
    /// разбора для них писать не нужно.
    /// </remarks>
    [GeneratedRegex("[a-z]+")]
    private static partial Regex WordRegex();

    /// <summary>
    /// Осмысленный пример английского текста: рассказ о библиотеке алгоритмов.
    /// Служебные слова (the, and, of, a, to, in, is) повторяются, поэтому частоты
    /// в таблице получаются разными и по ним видно смысл подсчёта.
    /// </summary>
    public const string SampleText =
        "The library of algorithms is a quiet place where the students and the teachers meet. "
        + "A good algorithm is like a map of the city: it shows the way to the answer, and the way "
        + "is shorter than a random walk. The sorting of words is a small task, and a small task "
        + "shows the idea of the big one. First we split the text into words, then we put the words "
        + "in order, and then we count how many times each word is in the text. The count of a word "
        + "is the number of the times the word is in the text, and the same words go one after another "
        + "in the sorted array. A radix sort looks at the letters of a word, and a quick sort compares "
        + "the words as a whole. The text of a thousand words is a text of a thousand small steps, "
        + "and the time of the sort is the time of the steps of the algorithm.";

    /// <summary>Общий словарь генератора: обычные английские слова без словоформ.</summary>
    private static readonly string[] Vocabulary =
    [
        "time", "word", "text", "sort", "array", "order", "letter", "number", "value", "index",
        "line", "list", "table", "result", "method", "step", "task", "program", "student", "teacher",
        "school", "lesson", "book", "page", "city", "map", "way", "answer", "question", "idea",
        "part", "place", "point", "rule", "size", "speed", "state", "test", "count", "data",
        "memory", "level", "model", "name", "pair", "problem", "process", "range", "search", "second",
        "set", "shape", "sign", "space", "start", "stop", "string", "system", "theory", "thing",
        "tree", "type", "unit", "user", "value", "window", "work", "world", "year", "zero",
        "quick", "radix", "bucket", "digit", "compare", "group", "repeat", "group", "measure", "random",
        "fast", "slow", "small", "large", "first", "last", "next", "same", "other", "simple"
    ];

    /// <summary>
    /// Служебные слова английского языка: они повторяются гораздо чаще знаменательных,
    /// поэтому включаются в текст несколько раз — так частотный словарь становится
    /// похожим на настоящий, а не на список уникальных терминов.
    /// </summary>
    private static readonly string[] FunctionWords =
    [
        "the", "and", "of", "a", "to", "in", "is", "that", "it", "for",
        "on", "with", "as", "at", "be", "by", "not", "or", "from", "this"
    ];

    /// <summary>Разбирает текст на слова: только латинские буквы, нижний регистр.</summary>
    /// <remarks>
    /// Сначала текст приводится к нижнему регистру через <c>ToLowerInvariant</c>
    /// (инвариантная культура — чтобы результат не зависел от настроек браузера),
    /// а затем из него выбираются все последовательности букв. Такое разделение
    /// означает, что «Word,» и «word.» дают одно и то же слово «word».
    /// </remarks>
    public static string[] SplitWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        var matches = WordRegex().Matches(text.ToLowerInvariant());
        var words = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
            words[i] = matches[i].Value;

        return words;
    }

    /// <summary>
    /// Строит английский текст примерно из <paramref name="wordCount"/> слов.
    /// </summary>
    /// <remarks>
    /// Служебные слова выпадают в среднем в каждом четвёртом слове, остальные берутся
    /// из общего словаря, поэтому различных слов получается несколько сотен. Иногда
    /// в текст добавляются запятые и точки: они проверяют, что разбор на слова
    /// действительно игнорирует знаки препинания.
    /// </remarks>
    public static string Generate(int wordCount, int seed = 20260404)
    {
        if (wordCount <= 0) return "";

        var random = new Random(seed);
        var builder = new StringBuilder(wordCount * 7);

        for (int i = 0; i < wordCount; i++)
        {
            if (i > 0) builder.Append(' ');

            bool useFunctionWord = random.Next(100) < 25;
            string word = useFunctionWord
                ? FunctionWords[random.Next(FunctionWords.Length)]
                : Vocabulary[random.Next(Vocabulary.Length)];

            builder.Append(word);

            if (random.Next(100) < 8)
                builder.Append(',');
            else if (random.Next(100) < 4)
                builder.Append('.');
        }

        return builder.ToString();
    }
}
