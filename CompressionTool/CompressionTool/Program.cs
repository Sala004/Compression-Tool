namespace CompressionTool
{
    public class HuffmanNode
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }

        public HuffmanNode(char symbol, int frequency)
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

    // Huffman Tree class
    public class HuffmanTree
    {
        private HuffmanNode Root { get; set; }

        public HuffmanTree(Dictionary<char, int> frequencies)
        {
            BuildTree(frequencies);
        }

        private void BuildTree(Dictionary<char, int> frequencies)
        {
            var priorityQueue = new PriorityQueue<HuffmanNode>();

            // Create a leaf node for each symbol and add it to the priority queue
            foreach (var symbol in frequencies)
            {
                priorityQueue.Enqueue(new HuffmanNode(symbol.Key, symbol.Value), symbol.Value);
            }

            // Build the Huffman tree
            while (priorityQueue.Count > 1)
            {
                var left = priorityQueue.Dequeue();
                var right = priorityQueue.Dequeue();

                var parent = new HuffmanNode(left.Frequency + right.Frequency)
                {
                    Left = left,
                    Right = right
                };

                priorityQueue.Enqueue(parent, parent.Frequency);
            }

            Root = priorityQueue.Dequeue();
        }

        public Dictionary<char, string> GetCodes()
        {
            var codes = new Dictionary<char, string>();
            Traverse(Root, "", codes);
            return codes;
        }

        private void Traverse(HuffmanNode node, string code, Dictionary<char, string> codes)
        {
            if (node == null) return;

            if (node.Left == null && node.Right == null)
            {
                codes[node.Symbol] = code;
                return;
            }

            Traverse(node.Left, code + "0", codes);
            Traverse(node.Right, code + "1", codes);
        }
    }

    // Priority Queue implementation for Huffman Nodes
    public class PriorityQueue<T>
    {
        private SortedDictionary<int, Queue<T>> _dictionary = new SortedDictionary<int, Queue<T>>();

        public int Count { get; private set; }

        public void Enqueue(T item, int priority)
        {
            if (!_dictionary.ContainsKey(priority))
            {
                _dictionary[priority] = new Queue<T>();
            }

            _dictionary[priority].Enqueue(item);
            Count++;
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            var first = _dictionary.First();
            var item = first.Value.Dequeue();

            if (first.Value.Count == 0)
            {
                _dictionary.Remove(first.Key);
            }

            Count--;
            return item;
        }
    }

    // Main program
    class Program
    {
        static void Main(string[] args)
        {
            string input = "Huffman coding is a data compression algorithm.";

            // Calculate frequency of each character
            var frequencies = new Dictionary<char, int>();
            foreach (var c in input)
            {
                if (frequencies.ContainsKey(c))
                {
                    frequencies[c]++;
                }
                else
                {
                    frequencies[c] = 1;
                }
            }

            // Build Huffman Tree
            var huffmanTree = new HuffmanTree(frequencies);

            // Get Huffman Codes
            var huffmanCodes = huffmanTree.GetCodes();

            // Display Huffman Codes
            Console.WriteLine("Huffman Codes:");
            foreach (var code in huffmanCodes)
            {
                Console.WriteLine($"{code.Key} : {code.Value}");
            }

            // Encode the input string
            string encodedString = "";
            foreach (var c in input)
            {
                encodedString += huffmanCodes[c];
            }

            Console.WriteLine("\nEncoded String:");
            Console.WriteLine(encodedString);
        }
    }
}
