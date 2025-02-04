using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool.Utilities
{
    internal class TextToByteConverter
    {
        private Dictionary<char, byte> _alphabet;
        private List<byte> _bytes;
        private string FileName;

        public TextToByteConverter(string fileName)
        {
            FileName = fileName;
            _alphabet = new Dictionary<char, byte>();
            _bytes = new List<byte>();
        }

        private void LoadSymbolDictionary()
        {
            InputReader InputReader = new InputReader();

            _alphabet = InputReader.ReadSymbolDictionary();
        }

        public int GetOriginalSize()
        {
            string FilePath = @"..\..\Dataset\" + FileName + ".tsv";
            byte[] tmp = File.ReadAllBytes(FilePath);
            return tmp.Length;
        }

        private void ConvertText(string Text)
        {
            for (int i = 0; i < Text.Length; i++)
            {
                _bytes.Add(_alphabet[Text[i]]);
            }
        }

        public List<byte> Convert()
        {
            InputReader InputReader = new InputReader();
            string Text = InputReader.ReadOriginalFile(FileName);

            LoadSymbolDictionary();

            ConvertText(Text);

            return  _bytes;
        }
    }
}
