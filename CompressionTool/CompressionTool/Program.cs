using System;
using System.Collections.Generic;
using System.IO;
using CompressionTool.Huffman;
using CompressionTool.Utilities;
using DecompressionTool;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string FileName = @"..\..\..\Dataset\DataSet_1.tsv";
        byte[] inputBytes = File.ReadAllBytes(FileName);
        int originalSize = inputBytes.Length;
        Console.WriteLine($"Original File Size: {originalSize} bytes");

        //LZSS Compression
        LZSSCompressor compressor = new LZSSCompressor();
        List<byte> compressedStream = compressor.LZSSCompress(Encoding.UTF8.GetString(inputBytes));
        Console.WriteLine($"LZSS Compressed Size: {compressedStream.Count} bytes");

        //Extract LZSS Streams
        LZSSExtractor extractor = new LZSSExtractor();
        extractor.ExtractStreams(compressedStream);

        int CalculateExtractor = extractor.LiteralsStream.Count
                               + extractor.BackwardDistanceStream.Count
                               + extractor.MatchLengthStream.Count;

        Console.WriteLine($"Extractor Result: {CalculateExtractor} bytes");

        //Huffman Encoding
        var (compressedLiterals, literalsHeader) = ApplyHuffman(extractor.LiteralsStream);
        var (compressedBackwardDistance, backwardDistanceHeader) = ApplyHuffman(extractor.BackwardDistanceStream);
        var (compressedMatchLengths, matchLengthsHeader) = ApplyHuffman(extractor.MatchLengthStream);

        // Huffman Decoding
        List<byte> decompressedLiterals = DecompressHuffman(compressedLiterals, literalsHeader);
        List<byte> decompressedBackwardDistance = DecompressHuffman(compressedBackwardDistance, backwardDistanceHeader);
        List<byte> decompressedMatchLengths = DecompressHuffman(compressedMatchLengths, matchLengthsHeader);

        // Validation
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
    }

    static (List<byte> compressedData, List<byte> header) ApplyHuffman(List<byte> data)
    {
        if (data.Count == 0)
            return (new List<byte>(), new List<byte> { 0 });

        var frequency = data.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());
        var encoder = new HuffmanEncoder(frequency);
        var codes = encoder.GetCanonicalCodes();
        List<byte> header = encoder.GetHeader();
        StringBuilder encodedBits = new StringBuilder();

        foreach (var b in data)
            encodedBits.Append(codes[b]);

        int paddingRequired = (8 - (encodedBits.Length % 8)) % 8;
        encodedBits.Append('0', paddingRequired);
        List<byte> compressedData = new List<byte>();

        for (int i = 0; i < encodedBits.Length; i += 8)
        {
            compressedData.Add(Convert.ToByte(encodedBits.ToString(i, 8), 2));
        }

        header.Add((byte)paddingRequired);
        return (compressedData, header);
    }

    static List<byte> DecompressHuffman(List<byte> compressedData, List<byte> header)
    {
        if (compressedData.Count == 0)
            return new List<byte>();
        List<byte> compressedStream = compressor.LZSSCompress(input);
  
        StreamExtractor extractor = new StreamExtractor();
        extractor.ExtractStreams(compressedStream);

        var decoder = new HuffmanDecoder();
        int paddingRequired = header[^1];
        header.RemoveAt(header.Count - 1);
        decoder.BuildCanonicalCodesFromHeader(header);
        List<byte> decompressedData = new List<byte>();
        string currentCode = "";
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

        for (int i = 0; i < compressedData.Count; i++)
        {
            byte byteValue = compressedData[i];
            int bitsToRead = (i == compressedData.Count - 1) ? 8 - paddingRequired : 8;
        Dictionary<byte, char> byteToCharMap = compressor.CharToByteMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        LZSSDecompressor decompressor = new LZSSDecompressor();
        string originalText = decompressor.LZSSDecompress(compressedStream, byteToCharMap);

        if(input.Length == originalText.Length)
            Console.WriteLine("Valid");
        else
            Console.WriteLine("Not valid");
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
