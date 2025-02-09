using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class LZSSCompressor
{
    // Public property to store the character to byte mapping
    public Dictionary<char, byte> CharToByteMap { get; private set; }

    public List<byte> LZSSCompress(string inputText)
    {
        // Create the character mapping before converting the text to a byte stream
        CharToByteMap = CreateCharacterMapping(inputText);
        List<byte> byteStream = ConvertTextToByteStream(inputText);

        #region TestTheMaping
        //Console.WriteLine("\nCharToByteMap : ");
        //foreach(var C in CharToByteMap)
        //{
        //    Console.WriteLine($"Char :{C.Key},Byte {C.Value}");

        //}
        //Console.WriteLine("\nByteStream : ");
        //foreach(var B in byteStream)
        //{
        //    Console.Write(B);

        //}
        //Console.WriteLine();
        #endregion

        return CompressWithLZ77(byteStream);
    }

    private List<byte> ConvertTextToByteStream(string fileContent)
    {
        List<byte> byteStream = new List<byte>();
        foreach (char C in fileContent)
        {
            byteStream.Add(CharToByteMap[C]);
        }
        return byteStream;
    }

    private Dictionary<char, byte> CreateCharacterMapping(string fileContent)
    {
        Dictionary<char, byte> charToByte = new Dictionary<char, byte>();
        byte value = 0;
        foreach (char C in fileContent)
        {
            if (!charToByte.ContainsKey(C))
            {
                charToByte.Add(C, value);
                value++;
            }
        }
        return charToByte;
    }

    private List<byte> CompressWithLZ77(List<byte> input)
    {
        const int SearchBufferSize = 512 * 1024;
        const int LookaheadBufferSize = 259;
        const int MinMatchLength = 4;

        List<bool> bitStream = new List<bool>();
        int inputLength = input.Count;
        int currentIndex = 0;
        Dictionary<string, List<int>> substringTable = new Dictionary<string, List<int>>();

        while (currentIndex < inputLength)
        {
            int maxMatchLength = 0;
            int bestMatchDistance = 0;

                string keyString=null;
                
               keyString = GetSubstringKey(input, currentIndex, MinMatchLength);

            #region //test SubStringTable
            //Console.WriteLine("\nCurrent index : " + currentIndex);
            //    Console.WriteLine("\nkeyString : " + keyString);

            //    Console.WriteLine("\nSubStringTable : " );
            //    foreach(var D in substringTable)
            //    {
            //        Console.Write(D.Key + " : ");

            //            foreach(int x in D.Value)
            //    {
            //        Console.Write(x);
            //    }
            //    Console.WriteLine();

            //    }
            #endregion


            if (!string.IsNullOrEmpty(keyString) && substringTable.ContainsKey(keyString))
                {
                    foreach (int startIndex in substringTable[keyString])
                    {
                        int matchLength = 0;
                        while (currentIndex + matchLength < inputLength &&
                               matchLength < LookaheadBufferSize &&
                               input[startIndex + matchLength] == input[currentIndex + matchLength])
                        {
                            matchLength++;
                        }

                        if (matchLength > maxMatchLength)
                        {
                            maxMatchLength = matchLength;
                            bestMatchDistance = currentIndex - startIndex;
                        }
                    }
                }
            
            if (!string.IsNullOrEmpty(keyString) && !substringTable.ContainsKey(keyString))
            {
                substringTable[keyString] = new List<int>();
                substringTable[keyString].Add(currentIndex);
                       
            }
            
            if (maxMatchLength >= MinMatchLength)
            {
                //Console.WriteLine($"find : {bestMatchDistance},{maxMatchLength}");
                bitStream.Add(true);
                AddBits(bitStream, bestMatchDistance - 1, 19);
                AddBits(bitStream, maxMatchLength - 4, 8);
                currentIndex += maxMatchLength;
            }
            else
            {
                //Console.WriteLine("can't find : "+input[currentIndex]);
                bitStream.Add(false);
                AddBits(bitStream, input[currentIndex], 8);
                currentIndex++;
            }
        }
        #region //test bitStream
        //    Console.WriteLine("\nbitStream.Count : " + bitStream.Count);
        //    Console.WriteLine("bitStream : ");

        //    // Iterate over the bitStream to print in the required format
        //for (int i = 0; i < bitStream.Count;)
        //{
        //    if (bitStream[i]) // If match found (1)
        //    {
        //        Console.Write("1 ");
        //        i++;

        //        // Print the next 19-bit value (distance)
        //        for (int j = 0; j < 19 && i < bitStream.Count; j++, i++)
        //        {
        //            Console.Write(bitStream[i] ? "1" : "0");
        //        }
        //        Console.Write(" ");

        //        // Print the next 8-bit value (match length)
        //        for (int j = 0; j < 8 && i < bitStream.Count; j++, i++)
        //        {
        //            Console.Write(bitStream[i] ? "1" : "0");
        //        }
        //    }
        //    else // If match not found (0)
        //    {
        //        Console.Write("0 ");
        //        i++;

        //        // Print the next 8-bit value (raw character)
        //        for (int j = 0; j < 8 && i < bitStream.Count; j++, i++)
        //        {
        //            Console.Write(bitStream[i] ? "1" : "0");
        //        }
        //    }
        //    Console.WriteLine(); // New line for each entry
        //}
        #endregion


        return ConvertBitStreamToBytes(bitStream);
    }

    private void AddBits(List<bool> bitStream, int value, int numBits)
    {
        for (int i = numBits - 1; i >= 0; i--)
        {
            bitStream.Add(((value >> i) & 1) == 1);
        }
    }

    private string GetSubstringKey(List<byte> data, int startIndex, int length)
{
    if (data == null || startIndex < 0 || length < 0 || startIndex + length > data.Count)
        return null;

    StringBuilder result = new StringBuilder();
    for (int i = startIndex; i < startIndex + length; i++)
    {
        result.Append(data[i].ToString());
    }

    return result.ToString();
}


    private List<byte> ConvertBitStreamToBytes(List<bool> bitStream)
    {
        int padding = (8 - (bitStream.Count % 8)) % 8;
        List<byte> byteStream = new List<byte> { (byte)padding };



        for (int i = 0; i < bitStream.Count; i += 8)
        {
            byte b = 0;
            for (int j = 0; j < 8 && i + j < bitStream.Count; j++)
            {
                if (bitStream[i + j])
                {
                    b |= (byte)(1 << (7 - j));
                }
            }
            byteStream.Add(b);
        }
        #region testbyteStream
        //Console.WriteLine("byteStream.Count : " + byteStream.Count);
        //Console.Write("btyeStream : " );
        //foreach(byte B in byteStream)
        //{
        //    Console.Write(B + ",");
        //}
        //Console.WriteLine();
        #endregion
        return byteStream;
    }
}