using System;
using System.Collections.Generic;
using System.Linq;

namespace CompressionTool.Huffman
{
    internal class HuffmanEncoder
    {
        private HuffmanNode Root { get; set; }
        public Dictionary<byte, int> CodeLengths { get; private set; } = new Dictionary<byte, int>();
        private Dictionary<byte, string> EncodingDictionary = new Dictionary<byte, string>();
        private Dictionary<byte, string> CanonicalCodes = new Dictionary<byte, string>();

        public HuffmanEncoder(Dictionary<byte, int> frequencies)
        {
            BuildTree(frequencies);
        }

        // Returns the header that can be used by the decoder
        public List<byte> GetHeader()
        {
            var header = new List<byte>();

            foreach (var symbol in CodeLengths)
            {
                header.Add(symbol.Key);            // The symbol (byte)
                header.Add((byte)symbol.Value);    // The code length (byte)
            }

            return header;
        }

        private void BuildTree(Dictionary<byte, int> frequencies)
        {
            var priorityQueue = new PriorityQueue<HuffmanNode, int>();

            // If no frequencies are provided, create a default tree with a single node
            if (frequencies.Count == 0)
            {
                Root = new HuffmanNode(0, 1); // Default symbol (0) with frequency 1
                CodeLengths[0] = 1; // Assign a default bit length
                return;
            }

            // Create a leaf node for each symbol (byte) and add it to the priority queue
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
        public Dictionary<byte, string> GetCodes()
        {
            var codes = new Dictionary<byte, string>();
            Traverse(Root, "", codes);
            return codes;
        }

        private void Traverse(HuffmanNode node, string code, Dictionary<byte, string> codes)
        {
            if (node == null) return;

            if (node.Left == null && node.Right == null) // Leaf node
            {
                string finalCode = code.Length > 0 ? code : "0";  // Ensure at least one bit
                codes[node.Symbol] = finalCode;
                CodeLengths[node.Symbol] = finalCode.Length;  // Store the correct length
                return;
            }

            Traverse(node.Left, code + "0", codes);
            Traverse(node.Right, code + "1", codes);
        }

        // Step 2: Generate Canonical Huffman Codes
        public Dictionary<byte, string> GetCanonicalCodes()
        {
            Traverse(Root, "", EncodingDictionary);

            // Sort symbols by code length first, then by value
            var sortedSymbols = EncodingDictionary
                .OrderBy(entry => entry.Value.Length)
                .ThenBy(entry => entry.Key)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            string currentCode = "";
            bool isFirstSymbol = true;

            foreach (var entry in sortedSymbols)
            {
                if (isFirstSymbol)
                {
                    // Initialize the first code with all zeros based on its length
                    currentCode = new string('0', entry.Value.Length);
                    isFirstSymbol = false;
                }
                else
                {
                    int previousLength = currentCode.Length;

                    // Convert current code to integer, increment by 1
                    long nextCodeValue = Convert.ToInt64(currentCode, 2) + 1;
                    currentCode = Convert.ToString(nextCodeValue, 2);

                    // Ensure the new code maintains the correct length
                    if (currentCode.Length < previousLength)
                    {
                        int paddingLength = previousLength - currentCode.Length;
                        currentCode = new string('0', paddingLength) + currentCode;
                    }

                    // Extend code length if needed
                    int extraBits = entry.Value.Length - currentCode.Length;
                    if (extraBits > 0)
                    {
                        currentCode += new string('0', extraBits);
                    }
                }

                // Store the canonical code
                CanonicalCodes[entry.Key] = currentCode;
            }

            return CanonicalCodes;
        }
    }
}