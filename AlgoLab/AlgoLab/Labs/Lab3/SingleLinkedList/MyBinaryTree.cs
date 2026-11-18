namespace AlgoLab.Labs.Lab3.SingleLinkedList;

public class MyBinaryTree
{
    private class Node
    {
        public char Data;
        public Node Left;
        public Node Right;

        public Node(char data)
        {
            Data = data;
        }
    }

    private Node Root;
    private string text;
    private int index;

    private Node ReadNode()
    {
        if (index >= text.Length)
        {
            throw new ArgumentException("не хватает символов");
        }
        
        char symbol = text[index];
        index++;

        if (symbol == '*')
        {
            return null;
        }
        Node node = new Node(symbol);
        
        node.Left = ReadNode();
        node.Right = ReadNode();
        return node;
    }

    public void Build(string value)
    {
        text = value;
        index = 0;
        
        Root = ReadNode();

        if (index != text.Length)
        {
            throw new ArgumentException("В строке лишние символы");
        }
    }


}