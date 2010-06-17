using System;
using nBinLib.Reader;

namespace nBinLib.BitReaders
{
    public class HuffmanReader<T>
    {
        private readonly int _maxSize;
        private readonly T[] _codeTable;
        private readonly int[] _lengthTable;

        public HuffmanReader(int maxSize)
        {
            _maxSize = maxSize;
            _codeTable = new T[1 << maxSize];
            _lengthTable = new int[1 << maxSize];

            for (var i = 0; i < 1 << maxSize; i++)
                _lengthTable[i] = -1;
        }

        public void Add(string bitString, T val)
        {
            uint value;
            int length;
            NormalizeString(bitString, out value, out length);
            var bitsToFill = _maxSize - length;

            var prefix = value << bitsToFill;

            for (var i = 0; i < 1 << bitsToFill; i++)
            {
                if (_lengthTable[prefix | i] != -1)
                    throw new Exception();

                _codeTable[prefix | i] = val;
                _lengthTable[prefix | i] = length;
            }
        }

        public T Parse(IBitReader br)
        {
            uint peeked;
            br.Peek(_maxSize, out peeked);

            if (_lengthTable[peeked] == -1)
                throw new Exception("Huffman Code not located.");

            br.Skip(_lengthTable[peeked]);

            return _codeTable[peeked];
        }

        private static void NormalizeString(string str, out uint val, out int length)
        {
            length = 0;
            val = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] != '1' && str[i] != '0')
                    continue;

                val <<= 1;
                if (str[i] == '1')
                    val |= 1;

            }
        }
    }
}
