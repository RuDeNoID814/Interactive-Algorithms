#nullable disable

namespace AlgoLab.Labs.Lab3.SingleLinkedList;

public class MyLinkedList
{
    private class Node
    {
        public int Data;
        public Node Next;

        public Node(int data)
        {
            Data = data;
            Next = null;
        }
    }

    private Node Head;

    public MyLinkedList()
    {
        Head = null;
    }

    // Добавление элемента связного списка
    public void Add(int data)
    {
        Node newNode = new Node(data);
        
        if (Head == null)
        {
            Head = newNode;
            return;
        }

        Node current = Head;
        while (current.Next != null)
        {
            current = current.Next;
        }
        
        // Привязываем новый узел в конец
        current.Next = newNode;
    }
    
    
    /// <summary>
    /// Задание 1
    /// </summary>
    public void Reverse()
    {
        Node prev = null;
        Node current = Head;
        Node next = null;
    
        while (current != null)
        {
            next = current.Next; 
            current.Next = prev; 
            prev = current;      
            current = next;      
        }
        Head = prev;
    }

    /// <summary>
    /// Задание 2
    /// </summary>
    public void MoveLastToFirst()
    {
        Node prev = null;
        Node current = Head;
        
        if (Head == null || Head.Next == null)
        {
            return;
        }
        
        while (current.Next != null)
        {
            prev = current;
            current = current.Next;
        }
        
        prev.Next = null;
        current.Next = Head;
        Head = current;
    }
    
    public void MoveFirstToLast()
    {
        if (Head == null || Head.Next == null)
        {
            return;
        }

        Node first = Head;
        Node current = Head;

        while (current.Next != null)
        {
            current = current.Next;
        }

        Head = first.Next;
        first.Next = null;
        current.Next = first;
    }

    /// <summary>
    /// Задание 3
    /// </summary>
    public int CountDistint()
    {
        int count = 0;
        Node current = Head;

        while (current != null)
        {
            Node next = current.Next;
            bool found = false;

            while (next != null)
            {
                if (next.Data == current.Data)
                {
                    found = true;
                    break;
                }
                
                next = next.Next;
            }

            if (!found)
            {
                count++;
            }
            
            current = current.Next;
        }
        
        return count;
    }

    /// <summary>
    /// Задание 4
    /// </summary>
    public void RemoveNotUnique()
    {
        Node current = Head;
        Node prev = null;

        while (current != null)
        {
            bool repeated = false;
            Node checkPrev = current;
            Node check = current.Next;

            while (check != null)
            {
                if (check.Data == current.Data)
                {
                    repeated = true;
                    checkPrev.Next = check.Next;
                }
                else
                {
                    checkPrev = check;
                }
                check = check.Next;
            }

            if (repeated)
            {
                if (prev == null)
                {
                    Head = current.Next;
                }
                else
                {
                    prev.Next = current.Next;
                }
            }
            else
            {
                prev = current;
            }
            current = current.Next;
        }
    }

    /// <summary>
    /// Задание 5
    /// </summary>
    public void InsertSelfAfter(int x)
    {
        Node position = Head;
        
        // Ищем переданное x
        while (position != null && position.Data != x)
        {
            position = position.Next;
        }

        if (position == null)
        {
            return;
        }
        
        // Копия всего класса
        MyLinkedList copy = new MyLinkedList();
        Node current = Head;

        while (current != null)
        {
            copy.Add(current.Data);
            current = current.Next;
        }
        
        // Последний узел копии
        Node last = copy.Head;
        while (last.Next != null)
        {
            last = last.Next;
        }
        
        
        last.Next = position.Next;
        position.Next = copy.Head;
    }

