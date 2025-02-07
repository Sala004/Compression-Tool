using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LZSSDecompressor
{
    /// <summary>
    /// Decompresses the given compressed data (produced by LZSSCompressor)
    /// and returns the original text.
    /// </summary>
    /// <param name="compressedData">
    /// The compressed byte stream. The first byte holds the number of padding bits,
    /// followed by the bit-packed tokens.
    /// </param>
    /// <param name="byteToCharMap">
    /// The mapping from byte values to characters (inverse of LZSSCompressor.CharToByteMap).
    /// </param>
    /// <returns>The original text.</returns>
    public string LZSSDecompress(List<byte> compressedData, Dictionary<byte, char> byteToCharMap)
    {
        if (compressedData == null || compressedData.Count == 0)
            return string.Empty;

        // --- Step 1. Reconstruct the bit stream ---
        // The first byte of the compressedData is the number of padding bits 
        // that were added at the end of the bit stream.
        int padding = compressedData[0];

        // Convert each subsequent byte into 8 bits (MSB first).
        List<bool> bitStream = new List<bool>();
        for (int i = 1; i < compressedData.Count; i++)
        {
            byte currentByte = compressedData[i];
            for (int bit = 7; bit >= 0; bit--)
            {
                bool bitValue = ((currentByte >> bit) & 1) == 1;
                bitStream.Add(bitValue);
            }
        }
        // Remove the extra padding bits added during compression.
        if (padding > 0)
        {
            bitStream.RemoveRange(bitStream.Count - padding, padding);
        }

        // --- Step 2. Process the bit stream to reconstruct the original byte stream ---
        // The compressor writes a sequence of tokens.
        // Each token starts with a flag bit:
        //   - If the flag is 1 (true): the next 19 bits encode (distance-1)
        //     and the following 8 bits encode (match length-4).
        //   - If the flag is 0 (false): the next 8 bits encode a literal byte.
        List<byte> decompressedBytes = new List<byte>();
        int bitIndex = 0;
        while (bitIndex < bitStream.Count)
        {
            // Read the flag bit.
            bool flag = bitStream[bitIndex];
            bitIndex++;

            if (flag)
            {
                // --- Match token ---
                // Read 19 bits for (distance - 1).
                if (bitIndex + 19 + 8 > bitStream.Count)
                    break; // insufficient bits; data may be corrupted

                int distanceMinusOne = ReadBits(bitStream, ref bitIndex, 19);
                int distance = distanceMinusOne + 1;

                // Read 8 bits for (match length - 4).
                int lengthMinusFour = ReadBits(bitStream, ref bitIndex, 8);
                int matchLength = lengthMinusFour + 4;

                // Copy matchLength bytes from already decompressed data,
                // starting "distance" bytes back.
                int copyPosition = decompressedBytes.Count - distance;
                for (int i = 0; i < matchLength; i++)
                {
                    // For overlapping matches, new bytes are immediately available.
                    byte value = decompressedBytes[copyPosition + i];
                    decompressedBytes.Add(value);
                }
            }
            else
            {
                // --- Literal token ---
                // Read 8 bits that represent the raw literal byte.
                if (bitIndex + 8 > bitStream.Count)
                    break;

                int literal = ReadBits(bitStream, ref bitIndex, 8);
                decompressedBytes.Add((byte)literal);
            }
        }

        // --- Step 3. Convert the decompressed byte stream back to text ---
        // Use the provided mapping (inverse of LZSSCompressor.CharToByteMap).
        StringBuilder originalText = new StringBuilder();
        foreach (byte b in decompressedBytes)
        {
            if (byteToCharMap.TryGetValue(b, out char ch))
                originalText.Append(ch);
            else
                originalText.Append('?'); // or throw an exception if desired
        }

        return originalText.ToString();
    }

    /// <summary>
    /// Reads the next 'count' bits from the bit stream (starting at index)
    /// and returns their integer value. The bit order is assumed to be MSB first.
    /// </summary>
    private int ReadBits(List<bool> bitStream, ref int index, int count)
    {
        int value = 0;
        for (int i = 0; i < count; i++)
        {
            value = (value << 1) | (bitStream[index] ? 1 : 0);
            index++;
        }
        return value;
    }
}
