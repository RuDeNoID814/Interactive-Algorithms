namespace AlgoLab.Labs.Lab4.Sorting;

/// <summary>
/// «Приборная панель» сортировки: алгоритм работает с массивом только через методы
/// трассировщика, поэтому каждое сравнение, перестановка и сдвиг автоматически
/// попадают и в массив, и в лог шагов, и в счётчики.
/// </summary>
/// <remarks>
/// Смысл разделения простой: код алгоритма остаётся почти таким же, как в учебнике,
/// а весь учёт и визуализация не засоряют его.
/// </remarks>
public sealed class SortTracer
{
    /// <summary>Предохранитель от лавины шагов на слишком больших массивах.</summary>
    private const int MaxSteps = 20000;

    private readonly int[] _data;
    private readonly List<SortStep> _steps = new();
    private readonly SortedSet<int> _sorted = new();

    private int _rangeLeft = -1;
    private int _rangeRight = -1;
    private int _pivotIndex = -1;
    private int _pivotValue;
    private int _heldIndex = -1;
    private int _heldValue;

    public SortTracer(IReadOnlyList<int> source)
    {
        _data = source.ToArray();
        Note("Начальный массив готов. Начинаем сортировку.");
    }

    /// <summary>Длина массива.</summary>
    public int Length => _data.Length;

    /// <summary>Сколько раз алгоритм сравнивал элементы.</summary>
    public int Comparisons { get; private set; }

    /// <summary>Сколько раз алгоритм менял элементы местами.</summary>
    public int Swaps { get; private set; }

    /// <summary>Сколько раз алгоритм перемещал элементы (сдвиги и вставки).</summary>
    public int Shifts { get; private set; }

    /// <summary>Всего перемещений: перестановки плюс сдвиги.</summary>
    public int TotalMoves => Swaps + Shifts;

    /// <summary>Лог шагов.</summary>
    public IReadOnlyList<SortStep> Steps => _steps;

    /// <summary>Текущее (итоговое) состояние массива.</summary>
    public int[] Values => _data;

    /// <summary>Признак того, что лог обрезан предохранителем.</summary>
    public bool Truncated { get; private set; }

    /// <summary>Текущее значение элемента по индексу.</summary>
    public int ValueAt(int index) => _data[index];

    // =========================================
    // ПОЯСНЕНИЯ И РАЗМЕТКА
    // =========================================

    /// <summary>Добавляет в лог пояснение, не меняя массив.</summary>
    public void Note(string message) =>
        Capture(SortStepKind.Note, message, Array.Empty<int>());

    /// <summary>Задаёт активный участок массива (в лог не пишется).</summary>
    public void SetRange(int left, int right)
    {
        _rangeLeft = left;
        _rangeRight = right;
    }

    /// <summary>Снимает разметку активного участка (в лог не пишется).</summary>
    public void ClearRange()
    {
        _rangeLeft = -1;
        _rangeRight = -1;
    }

    // =========================================
    // СРАВНЕНИЯ
    // =========================================

    /// <summary>Сравнивает элементы по индексам: истина, если левый больше правого.</summary>
    public bool IsGreater(int i, int j, string message)
    {
        Comparisons++;
        Capture(SortStepKind.Compare, message, new[] { i, j });
        return _data[i] > _data[j];
    }

    /// <summary>Сравнивает элементы по индексам: истина, если левый меньше правого.</summary>
    public bool IsLess(int i, int j, string message)
    {
        Comparisons++;
        Capture(SortStepKind.Compare, message, new[] { i, j });
        return _data[i] < _data[j];
    }

    /// <summary>Сравнивает элемент с опорным: истина, если элемент меньше опорного.</summary>
    public bool IsBelowPivot(int index, string message)
    {
        Comparisons++;
        Capture(SortStepKind.Compare, message, new[] { index });
        return _data[index] < _pivotValue;
    }

    /// <summary>Сравнивает элемент с опорным: истина, если элемент больше опорного.</summary>
    public bool IsAbovePivot(int index, string message)
    {
        Comparisons++;
        Capture(SortStepKind.Compare, message, new[] { index });
        return _data[index] > _pivotValue;
    }

    /// <summary>
    /// Сравнивает «взятый в руку» ключ с элементом массива.
    /// Истина, если ключ меньше элемента (значит, элемент надо сдвинуть вправо).
    /// </summary>
    public bool HeldIsSmaller(int index, string message)
    {
        Comparisons++;
        Capture(SortStepKind.Compare, message, new[] { index });
        return _heldValue < _data[index];
    }

