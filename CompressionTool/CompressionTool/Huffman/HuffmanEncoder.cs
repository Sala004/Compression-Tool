using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool.Huffman
{
    internal class HuffmanEncoder
    {
        private HuffmanNode Root { get; set; }

        public HuffmanEncoder(Dictionary<char, int> frequencies) 
        {
            BuildTree(frequencies);
        }

        private void BuildTree(Dictionary<char, int> frequencies)
        {
            var priorityQueue = new PriorityQueue<HuffmanNode>();

            // Create a leaf node for each symbol and add it to the priority queue
            foreach (var symbol in frequencies)
            {
                priorityQueue.Enqueue(new HuffmanNode(symbol.Key, symbol.Value), symbol.Value);
            }

            // Build the Huffman tree
            while (priorityQueue.Count > 1)
            {
                var left = priorityQueue.Dequeue();
                var right = priorityQueue.Dequeue();

                var parent = new HuffmanNode(left.Frequency + right.Frequency)
                {
                    Left = left,
                    Right = right
                };

                priorityQueue.Enqueue(parent, parent.Frequency);
            }

            Root = priorityQueue.Dequeue();
        }

        public Dictionary<char, string> GetCodes()
        {
            var codes = new Dictionary<char, string>();
            Traverse(Root, "", codes);
            return codes;
        }

        private void Traverse(HuffmanNode node, string code, Dictionary<char, string> codes)
        {
            if (node == null) return; //Tree not constructed

            if (node.Left == null && node.Right == null)
            {
                codes[node.Symbol] = code;
                return;
            }

            Traverse(node.Left, code + "0", codes);
            Traverse(node.Right, code + "1", codes);
        }
    }

    // Priority Queue implementation for Huffman Nodes
    public class PriorityQueue<T>
    {
        private SortedDictionary<int, Queue<T>> _dictionary = new SortedDictionary<int, Queue<T>>();

        public int Count { get; private set; }

        public void Enqueue(T item, int priority)
        {
            if (!_dictionary.ContainsKey(priority))
            {
                _dictionary[priority] = new Queue<T>();
            }

            _dictionary[priority].Enqueue(item);
            Count++;
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            var first = _dictionary.First();
            var item = first.Value.Dequeue();

            if (first.Value.Count == 0)
            {
                _dictionary.Remove(first.Key);
            }

            Count--;
            return item;
        }
    }
}
