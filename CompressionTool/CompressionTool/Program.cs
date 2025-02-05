using CompressionTool.Huffman;
using CompressionTool.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        InputReader _inputReader = new InputReader();
        string Input = _inputReader.ReadOriginalFile("DataSet_1");

        if (string.IsNullOrEmpty(Input))
        {
            Console.WriteLine("Error: Input file is empty or not read correctly!");
            return;
        }

        long originalSize = Encoding.UTF8.GetByteCount(Input);  // original size (in bytes)

        var frequencies = new Dictionary<char, int>();
        foreach (var c in Input)
        {
            if (frequencies.ContainsKey(c))
                frequencies[c]++;
            else
                frequencies[c] = 1;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Build Huffman Tree
        var huffmanTree = new HuffmanEncoder(frequencies);
        var canonicalHuffmanCodes = huffmanTree.GetCanonicalCodes();

        if (canonicalHuffmanCodes.Count == 0)
        {
            Console.WriteLine("Error: Huffman codes were not generated!");
            return;
        }

        Console.WriteLine("\nCanonical Huffman Codes:");
        foreach (var code in canonicalHuffmanCodes)
        {
            Console.WriteLine($"{code.Key} : {code.Value}");
        }

        // Encode the input string efficiently
        StringBuilder encodedString = new StringBuilder();
        foreach (var c in Input)
        {
            encodedString.Append(canonicalHuffmanCodes[c]);
        }

        stopwatch.Stop();

        long compressedSizeBits = encodedString.Length;
        long compressedSizeBytes = (compressedSizeBits + 7) / 8; // Convert bits to bytes

        Console.WriteLine("\nFirst 100 encoded bits:");
        Console.WriteLine(encodedString.Length > 100 ? encodedString.ToString().Substring(0, 100) : encodedString.ToString());

        Console.WriteLine("\nCompression Statistics:");
        Console.WriteLine($"Original Size      : {originalSize} bytes");
        Console.WriteLine($"Compressed Size    : {compressedSizeBytes} bytes");
        Console.WriteLine($"Compression Ratio  : {(1 - (double)compressedSizeBytes / originalSize):P2}");
        Console.WriteLine($"Compression Time   : {stopwatch.ElapsedMilliseconds} ms");
    }
}
