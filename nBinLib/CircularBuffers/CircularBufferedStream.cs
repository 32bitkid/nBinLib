using System;
using System.IO;

namespace nBinLib.CircularBuffers
{
    public class CircularBufferedStream : Stream
    {
        private readonly uint _capacity;
        private readonly uint _relativeMask;
        private readonly byte[] _buffer;

        public bool AllowOverflow { get; set; }

        private long _write;
        private long _read;

        public CircularBufferedStream()
            : this(20)
        {
        }

        public CircularBufferedStream(int sizeInBits)
        {
            _capacity = 1u << sizeInBits;
            _relativeMask = ~0u >> (32 - sizeInBits);
            _buffer = new byte[_capacity];
            _write = _read = 0;
            AllowOverflow = false;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get
            {
                return (_write >= _read) ? _write - _read : long.MaxValue - _read + _write;
            }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = Peek(buffer, offset, count);
            _read += (uint)count;
            return count;
        }

        public int Peek(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return 0;

            if (Length - count < 0)
                count = (int)Length;

            if ((_read & _relativeMask) <= ((_read + count - 1) & _relativeMask))
            {
                // Simple copy
                //      v----->
                // [0,0,1,2,3,4,0,0]
                Array.Copy(this._buffer, _read & _relativeMask, buffer, offset, count);
            }
            else
            {
                // Two copies
                //  -->         v--
                // [3,4,0,0,0,0,1,2]
                var start = _read & _relativeMask;
                var firstCopyLength = _capacity - start;
                var secondCopyLength = count - firstCopyLength;

                Array.Copy(this._buffer, start, buffer, offset, firstCopyLength);
                Array.Copy(this._buffer, 0, buffer, offset + firstCopyLength, secondCopyLength);
            }
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin != SeekOrigin.Current || offset < 0)
                throw new NotSupportedException();

            if (_read + (uint)offset > _write)
                throw new UnderflowException();

            // Move 
            _read += (uint)offset;

            return offset;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            if (Length + count > _capacity)
            {
                if (!AllowOverflow)
                    throw new Exception("Overflow");

                _read = _write + (uint)count - _capacity;
            }

            if ((_write & _relativeMask) <= ((_write + count - 1) & _relativeMask))
            {
                // Simple copy
                //      v----->
                // [0,0,1,2,3,4,0,0]
                Array.Copy(buffer, offset, this._buffer, _write & _relativeMask, count);
            }
            else
            {
                // Two copies
                //  -->         v--
                // [3,4,0,0,0,0,1,2]
                var start = _write & _relativeMask;
                var firstCopyLength = _capacity - start;
                var secondCopyLength = count - firstCopyLength;

                Array.Copy(buffer, offset, this._buffer, start, firstCopyLength);
                Array.Copy(buffer, offset + firstCopyLength, this._buffer, 0, secondCopyLength);
            }


            // Move the write position
            _write += (uint)count;
        }

        public int Scan(byte i)
        {
            if (Length == 0)
                return -1;

            if ((_read & _relativeMask) <= ((_read + Length - 1) & _relativeMask))
            {
                var found = Array.IndexOf(_buffer, i, (int)(_read & _relativeMask), (int)Length);
                return (found == -1) ? found : found - (int)(_read & _relativeMask);
            }
            else
            {
                var start = _read & _relativeMask;
                var firstCopyLength = _capacity - start;
                var secondCopyLength = Length - firstCopyLength;

                var found = Array.IndexOf(_buffer, i, (int)(_read & _relativeMask), (int)firstCopyLength);
                if (found != -1)
                    return found - (int)(_read & _relativeMask);

                found = Array.IndexOf(_buffer, i, 0, (int)secondCopyLength);
                if (found != -1)
                    return found + (int)firstCopyLength;

                return -1;
            }

        }
    }

    public class UnderflowException : Exception { }
}