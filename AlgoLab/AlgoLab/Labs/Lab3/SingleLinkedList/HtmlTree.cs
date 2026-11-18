#nullable disable
using System.Xml.Linq;

namespace AlgoLab.Labs.Lab3.SingleLinkedList;

public class HtmlTree
{
    private XElement Root;

    // Строим дерево из строки с закрытыми тегами
    public void Build(string html)
    {
        Root = XElement.Parse(html);
    }

    // Начинаем вывод с корня
    public void Print()
    {
        if (Root == null)
        {
            return;
        }

        PrintNode(Root, 0);
    }

    private void PrintNode(XElement node, int depth)
    {
        // Каждый уровень добавляет два пробела
        string indent = new string(' ', depth * 2);
        Console.WriteLine(indent + node.Name);

        // Выводим дочерние элементы
        foreach (XElement child in node.Elements())
        {
            PrintNode(child, depth + 1);
        }
    }
}