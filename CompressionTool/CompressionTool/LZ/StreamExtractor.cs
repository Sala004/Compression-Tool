using System;
using System.Collections.Generic;
using System.IO;
using CompressionTool.Utilities;

namespace CompressionTool.Utilities
{
    public class StreamExtractor
    { 
        public List<byte> Literals { get; private set; }
    
        public List<byte> MatchLengths { get; private set; }
        
        // Now each backward distance is stored as a List<byte> (3 bytes per value)
        public List<List<byte>> BackwardDistances { get; private set; }
        
        public List<bool> Flags { get; private set; }
   
        public int PaddingBits { get; private set; }

        /// <summary>
        /// Given the compressed stream (with first byte = padding bits count),
        /// this method reads the bits one code at a time and populates the Literals,
        /// MatchLengths, BackwardDistances and Flags lists.
        /// </summary>
        /// <param name="compressedStream">The full compressed stream as a list of bytes.</param>
        public void ExtractStreams(List<byte> compressedStream)
        {
            if (compressedStream == null || compressedStream.Count == 0)
                throw new ArgumentException("Invalid compressed stream.");

            Literals = new List<byte>();
            MatchLengths = new List<byte>();
            BackwardDistances = new List<List<byte>>();
            Flags = new List<bool>();

            PaddingBits = compressedStream[0];

            int totalBits = (compressedStream.Count - 1) * 8 - PaddingBits;
            int bitIndex = 0; 

            int ReadBits(int count)
            {
                int value = 0;
                for (int i = 0; i < count; i++)
                {
                    if (bitIndex >= totalBits)
                        throw new InvalidOperationException("Not enough bits in the stream.");
                    
                    int byteIndex = 1 + (bitIndex / 8);
                    int bitInByte = bitIndex % 8;
                    int shift = 7 - bitInByte;  
                    int bit = (compressedStream[byteIndex] >> shift) & 1;
                    
                    value = (value << 1) | bit;
                    bitIndex++;
                }
                return value;
            }

            while (bitIndex < totalBits)
            {
                int flag = ReadBits(1);
                if (flag == 0)
                {
                    byte literal = (byte)ReadBits(8);
                    Literals.Add(literal);
                    Flags.Add(false);
                }
                else
                {
                    // Read the 19-bit backward distance.
                    int backward = ReadBits(19);
                    byte matchLen = (byte)ReadBits(8);

                    // Convert the 19-bit integer into 3 bytes (big-endian)
                    List<byte> backwardBytes = new List<byte>
                    {
                        (byte)((backward >> 16) & 0xFF),  // Only lower 3 bits are used.
                        (byte)((backward >> 8) & 0xFF),
                        (byte)(backward & 0xFF)
                    };
                    BackwardDistances.Add(backwardBytes);
                    MatchLengths.Add(matchLen);
                    Flags.Add(true);
                }
            }
        }
    }
}
