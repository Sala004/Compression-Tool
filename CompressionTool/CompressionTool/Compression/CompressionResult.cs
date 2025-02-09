using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool.Compression
{
    internal class CompressionResult
    {
        public int CalculateExtractor;
        public List<byte> CompressedLiterals { get; set; }
        public List<byte> CompressedBackwardDistances { get; set; }
        public List<byte> CompressedMatchLengths { get; set; }
        public List<byte> LiteralsHeader { get; set; }
        public List<byte>  BackwardDistanceHeader { get; set; }
        public List<byte>  MatchLengthHeader { get; set; }
        public List<byte> CompressedLZSSStream { get; set; }
        public Dictionary<char, byte> CharToByteMap { get; set; }
        public byte PaddingBits { get; set; }
        public List<bool> Flags { get; set; } = new List<bool>();
    }
}
