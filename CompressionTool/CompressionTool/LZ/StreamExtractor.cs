using System;
using System.Collections.Generic;
using System.IO;

namespace CompressionTool.Utilities
{
    /// <summary>
    /// Reads the compressed stream (which begins with one padding‐bits byte) and “extracts”
    /// the tokens into three separate lists. In addition, it saves the token order (flag list)
    /// so that the streams can later be merged.
    /// </summary>
    public class StreamExtractor
    {
        /// <summary>
        /// The very first byte of the compressed stream tells you how many padding bits were appended.
        /// </summary>
        public byte PaddingBits { get; private set; }

        /// <summary>
        /// List of token flags – false means a literal token, true means a match token.
        /// </summary>
        public List<bool> Flags { get; private set; } = new List<bool>();

        /// <summary>
        /// For literal tokens: the literal bytes (each token adds one byte).
        /// </summary>
        public List<byte> Literals { get; private set; } = new List<byte>();

        /// <summary>
        /// For match tokens: the match length (each token adds one byte).
        /// </summary>
        public List<byte> MatchLengths { get; private set; } = new List<byte>();

        /// <summary>
        /// For match tokens: the backward distance is 19‐bits. This list stores them as three bytes per token (MSB first).
        /// </summary>
        public List<byte> BackwardDistances { get; private set; } = new List<byte>();

        /// <summary>
        /// Extracts the three “field” streams from the original compressed stream.
        /// </summary>
        /// <param name="compressedStream">The original compressed stream (first byte = # of padding bits).</param>
        public void ExtractStreams(List<byte> compressedStream)
        {
            if (compressedStream == null || compressedStream.Count == 0)
                throw new ArgumentException("Compressed stream is empty.");

            // The very first byte indicates how many bits were padded to complete the final byte.
            PaddingBits = compressedStream[0];

            // Total bits available in the data portion (after the first byte)
            int totalDataBits = (compressedStream.Count - 1) * 8 - PaddingBits;
            int bitPos = 0;

            // Process token by token until we run out of (non‐padding) bits.
            while (bitPos < totalDataBits)
            {
                // Each token begins with a flag bit.
                int flag = ReadBits(compressedStream, bitPos, 1);
                bitPos += 1;
                Flags.Add(flag == 1);

                if (flag == 0)
                {
                    // Literal token: next 8 bits represent the literal.
                    if (bitPos + 8 > totalDataBits)
                        throw new Exception("Unexpected end of stream reading literal token.");
                    int literalValue = ReadBits(compressedStream, bitPos, 8);
                    bitPos += 8;
                    Literals.Add((byte)literalValue);
                }
                else // flag == 1 → match token
                {
                    // Match token: next 19 bits = backward distance, then 8 bits = match length.
                    if (bitPos + 19 + 8 > totalDataBits)
                        throw new Exception("Unexpected end of stream reading match token.");
                    int backwardDistance = ReadBits(compressedStream, bitPos, 19);
                    bitPos += 19;
                    int matchLength = ReadBits(compressedStream, bitPos, 8);
                    bitPos += 8;

                    // Convert the 19-bit integer into three bytes (big-endian).
                    byte bd1 = (byte)((backwardDistance >> 16) & 0xFF);
                    byte bd2 = (byte)((backwardDistance >> 8) & 0xFF);
                    byte bd3 = (byte)(backwardDistance & 0xFF);
                    BackwardDistances.Add(bd1);
                    BackwardDistances.Add(bd2);
                    BackwardDistances.Add(bd3);

                    MatchLengths.Add((byte)matchLength);
                }
            }
        }

        /// <summary>
        /// Reads a specified number of bits from the compressed stream.
        /// The stream is assumed to start at index 1 (index 0 is the padding bits count).
        /// </summary>
        /// <param name="data">The entire compressed stream (including the first padding byte).</param>
        /// <param name="bitIndex">The bit offset (starting at 0 for the first bit of byte index 1).</param>
        /// <param name="count">How many bits to read.</param>
        /// <returns>The integer value represented by those bits.</returns>
        private int ReadBits(List<byte> data, int bitIndex, int count)
        {
            int value = 0;
            for (int i = 0; i < count; i++)
            {
                int overallBitIndex = bitIndex + i;
                // +1 because byte index 0 holds the padding info.
                int byteIndex = overallBitIndex / 8 + 1;
                // Read from the most-significant bit down.
                int bitOffset = 7 - (overallBitIndex % 8);
                int bit = (data[byteIndex] >> bitOffset) & 1;
                value = (value << 1) | bit;
            }
            return value;
        }
    }

    /// <summary>
    /// A simple helper class to write bits into a byte–aligned List&lt;byte&gt;.
    /// </summary>
    public class BitWriter
    {
        private List<byte> _bytes = new List<byte>();
        private int _currentByte = 0;
        private int _bitCount = 0; // how many bits have been written into _currentByte

        /// <summary>
        /// Writes a single bit (0 or 1).
        /// </summary>
        public void WriteBit(int bit)
        {
            _currentByte = (_currentByte << 1) | (bit & 1);
            _bitCount++;
            if (_bitCount == 8)
            {
                _bytes.Add((byte)_currentByte);
                _currentByte = 0;
                _bitCount = 0;
            }
        }

        /// <summary>
        /// Writes the lower <paramref name="bitCount"/> bits of <paramref name="value"/>, from MSB down.
        /// </summary>
        public void WriteBits(int value, int bitCount)
        {
            for (int i = bitCount - 1; i >= 0; i--)
            {
                int bit = (value >> i) & 1;
                WriteBit(bit);
            }
        }

        /// <summary>
        /// Returns the bytes written so far. If the current byte is not complete,
        /// it is padded with 0’s (to the right) to fill a complete byte.
        /// </summary>
        public List<byte> GetBytes()
        {
            if (_bitCount > 0)
            {
                int pad = 8 - _bitCount;
                _currentByte = _currentByte << pad;
                _bytes.Add((byte)_currentByte);
                _currentByte = 0;
                _bitCount = 0;
            }
            return _bytes;
        }
    }

}

