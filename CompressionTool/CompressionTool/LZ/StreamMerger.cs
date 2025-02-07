using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool.Utilities
{
    public static class StreamMerger
    {
        /// <summary>
        /// Given the lists produced by StreamExtractor (including the flag list),
        /// this method reconstructs the original compressed byte stream.
        /// </summary>
        /// <param name="flags">List of booleans indicating the type of each code (false = literal, true = match).</param>
        /// <param name="literals">List of literal bytes (for codes with flag = 0).</param>
        /// <param name="matchLengths">List of match lengths (for codes with flag = 1).</param>
        /// <param name="backwardDistances">List of backward distances (for codes with flag = 1), each stored as a List<byte>.</param>
        /// <returns>The merged (reconstructed) compressed stream as a list of bytes.</returns>
        public static List<byte> MergeStreams(List<bool> flags, List<byte> literals, List<byte> matchLengths, List<List<byte>> backwardDistances)
        {
            if (flags == null)
                throw new ArgumentNullException(nameof(flags));
            if (literals == null || matchLengths == null || backwardDistances == null)
                throw new ArgumentNullException("None of the input streams can be null.");

            int literalIndex = 0;
            int matchIndex = 0;
            List<bool> bitStream = new List<bool>();

            foreach (bool flag in flags)
            {
                if (!flag)
                {
                    bitStream.Add(false);
                    if (literalIndex >= literals.Count)
                        throw new InvalidOperationException("Not enough literal values.");
                    byte literal = literals[literalIndex++];
                    for (int i = 7; i >= 0; i--)
                        bitStream.Add(((literal >> i) & 1) == 1);
                }
                else
                {
                    bitStream.Add(true);
                    if (matchIndex >= matchLengths.Count || matchIndex >= backwardDistances.Count)
                        throw new InvalidOperationException("Not enough match values.");

                    // Reconstruct the 19-bit backward distance from 3 bytes.
                    List<byte> backwardBytes = backwardDistances[matchIndex];
                    if (backwardBytes == null || backwardBytes.Count != 3)
                        throw new InvalidOperationException("Invalid backward distance representation. Expected 3 bytes.");

                    int backward = (backwardBytes[0] << 16) | (backwardBytes[1] << 8) | backwardBytes[2];

                    byte matchLen = matchLengths[matchIndex];
                    matchIndex++;

                    for (int i = 18; i >= 0; i--)
                        bitStream.Add(((backward >> i) & 1) == 1);
                    for (int i = 7; i >= 0; i--)
                        bitStream.Add(((matchLen >> i) & 1) == 1);
                }
            }

            int paddingBits = (8 - (bitStream.Count % 8)) % 8;
            for (int i = 0; i < paddingBits; i++)
                bitStream.Add(false);

            List<byte> outputBytes = new List<byte>();
            outputBytes.Add((byte)paddingBits);

            for (int i = 0; i < bitStream.Count; i += 8)
            {
                byte b = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (bitStream[i + j])
                        b |= (byte)(1 << (7 - j));
                }
                outputBytes.Add(b);
            }

            return outputBytes;
        }
    }
}
