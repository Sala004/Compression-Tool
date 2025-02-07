using System;
using System.Collections.Generic;
using System.IO;
using CompressionTool.Utilities;

class Program
{
    static void Main(string[] args)
    {
        InputReader _inputReader = new InputReader();
        string input = _inputReader.ReadOriginalFile("DataSet_1");

        string filePath = @"D:\Programming\Our road map\Compression-Tool\CompressionTool\CompressionTool\DataSet\DataSet_1.tsv";
        long originalSize = new FileInfo(filePath).Length;

        LZSSCompressor compressor = new LZSSCompressor();
        List<byte> compressedStream = compressor.LZSSCompress(input);
  
        StreamExtractor extractor = new StreamExtractor();
        extractor.ExtractStreams(compressedStream);

        // MergeStreams now takes the backward distances as List<List<byte>>
        List<byte> mergedStream = StreamMerger.MergeStreams(
            extractor.Flags, 
            extractor.Literals, 
            extractor.MatchLengths, 
            extractor.BackwardDistances);

        Console.WriteLine("\n-------------------------------------------------");
        double compressionRatio = (double)compressedStream.Count / originalSize;
        Console.WriteLine("\nFile size before compression : " + originalSize);
        Console.WriteLine("File size after compression : " + compressedStream.Count);
        Console.WriteLine("Compression ratio : " + compressionRatio.ToString("P2"));
        Console.WriteLine("-------------------------------------------------");

        Dictionary<byte, char> byteToCharMap = compressor.CharToByteMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        LZSSDecompressor decompressor = new LZSSDecompressor();
        string originalText = decompressor.LZSSDecompress(compressedStream, byteToCharMap);

        if(input.Length == originalText.Length)
            Console.WriteLine("Valid");
        else
            Console.WriteLine("Not valid");
    }
}
