namespace AlgoLab.Labs.Lab4.Words;

/// <summary>
/// Результат одного запуска сортировки слов: сам отсортированный массив
/// и счётчики операций, по которым в отчёте видно, «сколько стоила» сортировка.
/// </summary>
/// <remarks>
/// Счётчики нужны не для красоты: у сравнительных алгоритмов (QuickSort) основная
/// работа — сравнения строк, а у поразрядных (LSD и ABC) сравнений вообще нет —
/// вместо них просмотры символов и пересылки элементов. Один и тот же набор полей
/// позволяет честно показать эту разницу в интерфейсе.
/// </remarks>
public sealed record WordSortResult
{
    /// <summary>Отсортированный массив слов (лексикографический порядок).</summary>
    public required string[] Sorted { get; init; }

    /// <summary>Число сравнений строк или их частей (для поразрядных — 0).</summary>
    public long Comparisons { get; init; }

    /// <summary>Сколько символов алгоритм «посмотрел» (для QuickSort — 0).</summary>
    public long CharacterViews { get; init; }

    /// <summary>Число пересылок элементов: обмены, копирования, раскладка по корзинам.</summary>
    public long Moves { get; init; }
}

/// <summary>
/// Три алгоритма сортировки массива слов: быстрая сортировка Хоара (сравнительная)
/// и два поразрядных — LSD-сортировка и ABC-сортировка (MSD, «американский флаг»).
/// </summary>
/// <remarks>
/// Все методы — обычные статические функции над <see cref="string"/>[]: они не знают
/// ни про интерфейс, ни про замеры времени, поэтому их легко вызывать и со страницы,
/// и из кода экспериментов. Слова уже приведены к нижнему регистру и содержат
/// только латинские буквы — на это рассчитана вся разметка «конец строки» и корзин.
/// </remarks>
public static class WordSortAlgorithms
{
    /// <summary>
    /// Быстрая сортировка Хоара: участок делится опорным элементом на две части,
    /// указатели i и j идут навстречу и меняют местами «неправильные» пары.
    /// </summary>
    /// <remarks>
    /// Строки сравниваются посимвольно через <see cref="string.CompareOrdinal"/>:
    /// для латинских слов это в точности лексикографический порядок. Опорным берём
    /// средний элемент участка — на уже отсортированных и случайных текстах он делит
    /// массив примерно пополам, и квадратичного худшего случая почти не возникает.
    /// </remarks>
    public static WordSortResult QuickSort(string[] words)
    {
        var data = (string[])words.Clone();
        long comparisons = 0, moves = 0;

        void Sort(int left, int right)
        {
            if (left >= right) return;

            // Опорный — середина участка: он делит массив примерно пополам и на
            // отсортированных данных, поэтому худший случай O(n²) почти не встречается.
            string pivot = data[(left + right) / 2];
            int i = left, j = right;

            while (i <= j)
            {
                // Идём слева вправо, пока слово меньше опорного. Опорное слово стоит
                // внутри участка, поэтому просмотр гарантированно остановится сам —
                // выходить за границы массива не нужно.
                while (Compare(data[i], pivot) < 0) i++;

                // Идём справа влево, пока слово больше опорного.
                while (Compare(data[j], pivot) > 0) j--;

                if (i <= j)
                {
                    if (i != j)
                    {
                        (data[i], data[j]) = (data[j], data[i]);
                        moves += 2; // обмен — это две пересылки
                    }
                    i++;
                    j--;
                }
            }

            Sort(left, j);
            Sort(i, right);
        }

        int Compare(string a, string b)
        {
            comparisons++;
            return string.CompareOrdinal(a, b);
        }

        Sort(0, data.Length - 1);
        return new WordSortResult { Sorted = data, Comparisons = comparisons, Moves = moves };
    }

