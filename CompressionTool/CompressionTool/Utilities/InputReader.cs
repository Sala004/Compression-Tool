using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool.Utilities
{
    internal class InputReader
    {
        public string ReadOriginalFile(string FileName)
        {
            string FilePath = @"..\..\..\Dataset\" + FileName + ".tsv";

            return File.ReadAllText(FilePath);
        }

        public Dictionary<char, byte> ReadSymbolDictionary()
        {
            byte Id = 0;
            Dictionary<char, byte> Alphabet = new Dictionary<char, byte>();
            string Text = File.ReadAllText(@"..\..\SymbolDictionary.txt", Encoding.UTF8);

            for(int i = 0; i < Text.Length; i++, Id++)
            {
                if (!Alphabet.ContainsKey(Text[i]))
                {
                    Alphabet.Add(Text[i], Id);
                }
            }

            return Alphabet;
        }
    }
}
