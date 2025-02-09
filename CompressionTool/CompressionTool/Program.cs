using CompressionTool.Huffman;
using CompressionTool.Utilities;
using DecompressionTool;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        string FileName = @"..\..\..\Dataset\DataSet_1.tsv";

        // Ensure consistent UTF-8 encoding
        string fileText = File.ReadAllText(FileName, Encoding.UTF8);
        byte[] inputBytes = Encoding.UTF8.GetBytes(fileText);
        int originalSize = inputBytes.Length;

        Console.WriteLine($"Original File Size: {originalSize} bytes");

        // LZSS Compression with stopwatch
        Stopwatch sw = Stopwatch.StartNew();
        LZSSCompressor compressor = new LZSSCompressor();
        List<byte> compressedStream = compressor.LZSSCompress(fileText);
        sw.Stop();
        Console.WriteLine($"LZSS Compressed Size: {compressedStream.Count} bytes");
        double totalTime = sw.Elapsed.TotalSeconds;

        // Extract LZSS Streams
        StreamExtractor extractor = new StreamExtractor();
        extractor.ExtractStreams(compressedStream);

        int CalculateExtractor = extractor.Literals.Count
                               + extractor.BackwardDistances.Count
                               + extractor.MatchLengths.Count;

        // Huffman Encoding
        sw.Restart();
        var (compressedLiterals, literalsHeader) = HuffmanEncoder.ApplyHuffman(extractor.Literals);
        var (compressedBackwardDistance, backwardDistanceHeader) = HuffmanEncoder.ApplyHuffman(extractor.BackwardDistances);
        var (compressedMatchLengths, matchLengthsHeader) = HuffmanEncoder.ApplyHuffman(extractor.MatchLengths);
        sw.Stop();

        totalTime += sw.Elapsed.TotalSeconds;
        int compressedSize = compressedLiterals.Count 
                           + compressedBackwardDistance.Count 
                           + compressedMatchLengths.Count;

        Console.WriteLine($"Huffman Compression Size: {compressedSize}");
        Console.WriteLine($"Compression ratio: {1 - ((double)compressedSize / originalSize):P2}");
        Console.WriteLine($"Time: {totalTime:F2}s");

        // Huffman Decoding
        List<byte> decompressedLiterals = HuffmanDecoder.DecompressHuffman(compressedLiterals, literalsHeader);
        List<byte> decompressedBackwardDistance = HuffmanDecoder.DecompressHuffman(compressedBackwardDistance, backwardDistanceHeader);
        List<byte> decompressedMatchLengths = HuffmanDecoder.DecompressHuffman(compressedMatchLengths, matchLengthsHeader);

        int totalHuffmanDecompressedSize = decompressedLiterals.Count
                                         + decompressedBackwardDistance.Count
                                         + decompressedMatchLengths.Count;

        if (CalculateExtractor == totalHuffmanDecompressedSize)
        {
            Console.WriteLine("Validation Passed: Decompressed size matches LZSS compressed size.");
        }
        else
        {
            Console.WriteLine($"Validation Failed: LZSS Compressed Size = {compressedStream.Count} bytes, Huffman Decompressed Size = {totalHuffmanDecompressedSize} bytes.");
        }

        // Merge the streams back
        List<byte> mergedStream = StreamMerger.MergeStreams(
            extractor.PaddingBits,
            extractor.Flags,
            decompressedLiterals,
            decompressedMatchLengths,
            decompressedBackwardDistance);

        Dictionary<byte, char> byteToCharMap = compressor.CharToByteMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        LZSSDecompressor decompressor = new LZSSDecompressor();

        sw.Restart();
        string originalText = decompressor.LZSSDecompress(mergedStream, byteToCharMap);
        sw.Stop();

        // Compare with built-in Deflate compression
        sw.Restart();
        byte[] deflateCompressed = DeflateCompress(inputBytes);
        sw.Stop();
        Console.WriteLine($"Deflate Compressed Size: {deflateCompressed.Length} bytes");
        Console.WriteLine($"Compression Ratio (Deflate): {1 - (double)deflateCompressed.Length / originalSize:P2}");
    }

    // Built-in Deflate compression
    static byte[] DeflateCompress(byte[] data)
    {
        using (MemoryStream output = new MemoryStream())
        using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
            dstream.Close();
            return output.ToArray();
        }
    }
}