using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace nBinLib.CircularBuffers
{
    public class ShortCircularBufferedStream : Stream
    {
        private readonly int _capacity;
        private readonly byte[] _buffer;

        public bool AllowOverflow { get; set; }

        private ushort _write;
        private ushort _read;

        public ShortCircularBufferedStream()
        {
            _capacity = 65536;
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
                return ShortLength;
            }
        }

        private  int ShortLength
        {
            get
            {
                return (_write >= _read) ? _write - _read : ushort.MaxValue - _read + _write + 1;
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
            _read += (ushort)count;
            return count;
        }

        public int Peek(byte[] buffer, int offset, int count)
        {
            if (ShortLength - count < 0)
                count = ShortLength;

            if (count == 0)
                return 0;

            if (_read <= (ushort)(_read + count - 1))
            {
                // Simple copy
                //      v----->
                // [0,0,1,2,3,4,0,0]
                Array.Copy(this._buffer, _read, buffer, offset, count);
            }
            else
            {
                // Two copies
                //  -->         v--
                // [3,4,0,0,0,0,1,2]
                var start = _read;
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
            _read += (ushort)offset;

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

            if (ShortLength + count > _capacity)
            {
                if (!AllowOverflow)
                    throw new Exception("Overflow");

                _read = (ushort)(_write + count - _capacity);
            }

            if (_write <= (ushort)(_write + count - 1))
            {
                // Simple copy
                //      v----->
                // [0,0,1,2,3,4,0,0]
                Array.Copy(buffer, offset, this._buffer, _write, count);
            }
            else
            {
                // Two copies
                //  -->         v--
                // [3,4,0,0,0,0,1,2]
                var start = _write;
                var firstCopyLength = _capacity - start;
                var secondCopyLength = count - firstCopyLength;

                Array.Copy(buffer, offset, _buffer, start, firstCopyLength);
                Array.Copy(buffer, offset + firstCopyLength, _buffer, 0, secondCopyLength);
            }


            // Move the write position
            _write += (ushort)count;
        }

        public int Scan(byte i)
        {
            if (ShortLength == 0)
                return -1;

            if (_read <= (_read + Length - 1))
            {
                var found = Array.IndexOf(_buffer, i, _read, ShortLength);
                return (found == -1) ? found : found - _read;
            }
            else
            {
                var start = _read;
                var firstCopyLength = _capacity - start;
                var secondCopyLength = ShortLength - firstCopyLength;

                var found = Array.IndexOf(_buffer, i, _read, firstCopyLength);
                if (found != -1)
                    return found - _read;

                found = Array.IndexOf(_buffer, i, 0, secondCopyLength);
                if (found != -1)
                    return found + firstCopyLength;

                return -1;
            }

        }
    }
}