    // =========================================
    // ПЕРЕСТАНОВКИ И ПЕРЕМЕЩЕНИЯ
    // =========================================

    /// <summary>Меняет два элемента местами.</summary>
    public void Swap(int i, int j, string message)
    {
        if (i != j)
        {
            (_data[i], _data[j]) = (_data[j], _data[i]);
            Swaps++;

            // Опорный элемент мог «переехать» вместе с обменом — следим за ним.
            if (_pivotIndex == i) _pivotIndex = j;
            else if (_pivotIndex == j) _pivotIndex = i;
        }

        Capture(SortStepKind.Swap, message, new[] { i, j });
    }

    /// <summary>Берёт элемент «в руку»: он временно хранится вне массива.</summary>
    public void Hold(int index, string message)
    {
        _heldIndex = index;
        _heldValue = _data[index];
        Capture(SortStepKind.Hold, message, new[] { index });
    }

    /// <summary>Сдвигает элемент из одной позиции в другую.</summary>
    public void Shift(int from, int to, string message)
    {
        _data[to] = _data[from];
        Shifts++;

        if (_pivotIndex == from) _pivotIndex = to;

        // «Дырка» от взятого в руку ключа переезжает на освободившееся место.
        if (_heldIndex >= 0) _heldIndex = from;

        Capture(SortStepKind.Shift, message, new[] { from, to });
    }

    /// <summary>Ставит «взятый в руку» ключ в освободившуюся позицию.</summary>
    public void PlaceHeld(int index, string message)
    {
        _data[index] = _heldValue;
        Shifts++;
        _heldIndex = -1;
        Capture(SortStepKind.Shift, message, new[] { index });
    }

    /// <summary>
    /// Возвращает ключ на прежнее место, не засчитывая перемещение:
    /// используется, когда вставлять никуда не пришлось.
    /// </summary>
    public void ReleaseHeld(string message)
    {
        _heldIndex = -1;
        Capture(SortStepKind.Note, message, Array.Empty<int>());
    }

    // =========================================
    // ОПОРНЫЙ ЭЛЕМЕНТ
    // =========================================

    /// <summary>Запоминает опорный элемент и возвращает его значение.</summary>
    public int MarkPivot(int index, string message)
    {
        _pivotIndex = index;
        _pivotValue = _data[index];
        Capture(SortStepKind.Pivot, message, new[] { index });
        return _pivotValue;
    }

    /// <summary>Снимает отметку опорного элемента.</summary>
    public void ClearPivot(string? message = null)
    {
        _pivotIndex = -1;

        if (message is not null)
            Capture(SortStepKind.Note, message, Array.Empty<int>());
    }

    // =========================================
    // ГОТОВЫЕ ЭЛЕМЕНТЫ
    // =========================================

    /// <summary>Отмечает элемент как стоящий на своём окончательном месте.</summary>
    public void MarkSorted(int index, string message)
    {
        _sorted.Add(index);
        Capture(SortStepKind.Sorted, message, new[] { index });
    }

    /// <summary>Отмечает весь массив отсортированным и завершает работу.</summary>
    public void MarkAllSorted(string message)
    {
        for (int i = 0; i < _data.Length; i++)
            _sorted.Add(i);

        _pivotIndex = -1;
        _heldIndex = -1;
        ClearRange();

        Capture(SortStepKind.Done, message, Array.Empty<int>());
    }

    // =========================================
    // ВНУТРЕННЕЕ
    // =========================================

    /// <summary>Сохраняет снимок состояния массива в лог.</summary>
    private void Capture(SortStepKind kind, string message, int[] highlight)
    {
        if (_steps.Count >= MaxSteps)
        {
            Truncated = true;
            return;
        }

        _steps.Add(new SortStep
        {
            Kind = kind,
            Values = (int[])_data.Clone(),
            Highlight = highlight,
            Sorted = _sorted.ToArray(),
            PivotIndex = _pivotIndex,
            PivotValue = _pivotValue,
            HeldIndex = _heldIndex,
            HeldValue = _heldValue,
            RangeLeft = _rangeLeft,
            RangeRight = _rangeRight,
            Comparisons = Comparisons,
            Swaps = Swaps,
            Shifts = Shifts,
            Message = message
        });
    }
}
