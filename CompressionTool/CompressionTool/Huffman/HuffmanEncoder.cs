using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompressionTool.Huffman
{
    internal class HuffmanEncoder
    {
        private HuffmanNode Root { get; set; }
        public Dictionary<byte, int> CodeLengths { get; private set; } = new Dictionary<byte, int>();
        private readonly Dictionary<byte, string> _encodingDictionary = new();
        private readonly Dictionary<byte, string> _canonicalCodes = new();

        public HuffmanEncoder(Dictionary<byte, int> frequencies)
        {
            BuildTree(frequencies);
        }

        public List<byte> GetHeader()
        {
            return CodeLengths.Select(symbol => new List<byte> { symbol.Key, (byte)symbol.Value }).SelectMany(x => x).ToList();
        }

        private void BuildTree(Dictionary<byte, int> frequencies)
        {
            var priorityQueue = new PriorityQueue<HuffmanNode, int>();

            if (frequencies.Count == 0)
            {
                Root = new HuffmanNode(0, 1);
                CodeLengths[0] = 1;
                return;
            }

            foreach (var symbol in frequencies)
                priorityQueue.Enqueue(new HuffmanNode(symbol.Key, symbol.Value), symbol.Value);

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

        public Dictionary<byte, string> GetCodes()
        {
            var codes = new Dictionary<byte, string>();
            TraverseTree(Root, "", codes);
            return codes;
        }

        private void TraverseTree(HuffmanNode node, string code, Dictionary<byte, string> codes)
        {
            if (node == null) return;

            if (node.Left == null && node.Right == null)
            {
                codes[node.Symbol] = string.IsNullOrEmpty(code) ? "0" : code;
                CodeLengths[node.Symbol] = codes[node.Symbol].Length;
                return;
            }

            TraverseTree(node.Left, code + "0", codes);
            TraverseTree(node.Right, code + "1", codes);
        }

        public Dictionary<byte, string> GetCanonicalCodes()
        {
            TraverseTree(Root, "", _encodingDictionary);
            return GenerateCanonicalCodes();
        }

        private Dictionary<byte, string> GenerateCanonicalCodes()
        {
            var sortedSymbols = _encodingDictionary
                .OrderBy(entry => entry.Value.Length)
                .ThenBy(entry => entry.Key)
                .ToList();

            string currentCode = new string('0', sortedSymbols.First().Value.Length);

            for (int i = 0; i < sortedSymbols.Count; i++)
            {
                var (symbol, _) = sortedSymbols[i];

                if (i > 0)
                {
                    long nextCodeValue = Convert.ToInt64(currentCode, 2) + 1;
                    currentCode = Convert.ToString(nextCodeValue, 2).PadLeft(currentCode.Length, '0');

                    int requiredLength = sortedSymbols[i].Value.Length;
                    if (currentCode.Length < requiredLength)
                        currentCode = currentCode.PadRight(requiredLength, '0');
                }

                _canonicalCodes[symbol] = currentCode;
            }

            return _canonicalCodes;
        }

        public static (List<byte> compressedData, List<byte> header) ApplyHuffman(List<byte> data)
        {
            if (data.Count == 0)
                return (new List<byte>(), new List<byte> { 0 });

            var frequency = data.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());
            var encoder = new HuffmanEncoder(frequency);
            var codes = encoder.GetCanonicalCodes();
            List<byte> header = encoder.GetHeader();

            StringBuilder encodedBits = new();
            foreach (var b in data)
                encodedBits.Append(codes[b]);

            int paddingRequired = CalculatePadding(encodedBits.Length);
            encodedBits.Append('0', paddingRequired);

            List<byte> compressedData = ConvertToByteList(encodedBits);
            header.Add((byte)paddingRequired);

            return (compressedData, header);
        }

        private static int CalculatePadding(int bitLength)
        {
            return (8 - (bitLength % 8)) % 8;
        }

        private static List<byte> ConvertToByteList(StringBuilder bitString)
        {
            var compressedData = new List<byte>();
            for (int i = 0; i < bitString.Length; i += 8)
            {
                compressedData.Add(Convert.ToByte(bitString.ToString(i, 8), 2));
            }
            return compressedData;
        }
    }
}
