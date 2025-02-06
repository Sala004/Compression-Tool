class LZSSExtractor
{
    public List<byte> LiteralsStream { get; private set; }
    public List<byte> MatchLengthStream { get; private set; }
    public List<byte> BackwardDistanceStream { get; private set; }

    public void ExtractStreams(List<byte> compressedStream)
    {
        LiteralsStream = new List<byte>();
        MatchLengthStream = new List<byte>();
        BackwardDistanceStream = new List<byte>();

        int padding = compressedStream[0];
        int bitIndex = 8; // Skip the padding byte

        while (bitIndex < compressedStream.Count * 8 - padding)
        {
            bool flag = GetBit(compressedStream, bitIndex);
            bitIndex++;

            if (!flag)
            {
                // Uncompressed literal
                byte literal = (byte)GetBits(compressedStream, bitIndex, 8);
                LiteralsStream.Add(literal);
                bitIndex += 8;
            }
            else
            {
                // Compressed match
                int backwardDistance = GetBits(compressedStream, bitIndex, 19);
                bitIndex += 19;
                int matchLength = GetBits(compressedStream, bitIndex, 8) + 4;
                bitIndex += 8;

                BackwardDistanceStream.Add((byte)(backwardDistance >> 11)); // Only store the most significant 8 bits
                MatchLengthStream.Add((byte)(matchLength - 4));
            }
        }
    }

    private bool GetBit(List<byte> data, int bitIndex)
    {
        int byteIndex = bitIndex / 8;
        int bitOffset = bitIndex % 8;
        return ((data[byteIndex] >> (7 - bitOffset)) & 1) == 1;
    }

    private int GetBits(List<byte> data, int bitIndex, int numBits)
    {
        int result = 0;
        for (int i = 0; i < numBits; i++)
        {
            if (GetBit(data, bitIndex + i))
            {
                result |= 1 << (numBits - 1 - i);
            }
        }
        return result;
    }
}
