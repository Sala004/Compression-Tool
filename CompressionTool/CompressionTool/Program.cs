using CompressionTool.Huffman;
using CompressionTool.Utilities;
using DecompressionTool;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.Diagnostics;
using CompressionTool.Compression;

class Program
{
    static void Main(string[] args)
    {
        string FileName = $@"..\..\..\Dataset\DataSet_1.tsv";
        Deflator.CompressAndCompare(FileName);
        Console.WriteLine("\n---------------------------------------------------\n");
        //Inflator.Decompress(result);
    }
}