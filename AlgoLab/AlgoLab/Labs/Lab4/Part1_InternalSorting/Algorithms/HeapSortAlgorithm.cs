namespace AlgoLab.Labs.Lab4.Part1_InternalSorting;

/// <summary>
/// Пирамидальная сортировка (HeapSort) на бинарной пирамиде (max-heap).
/// </summary>
/// <remarks>
/// Идея в два этапа.
/// <list type="number">
/// <item>
/// Построение пирамиды: массив рассматривается как двоичное дерево, где у элемента i
/// дети находятся на позициях 2i+1 и 2i+2. Проходим по «родителям» справа налево и
/// «просеиваем» каждый вниз, пока в корне не окажется самый большой элемент.
/// </item>
/// <item>
/// Собственно сортировка: самый большой элемент всегда в корне (позиция 0). Меняем его
/// с последним элементом кучи — максимум встаёт на своё окончательное место, а размер
/// кучи уменьшается на единицу. Новый корень снова просеиваем вниз.
/// </item>
/// </list>
/// <para>
/// Сложность: Θ(n log n) в среднем и в худшем случае, Θ(n) в лучшем
/// (например, когда все элементы равны и просеивание сразу заканчивается).
/// Память: O(1), сортировка выполняется «на месте».
/// Устойчивость: неустойчива — обмен корня с последним элементом переносит равные
/// элементы через весь массив.
/// </para>
/// </remarks>
public static class HeapSortAlgorithm
{
    /// <summary>Выполняет сортировку, записывая каждый шаг в трассировщик.</summary>
    public static void Run(SortTracer tracer)
    {
        int n = tracer.Length;

        if (n < 2)
        {
            tracer.MarkAllSorted("Массив из одного элемента уже упорядочен.");
            return;
        }

        tracer.SetRange(0, n - 1);
        tracer.Note("Этап 1. Построение пирамиды: самый большой элемент должен подняться в корень (позиция 0).");

        // Просеиваем всех «родителей» справа налево. Листья (вторая половина массива)
        // просеивать не нужно — у них нет детей.
        for (int root = n / 2 - 1; root >= 0; root--)
        {
            tracer.Note($"Просеиваем поддерево с корнем на позиции {root} (значение {tracer.ValueAt(root)}).");
            SiftDown(tracer, root, n);
        }

        tracer.Note($"Пирамида построена: в корне максимум — {tracer.ValueAt(0)}.");

        tracer.Note("Этап 2. По очереди ставим максимум в конец и уменьшаем кучу.");

        // Постепенно переносим максимум в конец неотсортированной части.
        for (int end = n - 1; end > 0; end--)
        {
            tracer.SetRange(0, end);

            tracer.Swap(0, end,
                $"Меняем местами корень {tracer.ValueAt(0)} и последний элемент кучи {tracer.ValueAt(end)}.");

            // Бывший корень (максимум) встал на своё окончательное место.
            tracer.MarkSorted(end, $"Элемент {tracer.ValueAt(end)} занял окончательное место на позиции {end}.");

            // Оставшаяся часть кучи снова «просеивается».
            SiftDown(tracer, 0, end);
        }

        tracer.MarkAllSorted("Готово! Пирамида разобрана по элементам, массив упорядочен.");
    }

    /// <summary>
    /// Просеивает элемент из позиции root вниз по куче размером size:
    /// меняет его с наибольшим из детей, пока порядок не восстановится.
    /// </summary>
    private static void SiftDown(SortTracer tracer, int root, int size)
    {
        while (true)
        {
            // Позиции детей в двоичном дереве, уложенном в массив.
            int left = 2 * root + 1;
            int right = left + 1;

            // Индекс наибольшего среди родителя и детей.
            int largest = root;

            // Сравниваем родителя с левым ребёнком.
            if (left < size &&
                tracer.IsLess(largest, left,
                    $"Сравниваем родителя {tracer.ValueAt(largest)} (позиция {largest}) с левым ребёнком {tracer.ValueAt(left)} (позиция {left})."))
            {
                largest = left;
            }

            // Сравниваем победителя с правым ребёнком.
            if (right < size &&
                tracer.IsLess(largest, right,
                    $"Сравниваем {tracer.ValueAt(largest)} (позиция {largest}) с правым ребёнком {tracer.ValueAt(right)} (позиция {right})."))
            {
                largest = right;
            }

            // Родитель не меньше детей — порядок в куче восстановлен.
            if (largest == root)
            {
                tracer.Note($"Просеивание закончено: у {tracer.ValueAt(root)} нет ребёнка с большим значением.");
                return;
            }

            // Меняем родителя с самым большим ребёнком и продолжаем спуск.
            tracer.Swap(root, largest,
                $"{tracer.ValueAt(largest)} больше родителя — поднимаем его на позицию {root}.");

            root = largest;
        }
    }

    /// <summary>
    /// Быстрая версия без трассировки — для замеров времени.
    /// </summary>
    public static void Sort(int[] data)
    {
        int n = data.Length;

        for (int root = n / 2 - 1; root >= 0; root--)
            SiftDown(data, root, n);

        for (int end = n - 1; end > 0; end--)
        {
            (data[0], data[end]) = (data[end], data[0]);
            SiftDown(data, 0, end);
        }
    }

    private static void SiftDown(int[] data, int root, int size)
    {
        while (true)
        {
            int left = 2 * root + 1;
            int right = left + 1;
            int largest = root;

            if (left < size && data[largest] < data[left]) largest = left;
            if (right < size && data[largest] < data[right]) largest = right;

            if (largest == root) return;

            (data[root], data[largest]) = (data[largest], data[root]);
            root = largest;
        }
    }
}
