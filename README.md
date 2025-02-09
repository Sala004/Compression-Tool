# Compression Tool
This tool is designed for efficient text compression by combining two powerful algorithms: **LZSS** and **Huffman coding**. Leveraging this hybrid approach, the tool achieves high compression ratios and fast performance—consistently reaching around **70% compression on ten different datasets** and completing compression in **less than one second**.


## Features

- **Hybrid Compression:** Combines **LZSS** and **Huffman coding** to maximize compression ratio and speed.
- The algorithm works with any dataset containing any language or symbols without any frequency limitations because the dictionary is generated on the fly based on the data.
- **Fast Execution:** Optimizes both memory and performance to deliver compression in less than one second.
- **Efficient Bitwise Operations:** Reduces output size with compact bitstream representations.

## Algorithms Used

### 1. LZSS (Lempel-Ziv-Storer-Szymanski) Compression

**LZSS** is a dictionary-based compression algorithm that replaces repeated occurrences of data with references to a single copy stored in a dynamically generated dictionary.

#### Expected Compression Ratio

- **LZSS Alone:** Achieves an average compression ratio of approximately **50%**, depending on the dataset.
```plaintext
Original File Size: 1929659 bytes
LZSS Compressed Size: 820435 bytes
LZSS Compression ratio: 57.48%
```
```plaintext
Original File Size: 1843146 bytes
LZSS Compressed Size: 758553 bytes
LZSS Compression ratio: 58.84%
```

#### Implementation Details

- **Search Buffer:** 512 KB
- **Lookahead Buffer:** 259 bytes
- **Encoding:** Matches are encoded using a **(distance, length)** pair to reduce redundancy; unmatched characters are stored directly.
- **Mapping:** A character-to-byte mapping is created to convert text into a compact byte stream.

#### Optimizations Applied

- **Improved Substring Matching:**
    - Uses a substring hash table to quickly locate repeated sequences.
    - Reduces search time by storing and looking up fixed-length substrings.
- **Efficient Bitstream Representation:**
    - Implements bitwise operations to store match lengths and distances compactly.
    - Minimizes wasted space in the output bitstream.
- **Enhanced Lookahead Buffer:**
    - A larger lookahead buffer (259 bytes) compared to traditional implementations (e.g., 32 or 64 bytes) allows for detecting longer matches.
    - An expanded search buffer increases the likelihood of finding repeated sequences.

### 2. Huffman Compression

Regular Huffman encoding assigns binary codes to symbols based on a tree structure. However, since the exact tree structure can vary, we need to store the entire tree for decoding, which isn’t always space-efficient.

In our application, we take it a step further by using Canonical Huffman Encoding. Instead of relying on tree structure, we first sort the symbols by code length and assign binary values in a structured, predictable way. This lets us store only the symbol lengths instead of the full tree, making decoding simpler and reducing overhead.

But the real optimization comes from how we handle the compressed data. Our compressed stream isn’t just raw bytes—it consists of three separate streams:

 1. **Literals** (actual byte values)
 2. **Backward distances** (how far back we reference repeated data)
 3. **Match lengths** (how many bytes we match from previous data)

Instead of applying Huffman encoding to the entire compressed stream at once, we first use a stream extractor to separate these components. Then, we apply Huffman encoding to each stream individually, allowing for better compression ratios.

#### How much better? Here’s the impact:
##### Applying Huffman to the entire compressed stream directly:
```plaintext
LZSS Compressed Size: 758553 bytes
Huffman Compressed Size: 699346 bytes
Final Huffman Compression Ratio: 7.81%
```
##### Applying Huffman to separate streams individually:
```plaintext
LZSS Compressed Size: 758553 bytes
Huffman Compressed Size: 608801 bytes
Final Huffman Compression Ratio: 27.92%
```

By separating the streams and compressing them individually, we achieved a significant improvement, reducing the final size from **699,346 bytes to 608,801 bytes**—a much stronger compression ratio of **27.92% instead of just 7.81%**.