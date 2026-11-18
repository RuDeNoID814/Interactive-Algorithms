#nullable disable
using System;
using System.Security.Cryptography;
using System.Text;

namespace AlgoLab.Labs.Lab3.SingleLinkedList;

public class Blockchain
{
    private class Block
    {
        public string Data;
        public string Hash;
        public string PreviousHash;
        public Block Previous;

        public Block(string data, Block previous)
        {
            Data = data;
            Previous = previous;

            // Запоминаем хеш предыдущего блока
            if (previous == null)
            {
                PreviousHash = "0";
            }
            else
            {
                PreviousHash = previous.Hash;
            }

            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            string text = PreviousHash + "|" + Data;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }
    }

    private Block last;

    // Добавляем блок со ссылкой на прежний последний
    public void Add(string data)
    {
        Block block = new Block(data, last);
        last = block;
    }

    public void Print()
    {
        Block current = last;

        while (current != null)
        {
            Console.WriteLine("Данные: " + current.Data);
            Console.WriteLine("Хеш: " + current.Hash);
            Console.WriteLine("Предыдущий хеш: " + current.PreviousHash);
            Console.WriteLine();

            current = current.Previous;
        }
    }

    // Данные блоков для показа на странице, от последнего к первому.
    public IEnumerable<(string Data, string Hash, string PreviousHash)> GetBlocks()
    {
        Block current = last;
        while (current != null)
        {
            yield return (current.Data, current.Hash, current.PreviousHash);
            current = current.Previous;
        }
    }

}