using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace CompressionTool.Huffman
{
    internal class HuffmanEncoder
    {
        private HuffmanNode Root { get; set; }
        public Dictionary<char, int> CodeLengths { get; private set; } = new Dictionary<char, int>();

        public HuffmanEncoder(Dictionary<char, int> frequencies)
        {
            BuildTree(frequencies);
        }

        private void BuildTree(Dictionary<char, int> frequencies)
        {
            var priorityQueue = new PriorityQueue<HuffmanNode, int>();

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

        // Step 1: Get Normal Huffman Codes
        public Dictionary<char, string> GetCodes()
        {
            var codes = new Dictionary<char, string>();
            Traverse(Root, "", codes);
            return codes;
        }

        private void Traverse(HuffmanNode node, string code, Dictionary<char, string> codes)
        {
            if (node == null) return;

            if (node.Left == null && node.Right == null)
            {
                codes[node.Symbol] = code;
                CodeLengths[node.Symbol] = code.Length;  // Store the code length
                return;
            }

            Traverse(node.Left, code + "0", codes);
            Traverse(node.Right, code + "1", codes);
        }

        // Step 2: Generate Canonical Huffman Codes
        public Dictionary<char, string> GetCanonicalCodes()
        {
            var codes = new Dictionary<char, string>();
            Traverse(Root, "", codes);
            var sortedSymbols = CodeLengths.OrderBy(x => x.Value)
                                           .ThenBy(x => x.Key)
                                           .ToList();

            // a string of '0's with the same length as the smallest code's length
            string currentCode = new string('0', sortedSymbols[0].Value);
            Dictionary<char, string> canonicalCodes = new Dictionary<char, string>();

            foreach (var (symbol, length) in sortedSymbols)
            {
                canonicalCodes[symbol] = currentCode;
                if (currentCode.Length == length)
                {
                    currentCode = IncrementBinary(currentCode); // Generate the next code in binary
                }
            }

            return canonicalCodes;
        }

        private string IncrementBinary(string binary)
        {
            int decimalValue = Convert.ToInt32(binary, 2) + 1;
            return Convert.ToString(decimalValue, 2).PadLeft(binary.Length, '0');
        }
    }
}