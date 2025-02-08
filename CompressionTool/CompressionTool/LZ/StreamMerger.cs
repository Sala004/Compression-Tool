using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool.Utilities
{
       /// <summary>
    /// The merger takes the separate streams (plus the token–flags and the original padding–bits count)
    /// and reassembles the original compressed byte stream.
    /// </summary>
    public class StreamMerger
    {
        /// <summary>
        /// Merges the three streams (and the token order) into the original compressed stream.
        /// </summary>
        /// <param name="paddingBits">
        /// The padding bits count (the first byte of the original stream).
        /// </param>
        /// <param name="flags">
        /// The token order: false = literal token; true = match token.
        /// </param>
        /// <param name="literals">
        /// The literal bytes (one byte per literal token).
        /// </param>
        /// <param name="matchLengths">
        /// The match lengths (one byte per match token).
        /// </param>
        /// <param name="backwardDistances">
        /// The backward distances (three bytes per match token, big-endian; only 19 bits used).
        /// </param>
        /// <returns>The reconstructed compressed byte stream.</returns>
        public static List<byte> MergeStreams(
            byte paddingBits,
            List<bool> flags,
            List<byte> literals,
            List<byte> matchLengths,
            List<byte> backwardDistances)
        {
            BitWriter writer = new BitWriter();
            int literalIndex = 0;
            int matchLengthIndex = 0;
            int backwardDistanceIndex = 0;

            // Process tokens in the same order they were extracted.
            foreach (bool flag in flags)
            {
                if (!flag)
                {
                    // Literal token: write flag 0 and then 8 bits for the literal.
                    writer.WriteBit(0);
                    if (literalIndex >= literals.Count)
                        throw new Exception("Not enough literal bytes to merge.");
                    writer.WriteBits(literals[literalIndex], 8);
                    literalIndex++;
                }
                else
                {
                    // Match token: write flag 1, then 19 bits (backward distance) and 8 bits (match length).
                    writer.WriteBit(1);
                    if (backwardDistanceIndex + 3 > backwardDistances.Count)
                        throw new Exception("Not enough backward distance bytes to merge.");
                    // Reassemble the 19–bit integer from its three bytes.
                    int bd = (backwardDistances[backwardDistanceIndex] << 16)
                           | (backwardDistances[backwardDistanceIndex + 1] << 8)
                           | (backwardDistances[backwardDistanceIndex + 2]);
                    backwardDistanceIndex += 3;
                    // Ensure only the lower 19 bits are written.
                    bd = bd & 0x7FFFF; // 0x7FFFF == 524287 → 19 ones in binary.
                    writer.WriteBits(bd, 19);
                    if (matchLengthIndex >= matchLengths.Count)
                        throw new Exception("Not enough match length bytes to merge.");
                    writer.WriteBits(matchLengths[matchLengthIndex], 8);
                    matchLengthIndex++;
                }
            }

            // Get the data bytes from the BitWriter.
            List<byte> dataBytes = writer.GetBytes();

            // The original compressed stream begins with the padding bits count.
            List<byte> mergedStream = new List<byte> { paddingBits };
            mergedStream.AddRange(dataBytes);
            return mergedStream;
        }
    }
}

