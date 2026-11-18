using System.Collections;

namespace AlgoLab.Labs.Lab3.Queue;

/// <summary>
/// FIFO queue implemented with a singly linked list.
/// </summary>
public sealed class LinkedQueue<T> : IEnumerable<T>
{
    private sealed class Node(T value)
    {
        public T Value { get; } = value;
        public Node? Next { get; set; }
    }

    private Node? head;
    private Node? tail;

    public int Count { get; private set; }
    public bool IsEmpty => head is null;

    public void Enqueue(T value)
    {
        var node = new Node(value);

        if (tail is null)
        {
            // При добавлении в пустую очередь первый узел одновременно
            // становится началом и концом.
            head = tail = node;
        }
        else
        {
            tail.Next = node;
            tail = node;
        }

        Count++;
    }

    public bool TryDequeue(out T value)
    {
        if (head is null)
        {
            value = default!;
            return false;
        }

        value = head.Value;
        head = head.Next;
        Count--;

        if (head is null)
        {
            // После удаления последнего узла очередь снова пуста.
            tail = null;
        }

        return true;
    }

    public bool TryPeek(out T value)
    {
        if (head is null)
        {
            value = default!;
            return false;
        }

        value = head.Value;
        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var current = head; current is not null; current = current.Next)
        {
            yield return current.Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
