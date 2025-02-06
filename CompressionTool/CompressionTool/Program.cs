using CompressionTool.Utilities;

class Program
{
    static void Main(string[] args)
    {
        InputReader _inputReader = new InputReader();
        string Input = _inputReader.ReadOriginalFile("DataSet_1");

        LZSSCompressor compressor = new LZSSCompressor();
        List<byte> compressedStream = compressor.LZSSCompress(Input);


        var _charToByteMap = compressor.CharToByteMap;

        LZSSExtractor extractor = new LZSSExtractor();
        extractor.ExtractStreams(compressedStream);

        Console.WriteLine("Literals Stream: " + string.Join(", ", extractor.LiteralsStream));
        Console.WriteLine("Match Length Stream: " + string.Join(", ", extractor.MatchLengthStream));
        Console.WriteLine("Backward Distance Stream: " + string.Join(", ", extractor.BackwardDistanceStream));
    }
}