    /// <summary>
    /// Поразрядная LSD-сортировка («с младших разрядов») для строк.
    /// Слова выравниваются по длине: недостающие позиции считаем символом-«пустышкой»,
    /// который меньше любой буквы, поэтому короткие слова встают раньше длинных.
    /// </summary>
    /// <remarks>
    /// Каждый проход — устойчивая поразрядная сортировка подсчётом по одному символу:
    /// сначала считаем, сколько слов попадёт в каждую корзину (буква a–z или пустышка),
    /// затем по счётчикам вычисляем границы корзин и раскладываем слова во второй массив.
    /// Устойчивость обязательна: именно она гарантирует, что порядок, наведённый
    /// предыдущими проходами по младшим разрядам, не будет разрушен.
    /// </remarks>
    public static WordSortResult RadixLsdSort(string[] words)
    {
        if (words.Length == 0)
            return new WordSortResult { Sorted = Array.Empty<string>() };

        const int buckets = 27; // 26 букв + корзина «конец слова»
        var data = (string[])words.Clone();
        var buffer = new string[data.Length];
        var counts = new int[buckets];

        int maxLen = 0;
        foreach (var w in data)
            if (w.Length > maxLen) maxLen = w.Length;

        long views = 0, moves = 0;

        // Идём от последнего символа к первому: младшие разряды уже упорядочены,
        // и устойчивая сортировка по старшему разряду сохраняет этот порядок.
        for (int pos = maxLen - 1; pos >= 0; pos--)
        {
            Array.Clear(counts);

            foreach (var w in data)
            {
                counts[Bucket(w, pos)]++;
                views++;
            }

            // Превращаем счётчики в правые границы корзин: counts[b] — индекс,
            // на котором нужно положить СЛЕДУЮЩЕЕ слово корзины b.
            for (int b = 1; b < buckets; b++)
                counts[b] += counts[b - 1];

            // Обратный проход даёт устойчивость: равные ключи сохраняют прежний порядок.
            for (int i = data.Length - 1; i >= 0; i--)
            {
                int b = Bucket(data[i], pos);
                counts[b]--;
                buffer[counts[b]] = data[i];
                moves++;
                views++;
            }

            (data, buffer) = (buffer, data);
        }

        return new WordSortResult { Sorted = data, CharacterViews = views, Moves = moves };
    }

    /// <summary>
    /// ABC-сортировка (MSD-поразрядная, «американский флаг»): слова раскладываются
    /// по корзинам по ТЕКУЩЕМУ символу, начиная с первого, и каждая корзина
    /// сортируется рекурсивно по следующему символу.
    /// </summary>
    /// <remarks>
    /// Конец короткого слова — отдельная корзина с самым маленьким «номером»,
    /// поэтому слова-префиксы (например, «a» перед «and») оказываются раньше.
    /// Корзины формируются списками, а границы внутри массива вычисляются по счётчикам
    /// — так проще видеть структуру алгоритма, а пересылки учитываются при возврате
    /// слов из корзин в общий массив. На обычных текстах глубина рекурсии мала:
    /// чаще всего уже на 2–3 символе каждая корзина содержит одинаковые слова.
    /// </remarks>
    public static WordSortResult AbcSort(string[] words)
    {
        var data = (string[])words.Clone();
        long views = 0, moves = 0;

        Sort(data, 0, data.Length, 0);

        return new WordSortResult { Sorted = data, CharacterViews = views, Moves = moves };

        // Сортирует участок [from, to) массива по символу на позиции depth.
        void Sort(string[] a, int from, int to, int depth)
        {
            if (to - from <= 1) return;

            // 27 корзин: 0 — конец слова, 1..26 — буквы a..z.
            var buckets = new List<string>[27];
            for (int b = 0; b < buckets.Length; b++) buckets[b] = [];

            // Раскладываем слова участка по корзинам.
            for (int i = from; i < to; i++)
            {
                int b = depth < a[i].Length ? a[i][depth] - 'a' + 1 : 0;
                buckets[b].Add(a[i]);
                views++;
            }

            // Возвращаем корзины в массив по порядку: сначала слова, закончившиеся
            // на предыдущем шаге, затем буквы по алфавиту.
            int pos = from;
            foreach (var bucket in buckets)
            {
                foreach (var word in bucket)
                {
                    a[pos++] = word;
                    moves++;
                }
            }

            // Рекурсивно сортируем каждую корзину по следующему символу.
            // Нулевую корзину пропускаем: в ней лежат слова, которые уже закончились,
            // а значит полностью совпали между собой — сортировать в них нечего.
            // Без этого пропуска рекурсия никогда бы не остановилась.
            pos = from + buckets[0].Count;
            for (int b = 1; b < buckets.Length; b++)
            {
                if (buckets[b].Count > 1)
                    Sort(a, pos, pos + buckets[b].Count, depth + 1);
                pos += buckets[b].Count;
            }
        }
    }

    /// <summary>Номер корзины для символа на позиции <paramref name="pos"/>: 0 — конец слова.</summary>
    private static int Bucket(string word, int pos) =>
        pos < word.Length ? word[pos] - 'a' + 1 : 0;
}
