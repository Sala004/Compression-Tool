using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool.Huffman
{
    internal class HuffmanNode
    {

        public byte Symbol { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }

        public HuffmanNode(byte symbol, int frequency)
        {
            Symbol = symbol;
            Frequency = frequency;
            Left = null;
            Right = null;
        }

        public HuffmanNode(int frequency)
        {
            Frequency = frequency;
            Left = null;
            Right = null;
        }
    }
}
