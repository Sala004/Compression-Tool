using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DecompressionTool
{
    internal class HuffmanDecoder
    {
        private readonly Dictionary<byte, string> CanonicalCodes; // Store canonical codes
        public Dictionary<string, byte> ReverseCodebook { get; private set; } // Reverse lookup for decoding

        public HuffmanDecoder()
        {
            CanonicalCodes = new Dictionary<byte, string>();
            ReverseCodebook = new Dictionary<string, byte>();
        }

        // Reconstruct the canonical Huffman codes from the header
        public void BuildCanonicalCodesFromHeader(List<byte> header)
        {
            if (header == null || header.Count < 2 || header.Count % 2 != 0)
            {
                throw new ArgumentException("Invalid or empty header. Cannot build canonical codes.");
            }

            var codeLengths = new SortedDictionary<byte, int>(); // Auto-sorts symbols

            for (int i = 0; i < header.Count; i += 2)
            {
                byte symbol = header[i];
                byte codeLength = header[i + 1];
                codeLengths[symbol] = codeLength;
            }

            if (codeLengths.Count == 0)
                throw new InvalidOperationException("No valid symbols found in the header.");

            ReverseCodebook.Clear();
            int nextCode = 0; // Start from all 0s
            int previousLength = 0;

            foreach (var (symbol, length) in codeLengths.OrderBy(x => x.Value).ThenBy(x => x.Key))
            {
                if (length > previousLength)
                {
                    nextCode <<= (length - previousLength); // Shift left to match new length
                    previousLength = length;
                }

                string binaryCode = Convert.ToString(nextCode, 2).PadLeft(length, '0');
                CanonicalCodes[symbol] = binaryCode;
                ReverseCodebook[binaryCode] = symbol;

                nextCode++;
            }
        }

        // Decode the encoded data using the canonical Huffman codes
        public byte[] Decode(byte[] encodedData)
        {
            if (encodedData == null || encodedData.Length == 0) return Array.Empty<byte>();

            var decodedData = new List<byte>();
            var bitString = new StringBuilder();

            foreach (var b in encodedData)
            {
                bitString.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            string currentCode = "";
            foreach (char bit in bitString.ToString())
            {
                currentCode += bit;
                if (ReverseCodebook.TryGetValue(currentCode, out byte symbol))
                {
                    decodedData.Add(symbol);
                    currentCode = "";
                }
            }

            return decodedData.ToArray();
        }


        public static List<byte> DecompressHuffman(List<byte> compressedData, List<byte> header)
        {
            if (compressedData.Count == 0)
                return new List<byte>();

            var decoder = new HuffmanDecoder();
            int paddingRequired = header[^1];
            header.RemoveAt(header.Count - 1);
            decoder.BuildCanonicalCodesFromHeader(header);
            List<byte> decompressedData = new List<byte>();
            string currentCode = "";

            for (int i = 0; i < compressedData.Count; i++)
            {
                byte byteValue = compressedData[i];
                int bitsToRead = (i == compressedData.Count - 1) ? 8 - paddingRequired : 8;

                for (int j = 7; j >= 8 - bitsToRead; j--)
                {
                    currentCode += ((byteValue >> j) & 1) == 1 ? '1' : '0';
                    if (decoder.ReverseCodebook.TryGetValue(currentCode, out byte decodedByte))
                    {
                        decompressedData.Add(decodedByte);
                        currentCode = "";
                    }
                }
            }
            return decompressedData;
        }
    }
}