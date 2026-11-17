using System.Text;

namespace AlgoLab.Labs.Lab3.Stack;

/// <summary>Стек на собственном односвязном списке. Голова списка = вершина стека.</summary>
public sealed class MyStack<T>
{
    private sealed class Node
    {
        public T Value;
        public Node? Next;
        public Node(T value, Node? next) { Value = value; Next = next; }
    }

    private Node? _head;

    public int Count { get; private set; }

    /// <summary>O(1): новый узел становится головой.</summary>
    public void Push(T value)
    {
        _head = new Node(value, _head);
        Count++;
    }

    /// <summary>O(1): снимает голову. Для пустого стека бросает InvalidOperationException.</summary>
    public T Pop()
    {
        if (_head is null) throw new InvalidOperationException("Стек пуст");
        var value = _head.Value;
        _head = _head.Next;
        Count--;
        return value;
    }

    /// <summary>O(1): значение вершины без удаления.</summary>
    public T Top()
    {
        if (_head is null) throw new InvalidOperationException("Стек пуст");
        return _head.Value;
    }

    public bool IsEmpty() => _head is null;

    /// <summary>O(n): элементы от вершины к основанию.</summary>
    public string Print(string separator = " ")
    {
        var sb = new StringBuilder();
        for (var node = _head; node is not null; node = node.Next)
        {
            if (sb.Length > 0) sb.Append(separator);
            sb.Append(node.Value);
        }
        return sb.ToString();
    }

    /// <summary>Элементы от вершины к основанию (для визуализации).</summary>
    public List<T> ToList()
    {
        var list = new List<T>(Count);
        for (var node = _head; node is not null; node = node.Next)
            list.Add(node.Value);
        return list;
    }
}
