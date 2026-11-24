namespace AlgoLab.Labs.Lab4.Sorting;

/// <summary>
/// Вид события, попавшего в лог сортировки.
/// </summary>
public enum SortStepKind
{
    /// <summary>Пояснение: массив не менялся.</summary>
    Note,

    /// <summary>Сравнение двух элементов.</summary>
    Compare,

    /// <summary>Обмен двух элементов местами.</summary>
    Swap,

    /// <summary>Сдвиг элемента (перемещение внутри массива).</summary>
    Shift,

    /// <summary>Ключ взят «в руку» и ищет своё место.</summary>
    Hold,

    /// <summary>Выбран опорный элемент.</summary>
    Pivot,

    /// <summary>Элемент занял своё окончательное место.</summary>
    Sorted,

    /// <summary>Сортировка завершена.</summary>
    Done
}

/// <summary>
/// Один шаг сортировки: снимок массива и всё, что нужно подсветить на экране.
/// </summary>
/// <remarks>
/// Шаги хранят массив целиком, а не «дельту». Для учебных размеров (до 30 элементов)
/// это наглядно и дёшево: можно вернуться на любой шаг назад без пересчёта.
/// </remarks>
public sealed class SortStep
{
    /// <summary>Что произошло на этом шаге.</summary>
    public SortStepKind Kind { get; init; }

    /// <summary>Состояние массива после этого шага.</summary>
    public int[] Values { get; init; } = Array.Empty<int>();

    /// <summary>Индексы, которые нужно подсветить (сравниваемые или обмениваемые).</summary>
    public int[] Highlight { get; init; } = Array.Empty<int>();

    /// <summary>Индексы элементов, уже стоящих на своём окончательном месте.</summary>
    public int[] Sorted { get; init; } = Array.Empty<int>();

    /// <summary>Индекс опорного элемента или −1, если опорный не выбран.</summary>
    public int PivotIndex { get; init; } = -1;

    /// <summary>Значение опорного элемента.</summary>
    public int PivotValue { get; init; }

    /// <summary>Индекс «дырки», из которой взят ключ, или −1.</summary>
    public int HeldIndex { get; init; } = -1;

    /// <summary>Значение ключа, взятого «в руку».</summary>
    public int HeldValue { get; init; }

    /// <summary>Левая граница активного участка массива или −1.</summary>
    public int RangeLeft { get; init; } = -1;

    /// <summary>Правая граница активного участка массива или −1.</summary>
    public int RangeRight { get; init; } = -1;

    /// <summary>Накопленное число сравнений к этому шагу.</summary>
    public int Comparisons { get; init; }

    /// <summary>Накопленное число перестановок к этому шагу.</summary>
    public int Swaps { get; init; }

    /// <summary>Накопленное число перемещений (сдвигов) к этому шагу.</summary>
    public int Shifts { get; init; }

    /// <summary>Пояснение для человека.</summary>
    public string Message { get; init; } = "";

    /// <summary>Проверяет, входит ли индекс в активный участок.</summary>
    public bool InRange(int index) =>
        RangeLeft < 0 || RangeRight < 0 || (index >= RangeLeft && index <= RangeRight);

    /// <summary>Проверяет, стоит ли элемент на своём окончательном месте.</summary>
    public bool IsSorted(int index) => Array.IndexOf(Sorted, index) >= 0;

    /// <summary>Проверяет, подсвечен ли элемент на этом шаге.</summary>
    public bool IsHighlighted(int index) => Array.IndexOf(Highlight, index) >= 0;
}
