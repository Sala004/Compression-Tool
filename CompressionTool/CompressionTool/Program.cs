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

        string FileName = @"..\..\..\Dataset\DataSet_2.tsv";

        byte[] inputBytes = File.ReadAllBytes(FileName);
        int originalSize = inputBytes.Length;
        Console.WriteLine($"Original File Size: {originalSize} bytes");

        //LZSS Compression
        LZSSCompressor compressor = new LZSSCompressor();
        List<byte> compressedStream = compressor.LZSSCompress(Encoding.UTF8.GetString(inputBytes));
        Console.WriteLine($"LZSS Compressed Size: {compressedStream.Count} bytes");


        //Extract LZSS Streams
        StreamExtractor extractor = new StreamExtractor();
        extractor.ExtractStreams(compressedStream);

        int CalculateExtractor = extractor.Literals.Count
                               + extractor.BackwardDistances.Count
                               + extractor.MatchLengths.Count;

        //Console.WriteLine($"Extractor Result: {CalculateExtractor} bytes");

        //Huffman Encoding
        Console.WriteLine("------------------------------------------------------------");
        var (compressedLiterals, literalsHeader) = ApplyHuffman(extractor.Literals);
        var (compressedBackwardDistance, backwardDistanceHeader) = ApplyHuffman(extractor.BackwardDistances);
        var (compressedMatchLengths, matchLengthsHeader) = ApplyHuffman(extractor.MatchLengths);

        int sum = (compressedLiterals.Count + compressedBackwardDistance.Count + compressedMatchLengths.Count);


        double compressionRatio = 1 - (double)sum / originalSize;
        Console.WriteLine("Compression ratio            : " + compressionRatio.ToString("P2"));

        Console.WriteLine("------------------------------------------------------------");



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
        // Now merge the streams back into the original compressed stream.
        List<byte> mergedStream = StreamMerger.MergeStreams(
            extractor.PaddingBits,
            extractor.Flags,
            decompressedLiterals,
            decompressedMatchLengths,
            decompressedBackwardDistance);

        Dictionary<byte, char> byteToCharMap = compressor.CharToByteMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        LZSSDecompressor decompressor = new LZSSDecompressor();


        string originalText = decompressor.LZSSDecompress(mergedStream, byteToCharMap);

        Console.WriteLine(mergedStream.SequenceEqual(compressedStream));


        string fileText = File.ReadAllText(FileName);
        if (originalText == fileText)
        {
            Console.WriteLine("Valid");
        }
        else
        {
            Console.WriteLine("NotValid");
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

//using CompressionTool.Huffman;
//using CompressionTool.Utilities;
//using DecompressionTool;
//using System.Text;
//using System.IO;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//class Program
//{
//    static void Main(string[] args)
//    {

//        string FileName = @"..\..\..\Dataset\DataSet_2.tsv";

//        byte[] inputBytes = File.ReadAllBytes(FileName);
//        int originalSize = inputBytes.Length;
//        Console.WriteLine($"Original File Size: {originalSize} bytes");

//        //LZSS Compression
//        LZSSCompressor compressor = new LZSSCompressor();
//        List<byte> compressedStream = compressor.LZSSCompress(Encoding.UTF8.GetString(inputBytes));
//        Console.WriteLine($"LZSS Compressed Size: {compressedStream.Count} bytes");


//        //Extract LZSS Streams
//        StreamExtractor extractor = new StreamExtractor();
//        extractor.ExtractStreams(compressedStream);

//        int CalculateExtractor = extractor.Literals.Count
//                               + extractor.BackwardDistances.Count
//                               + extractor.MatchLengths.Count;

//        //Console.WriteLine($"Extractor Result: {CalculateExtractor} bytes");

//        //Huffman Encoding
//        Console.WriteLine("------------------------------------------------------------");
//        var (compressedLiterals, literalsHeader) = ApplyHuffman(extractor.Literals);
//        var (compressedBackwardDistance, backwardDistanceHeader) = ApplyHuffman(extractor.BackwardDistances);
//        var (compressedMatchLengths, matchLengthsHeader) = ApplyHuffman(extractor.MatchLengths);

//        int sum = compressedStream.Count;


//        double compressionRatio = 1 - (double)sum / originalSize ;
//        Console.WriteLine("Compression ratio            : " + compressionRatio.ToString("P2"));

//        Console.WriteLine("------------------------------------------------------------");



//        // Huffman Decoding
//        List<byte> decompressedLiterals = DecompressHuffman(compressedLiterals, literalsHeader);
//        List<byte> decompressedBackwardDistance = DecompressHuffman(compressedBackwardDistance, backwardDistanceHeader);
//        List<byte> decompressedMatchLengths = DecompressHuffman(compressedMatchLengths, matchLengthsHeader);

//        // Validation
//        int totalHuffmanDecompressedSize = decompressedLiterals.Count
//                                         + decompressedBackwardDistance.Count
//                                         + decompressedMatchLengths.Count;

//        if (CalculateExtractor == totalHuffmanDecompressedSize)
//        {
//            Console.WriteLine("Validation Passed: Decompressed size matches LZSS compressed size.");
//        }
//        else
//        {
//            Console.WriteLine($"Validation Failed: LZSS Compressed Size = {compressedStream.Count} bytes, Huffman Decompressed Size = {totalHuffmanDecompressedSize} bytes.");
//        }
//        // Now merge the streams back into the original compressed stream.
//        List<byte> mergedStream = StreamMerger.MergeStreams(
//            extractor.PaddingBits,
//            extractor.Flags,
//            decompressedLiterals,
//            decompressedMatchLengths,
//            decompressedBackwardDistance);

//        Dictionary<byte, char> byteToCharMap = compressor.CharToByteMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
//        LZSSDecompressor decompressor = new LZSSDecompressor();


//        string originalText = decompressor.LZSSDecompress(mergedStream, byteToCharMap);

//        Console.WriteLine(mergedStream.SequenceEqual(compressedStream));


//        string fileText=File.ReadAllText(FileName);
//        if(originalText==fileText)
//        {
//            Console.WriteLine("Valid");
//        }
//        else
//        {
//            Console.WriteLine("NotValid");
//        }



//    }

//    static (List<byte> compressedData, List<byte> header) ApplyHuffman(List<byte> data)
//    {
//        if (data.Count == 0)
//            return (new List<byte>(), new List<byte> { 0 });

//        var frequency = data.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());
//        var encoder = new HuffmanEncoder(frequency);
//        var codes = encoder.GetCanonicalCodes();
//        List<byte> header = encoder.GetHeader();
//        StringBuilder encodedBits = new StringBuilder();

//        foreach (var b in data)
//            encodedBits.Append(codes[b]);

//        int paddingRequired = (8 - (encodedBits.Length % 8)) % 8;
//        encodedBits.Append('0', paddingRequired);
//        List<byte> compressedData = new List<byte>();

//        for (int i = 0; i < encodedBits.Length; i += 8)
//        {
//            compressedData.Add(Convert.ToByte(encodedBits.ToString(i, 8), 2));
//        }

//        header.Add((byte)paddingRequired);
//        return (compressedData, header);
//    }

//    static List<byte> DecompressHuffman(List<byte> compressedData, List<byte> header)
//    {
//        if (compressedData.Count == 0)
//            return new List<byte>();

//        var decoder = new HuffmanDecoder();
//        int paddingRequired = header[^1];
//        header.RemoveAt(header.Count - 1);
//        decoder.BuildCanonicalCodesFromHeader(header);
//        List<byte> decompressedData = new List<byte>();
//        string currentCode = "";

//        for (int i = 0; i < compressedData.Count; i++)
//        {
//            byte byteValue = compressedData[i];
//            int bitsToRead = (i == compressedData.Count - 1) ? 8 - paddingRequired : 8;

//            for (int j = 7; j >= 8 - bitsToRead; j--)
//            {
//                currentCode += ((byteValue >> j) & 1) == 1 ? '1' : '0';
//                if (decoder.ReverseCodebook.TryGetValue(currentCode, out byte decodedByte))
//                {
//                    decompressedData.Add(decodedByte);
//                    currentCode = "";
//                }
//            }
//        }
//        return decompressedData;
//    }
//}