    /// <summary>
    /// Задание 6
    /// </summary>
    public void InsertSorted(int e)
    {
        Node node = new Node(e);
        
        // Проверка , можно ли его сразу поставить первым
        if (Head == null || e <= Head.Data)
        {
            node.Next = Head;
            Head = node;
            return;
        }
        Node current = Head;
        // Ищем узел, после которого вставим число
        while (current.Next != null && current.Next.Data < e)
        {
            current = current.Next;
        }
        
        node.Next = current.Next;
        current.Next = node;
    }

    /// <summary>
    /// Задание 7
    /// </summary>
    public void RemoveAll(int e)
    {
        // Удаляем подходяшие элементы в начале списка
        while (Head != null && Head.Data == e)
        {
            Head = Head.Next;
        }
        
        Node current = Head;
        while (current != null && current.Next != null)
        {
            if (current.Next.Data == e)
            {
                // Обходим удаляемый узел
                current.Next = current.Next.Next;
            }
            else
            {
                current = current.Next;
            }
        }
    }

    /// <summary>
    /// Задание 8
    /// </summary>
    public void InsertBefore(int e, int f)
    {
        if (Head == null)
        {
            return;
        }

        Node node = new Node(f);
        
        // Вставка перед головой (отдельный случай)
        if (Head.Data == e)
        {
            node.Next = Head;
            Head = node;
            return;
        }
        
        Node current = Head;
        // Остановка перед узлом со значением e
        while (current.Next != null && current.Next.Data != e)
        {
            current = current.Next;
        }
        
        // Если e отсутствует
        if (current.Next == null)
        {
            return;
        }

        node.Next = current.Next;
        current.Next = node;
    }
    
    /// <summary>
    /// Задание 9
    /// </summary>
    public void Append(MyLinkedList other)
    {
        MyLinkedList copy = new MyLinkedList();
        Node current = other.Head;

        // Копируем узлы, чтобы списки не использовали одни и те же узлы
        while (current != null)
        {
            copy.Add(current.Data);
            current = current.Next;
        }

        // Если текущий список пуст, копия становится его содержимым
        if (Head == null)
        {
            Head = copy.Head;
            return;
        }

        current = Head;

        // Доходим до последнего узла и соединяем копию
        while (current.Next != null)
        {
            current = current.Next;
        }
        current.Next = copy.Head;
    }
    
    /// <summary>
    /// Задание 10
    /// </summary>
    public MyLinkedList Split(int value)
    {
        MyLinkedList second = new MyLinkedList();

        Node current = Head;
        Node prev = null;

        // Ищем разделитель и запоминаем предыдущий узел
        while (current != null && current.Data != value)
        {
            prev = current;
            current = current.Next;
        }

        // Возвращает пустой второй список, если числа нету
        if (current == null)
            return second;
        
        // Второй список начнем после разделителя
        second.Head = current.Next;

        // Если разделитель был первым
        if (prev == null)
        {
            Head = null;
        }
        else
        {
            // Завершаем первую часть перед разделителем
            prev.Next = null;
        }
        
        current.Next = null;

        return second;
    }

    /// <summary>
    /// Задание 11
    /// </summary>
    public void Duplicate()
    {
        // this - текущий список
        Append(this);
    }

    /// <summary>
    /// Задание 12
    /// </summary>
    public void Swap(int first, int second)
    {
        Node firstNode = Head;
        Node secondNode = Head;
        
        while (firstNode != null && firstNode.Data != first)
        {
            firstNode = firstNode.Next;
        }

        while (secondNode != null && secondNode.Data != second)
        {
            secondNode = secondNode.Next;
        }
        
        if (firstNode == null || secondNode == null)
            return;

        int temp = firstNode.Data;
        firstNode.Data = secondNode.Data;
        secondNode.Data = temp;
    }
    

    public void Print()
    {
        Node current = Head;
        while (current != null)
        {
            Console.Write(current.Data + " -> ");
            current = current.Next;
        }
        Console.WriteLine("null");
    }

    // Передаём значения узлов интерфейсу сайта.
    public IEnumerable<int> GetValues()
    {
        Node current = Head;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

}