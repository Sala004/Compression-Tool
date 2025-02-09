using CompressionTool.Utilities;
using DecompressionTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool.Compression
{
    internal class Inflator
    {
        public static string Decompress(CompressionResult compressedData)
        {
            Console.WriteLine($"Starting Decompression...");

            Stopwatch sw = Stopwatch.StartNew();

            // Huffman Decoding
            List<byte> decompressedLiterals = HuffmanDecoder.DecompressHuffman(compressedData.CompressedLiterals, compressedData.LiteralsHeader);
            List<byte> decompressedBackwardDistance = HuffmanDecoder.DecompressHuffman(compressedData.CompressedBackwardDistances, compressedData.BackwardDistanceHeader);
            List<byte> decompressedMatchLengths = HuffmanDecoder.DecompressHuffman(compressedData.CompressedMatchLengths, compressedData.MatchLengthHeader);

            int totalHuffmanDecompressedSize = decompressedLiterals.Count
                                 + decompressedBackwardDistance.Count
                                 + decompressedMatchLengths.Count;

            if (compressedData.CalculateExtractor == totalHuffmanDecompressedSize)
            {
                Console.WriteLine("Validation Passed: Decompressed size matches LZSS compressed size.");
            }
            else
            {
                Console.WriteLine($"Validation Failed: LZSS Compressed Size = {compressedData.CalculateExtractor} bytes, Huffman Decompressed Size = {totalHuffmanDecompressedSize} bytes.");
            }

            // Merge the streams back
            List<byte> mergedStream = StreamMerger.MergeStreams(
                compressedData.PaddingBits,
                compressedData.Flags,
                decompressedLiterals,
                decompressedMatchLengths,
                decompressedBackwardDistance);

            // Restore LZSS Stream
            Dictionary<byte, char> byteToCharMap = compressedData.CharToByteMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            // LZSS Decompression
            LZSSDecompressor decompressor = new LZSSDecompressor();
            string decompressedText = decompressor.LZSSDecompress(mergedStream, byteToCharMap);

            sw.Stop();
            Console.WriteLine($"Decompression completed in {sw.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"Decompressed Size: {Encoding.UTF8.GetByteCount(decompressedText)} bytes");

            return decompressedText;
        }
    }
}
