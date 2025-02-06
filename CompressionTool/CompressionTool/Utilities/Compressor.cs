using CompressionTool.Huffman;
using CompressionTool.Utilities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompressionTool
{
    class Deflator
    {
        //        private Dictionary<byte, string> LiteralsCodeBook;
        //        private Dictionary<byte, string> MatchLengthsCodeBook;
        //        private Dictionary<byte, string> BackwardDistanceCodeBook;
        //        private Dictionary<byte, int> LiteralsCount;
        //        private Dictionary<byte, int> MatchLengthCount;
        //        private Dictionary<byte, int> BackwardDistanceCount;
        //        private List<byte> BackwardDistanceHeader;
        //        private List<byte> LiteralsHeader;
        //        private List<byte> MatchLengthsHeader;
        //        private List<byte> InputStream;
        //        private List<byte> CompressedOutput;
        //        private List<byte> LiteralsStream;
        //        private List<byte> MatchLengthsStream;
        //        private List<byte> BackwardDistanceStream;
        //        private List<byte> OriginalFile;
        //        private int InputStreamIndex;
        //        private byte InputBytePadding;
        //        private byte OutputBytePadding;
        //        private int OriginalFileSize;
        //        private string TextBuffer;

        //        private void ConvertOriginalFileToBytes(string FileName)
        //        {
        //            var textToByteConverter = new TextToByteConverter(FileName);
        //            OriginalFile = textToByteConverter.Convert();
        //            OriginalFileSize = textToByteConverter.GetOriginalFileSize();
        //        }

        //        private void EncodeLZ77()
        //        {
        //            var lz77Encoder = new LZSSEncoder();
        //            InputStream = lz77Encoder.LZSSCompress();
        //        }

        //        private void ExtractStreams()
        //        {
        //            var streamExtractor = new StreamExtractor(InputStream);
        //            LiteralsStream = streamExtractor.ExtractLiterals();
        //            MatchLengthsStream = streamExtractor.ExtractMatchLengths();
        //            BackwardDistanceStream = streamExtractor.ExtractBackwardDistances();
        //            InputBytePadding = streamExtractor.ExtractBytePadding();
        //        }

        //        private void GetSymbolFrequencies()
        //        {
        //            LiteralsCount = new Probability(LiteralsStream).GetCharactersCount();
        //            MatchLengthCount = new Probability(MatchLengthsStream).GetCharactersCount();
        //            BackwardDistanceCount = new Probability(BackwardDistanceStream).GetCharactersCount();
        //        }

        //        private void GetCodeBooksAndHeaders()
        //        {
        //            LiteralsCodeBook = new HuffmanEncoder().GetCodeBook(LiteralsCount);
        //            LiteralsHeader = new HuffmanEncoder().GetHeader(Constants.LiteralsHeaderSize);

        //            MatchLengthsCodeBook = new HuffmanEncoder().GetCodeBook(MatchLengthCount);
        //            MatchLengthsHeader = new HuffmanEncoder().GetHeader(Constants.MatchLengthsHeaderSize);

        //            BackwardDistanceCodeBook = new HuffmanEncoder().GetCodeBook(BackwardDistanceCount);
        //            BackwardDistanceHeader = new HuffmanEncoder().GetHeader(Constants.BackwardDistanceHeaderSize);
        //        }

        //        private char GetFlagBit()
        //        {
        //            BufferText();
        //            char flagBit = TextBuffer[0];
        //            TextBuffer = TextBuffer.Remove(0, Constants.Bit);
        //            return flagBit;
        //        }

        //        private string GetNextBits(int length)
        //        {
        //            BufferText();
        //            string bits = TextBuffer.Substring(0, length);
        //            TextBuffer = TextBuffer.Remove(0, length);
        //            return bits;
        //        }

        //        private void TrimPadding()
        //        {
        //            string paddedByte = Convert.ToString(InputStream[InputStream.Count - 1], 2).PadLeft(Constants.Byte, '0');
        //            InputStreamIndex++;
        //            TextBuffer += paddedByte.Substring(0, paddedByte.Length - InputBytePadding);
        //        }

        //        private void BufferText()
        //        {
        //            if (TextBuffer.Length >= Constants.BufferingSize) return;

        //            StringBuilder toBeDecoded = new StringBuilder();

        //            for (int i = InputStreamIndex; i < InputStream.Count && TextBuffer.Length + toBeDecoded.Length < Constants.BufferingSize; i++)
        //            {
        //                InputStreamIndex++;

        //                if (i == InputStream.Count - 1)
        //                {
        //                    TextBuffer += toBeDecoded.ToString();
        //                    TrimPadding();
        //                    return;
        //                }
        //                else
        //                {
        //                    string newByte = Convert.ToString(InputStream[i], 2).PadLeft(Constants.Byte, '0');
        //                    toBeDecoded.Append(newByte);
        //                }
        //            }

        //            TextBuffer += toBeDecoded.ToString();
        //        }

        //        private void EncodeHuffman()
        //        {
        //            int currentCode = Constants.CodeUnkown;
        //            StringBuilder toBeEncoded = new StringBuilder();

        //            BufferText();

        //            while (TextBuffer.Length > 0)
        //            {
        //                if (currentCode == Constants.CodeUnkown)
        //                {
        //                    char flagBit = GetFlagBit();
        //                    toBeEncoded.Append(flagBit);
        //                    currentCode = flagBit == Constants.Uncompressed ? Constants.CodeLiteral : Constants.CodeDistance;
        //                }
        //                else if (currentCode == Constants.CodeDistance)
        //                {
        //                    string backwardDistance = GetNextBits(Constants.BackwardDistanceCodewordLength);
        //                    byte higherByte = Convert.ToByte(backwardDistance.Substring(0, Constants.Byte), 2);
        //                    toBeEncoded.Append(BackwardDistanceCodeBook[higherByte]);
        //                    toBeEncoded.Append(backwardDistance.Substring(Constants.Byte, Constants.BackwardDistanceCodewordLength - Constants.Byte));
        //                    currentCode = Constants.CodeLength;
        //                }
        //                else if (currentCode == Constants.CodeLength)
        //                {
        //                    byte matchLength = Convert.ToByte(GetNextBits(Constants.MatchLengthCodewordLength), 2);
        //                    toBeEncoded.Append(MatchLengthsCodeBook[matchLength]);
        //                    currentCode = Constants.CodeUnkown;
        //                }
        //                else if (currentCode == Constants.CodeLiteral)
        //                {
        //                    byte literal = Convert.ToByte(GetNextBits(Constants.LiteralCodewordLength), 2);
        //                    toBeEncoded.Append(LiteralsCodeBook[literal]);
        //                    currentCode = Constants.CodeUnkown;
        //                }

        //                if (TextBuffer.Length == 0)
        //                    BufferText();
        //            }

        //            if (toBeEncoded.Length > 0)
        //            {
        //                OutputBytePadding = (byte)(Constants.Byte - toBeEncoded.Length);
        //                toBeEncoded.Append('0', OutputBytePadding);
        //                ToBinary(toBeEncoded.ToString());
        //            }

        //            CompressedOutput.Insert(Constants.LiteralsHeaderSize + Constants.MatchLengthsHeaderSize + Constants.BackwardDistanceHeaderSize, OutputBytePadding);
        //        }

        //        private void ToBinary(string encodedText)
        //        {
        //            int startIndex = 0;
        //            while (startIndex <= encodedText.Length - Constants.Byte)
        //            {
        //                byte stringByte = Convert.ToByte(encodedText.Substring(startIndex, Constants.Byte), 2);
        //                CompressedOutput.Add(stringByte);
        //                startIndex += Constants.Byte;
        //            }
        //        }

        //        private void BuildHeader()
        //        {
        //            AddHeader(LiteralsHeader);
        //            AddHeader(MatchLengthsHeader);
        //            AddHeader(BackwardDistanceHeader);
        //        }

        //        private void AddHeader(List<byte> header)
        //        {
        //            foreach (var byteItem in header)
        //            {
        //                CompressedOutput.Add(byteItem);
        //            }
        //        }

        //        private void ProduceFile(string fileName)
        //        {
        //            new OutputWriter().WriteFinalCompressedFile(CompressedOutput, fileName);
        //        }

        //        public Deflator()
        //        {
        //            LiteralsCodeBook = new Dictionary<byte, string>();
        //            MatchLengthsCodeBook = new Dictionary<byte, string>();
        //            BackwardDistanceCodeBook = new Dictionary<byte, string>();
        //            LiteralsCount = new Dictionary<byte, int>();
        //            MatchLengthCount = new Dictionary<byte, int>();
        //            BackwardDistanceCount = new Dictionary<byte, int>();
        //            BackwardDistanceHeader = new List<byte>();
        //            LiteralsHeader = new List<byte>();
        //            MatchLengthsHeader = new List<byte>();
        //            InputStream = new List<byte>();
        //            CompressedOutput = new List<byte>();
        //            LiteralsStream = new List<byte>();
        //            MatchLengthsStream = new List<byte>();
        //            BackwardDistanceStream = new List<byte>();
        //            OriginalFile = new List<byte>();
        //            TextBuffer = "";
        //            InputStreamIndex = 0;
        //            OutputBytePadding = 0;
        //            InputBytePadding = 0;
        //        }

        //        public int GetOriginalFileSize() => OriginalFileSize;

        //        public int GetCompressedFileSize() => CompressedOutput.Count;

        //        public double GetCompressionRatio() => 1 - (CompressedOutput.Count / (double)OriginalFileSize);

        //        public void Deflate(string fileName)
        //        {
        //            ConvertOriginalFileToBytes(fileName);
        //            EncodeLZ77();
        //            ExtractStreams();
        //            GetSymbolFrequencies();
        //            GetCodeBooksAndHeaders();
        //            BuildHeader();
        //            EncodeHuffman();
        //            ProduceFile(fileName);
        //        }
    }
}
