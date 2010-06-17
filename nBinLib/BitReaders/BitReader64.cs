using System;
using System.IO;

namespace nBinLib.Reader
{
    public class BitReader64 : IBitReader
    {
        private readonly Stream _stream;
        private ulong _buffer;
        private int _bitsLoaded;
        private const byte MaximumBufferLenghtInBits = 64;
        private readonly byte[] _byteBuffer;

        public BitReader64(Stream stream)
        {
            _byteBuffer = new byte[MaximumBufferLenghtInBits >> 3];
            _stream = stream;
        }

        private void FillBuffer(int minimumBitsToRead)
        {
            var bytesToRead = (MaximumBufferLenghtInBits - _bitsLoaded) >> 3;
            var actualBytesRead = _stream.Read(_byteBuffer, 0, bytesToRead);

            if (minimumBitsToRead > _bitsLoaded + (actualBytesRead << 8))
                throw new Exception("Not enough bytes in source to satisfy read.");

            for (var i = 0; i < actualBytesRead; i++)
            {
                _buffer |= ((ulong)_byteBuffer[i]) << (56 - (8 * i) - _bitsLoaded);
            }
            _bitsLoaded += actualBytesRead * 8;
        }

        public int Skip(int length)
        {
            if (length > _bitsLoaded)
            {
                int remainingBits;

                if (length > MaximumBufferLenghtInBits)
                {
                    var bytesToSkip = (length - _bitsLoaded) >> 3;
                    remainingBits = (length - _bitsLoaded) & 7;

                    _stream.Seek(bytesToSkip, SeekOrigin.Current);

                    
                }
                else
                {
                    remainingBits = length - _bitsLoaded;
                }

                // Invalidate Buffer
                _buffer = 0;
                _bitsLoaded = 0;

                FillBuffer(remainingBits);
                _buffer &= (~0ul >> remainingBits);
                _buffer <<= remainingBits;
                _bitsLoaded -= remainingBits;

            }
            else if(length == MaximumBufferLenghtInBits)
            {
                _buffer = 0;
                _bitsLoaded = 0;
            }
            else 
            {
                _buffer &= (~0ul >> length);
                _buffer <<= length;
                _bitsLoaded -= length;
            }
            return length;
        }

        public ulong Read(int length)
        {
            ulong val;
            Peek(length, out val);
            Skip(length);
            return val;
        }

        public int Read(int length, out ulong val)
        {
            Peek(length, out val);
            Skip(length);
            return length;
        }

        public int Read(int length, out uint val)
        {
            Peek(length, out val);
            Skip(length);
            return length;
        }

        public bool ReadBool()
        {
            uint val;
            Read(1, out val);
            return val == 1;
        }

        public bool PeekBool()
        {
            uint val;
            Peek(1, out val);
            return val == 1;
        }

        public int Peek(int length, out uint val)
        {
            ulong retVal;
            Peek(length, out retVal);
            val = (uint)retVal;
            return length;
        }

        public int Peek(int length, out ulong val)
        {
            if (length > MaximumBufferLenghtInBits)
                throw new Exception("Requested length is greater than the maximum buffer size");

            if (length > _bitsLoaded)
                FillBuffer(length);

            var mask = (length == MaximumBufferLenghtInBits) ? ~0ul : ~(~0ul >> length);
            val = (_buffer & mask) >> (MaximumBufferLenghtInBits - length);
            return length;
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            ByteAlign();

            var bytesFromBuffer = 0;
            while (_bitsLoaded > 0)
            {
                buffer[offset + bytesFromBuffer] = (byte)(_buffer >> (MaximumBufferLenghtInBits - 8));
                _buffer <<= 8;
                _bitsLoaded -= 8;
                bytesFromBuffer++;
            }

            // Invalidate Buffer
            _buffer = 0;
            _bitsLoaded = 0;

            return _stream.Read(buffer, offset + bytesFromBuffer, length - bytesFromBuffer);
        }

        public int ByteAlign()
        {
            var length = _bitsLoaded & 7;
            Skip(_bitsLoaded & 7);
            return length;
        }
    }
}
