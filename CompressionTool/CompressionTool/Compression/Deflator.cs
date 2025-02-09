using CompressionTool.Huffman;
using CompressionTool.Utilities;
using DecompressionTool;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CompressionTool.Compression
{
    class Deflator
    {
        public static void CompressAndCompare(string filePath)
        {
            string fileText = File.ReadAllText(filePath, Encoding.UTF8);
            byte[] inputBytes = Encoding.UTF8.GetBytes(fileText);
            int originalSize = inputBytes.Length;

            Console.WriteLine($"Original File Size: {originalSize} bytes");

            // LZSS Compression
            Stopwatch sw = Stopwatch.StartNew();
            LZSSCompressor compressor = new LZSSCompressor();
            List<byte> compressedStream = compressor.LZSSCompress(fileText);
            sw.Stop();
            Console.WriteLine($"LZSS Compressed Size: {compressedStream.Count} bytes");
            double totalTime = sw.Elapsed.TotalSeconds;

            // Extract LZSS Streams
            StreamExtractor extractor = new StreamExtractor();
            extractor.ExtractStreams(compressedStream);

            int calculateExtractor = extractor.Literals.Count
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

            double compressionRatio = 1 - ((double)compressedSize / calculateExtractor);
            Console.WriteLine($"Huffman Compression Size: {compressedSize} bytes");
            Console.WriteLine($"Huffman Compression ratio: {compressionRatio:P2}");
            Console.WriteLine($"Time: {totalTime:F2}s");

            // Return compressed data
            //return new CompressionResult
            //{
            //    CalculateExtractor = calculateExtractor,
            //    CompressedLZSSStream = compressedStream,
            //    CompressedLiterals = compressedLiterals,
            //    CompressedBackwardDistances = compressedBackwardDistance,
            //    CompressedMatchLengths = compressedMatchLengths,
            //    CharToByteMap = compressor.CharToByteMap,
            //    LiteralsHeader = literalsHeader,
            //    BackwardDistanceHeader = backwardDistanceHeader,
            //    MatchLengthHeader = matchLengthsHeader,
            //    PaddingBits = extractor.PaddingBits,
            //    Flags = extractor.Flags,
            //};
        }
    }
}
