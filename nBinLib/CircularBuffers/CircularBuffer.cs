using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nBinLib.CircularBuffers
{
    class CircularBuffer<T> : ICollection<T>
    {
        private readonly uint _capacity;
        private readonly uint _relativeMask;
        private readonly T[] _buffer;

        public bool AllowOverflow { get; set; }
        public bool SafeUnderflow { get; set; }

        private long _write;
        private long _read;

        public CircularBuffer() : this(20)
        {
        }

        public CircularBuffer(int sizeInBits)
        {
            _capacity = 1u << sizeInBits;
            _relativeMask = ~0u >> (32 - sizeInBits);
            _buffer = new T[_capacity];
            _write = _read = 0;
        }

        public long LongCount
        {
            get
            {
                return (_write >= _read) ? _write - _read : _write + uint.MaxValue - _read;
            }
        }

        public long Trash(long offset)
        {
            if (_read + (uint)offset > _write)
                if (SafeUnderflow)
                    offset = LongCount;
                else
                    throw new UnderflowException();

            // Move 
            _read += (uint)offset;

            return offset;
        }

        public long Read(T[] buffer, int offset, long count)
        {
            count = Peek(buffer, offset, count);
            _read += (uint)count;
            return count;
        }

        public long Peek(T[] buffer, int offset, long count)
        {
            if (count == 0)
                return 0;

            if (LongCount - count < 0)
                if (SafeUnderflow)
                    count = (int)LongCount;
                else
                    new UnderflowException();

            if ((_read & _relativeMask) <= ((_read + count - 1) & _relativeMask))
            {
                // Simple copy
                //      v----->
                // [0,0,1,2,3,4,0,0]
                Array.Copy(_buffer, _read & _relativeMask, buffer, offset, count);
            }
            else
            {
                // Two copies
                //  -->         v--
                // [3,4,0,0,0,0,1,2]
                var start = _read & _relativeMask;
                var firstCopyLength = _capacity - start;
                var secondCopyLength = count - firstCopyLength;

                Array.Copy(_buffer, start, buffer, offset, firstCopyLength);
                Array.Copy(_buffer, 0, buffer, offset + firstCopyLength, secondCopyLength);
            }
            return count;
        }

        public void Add(params T[] items)
        {
            Add(items, 0, items.Length);
        }

        public void Add(T[] buffer, int offset, int count)
        {
            if (LongCount + count > _capacity)
                if(AllowOverflow)
                    _read = _write + (uint)count - _capacity;
                else
                    throw new OverflowException();

            if((_write & _relativeMask) <= ((_write + count - 1) & _relativeMask))
            {
                // Simple copy
                //      v----->
                // [0,0,1,2,3,4,0,0]
                Array.Copy(buffer, offset, _buffer, _write & _relativeMask, count);
            }
            else
            {
                // Two copies
                //  -->         v--
                // [3,4,0,0,0,0,1,2]
                var start = _write & _relativeMask;
                var firstCopyLength = _capacity - start;
                var secondCopyLength = count - firstCopyLength;

                Array.Copy(buffer, offset, _buffer, start, firstCopyLength);
                Array.Copy(buffer, offset + firstCopyLength, _buffer, 0, secondCopyLength);
            }


            // Move the write position
            _write += (uint)count;
        }

        public int IndexOf(T item)
        {
            if (LongCount == 0)
                return -1;

            if ((_read & _relativeMask) <= ((_read + LongCount - 1) & _relativeMask))
            {
                var found = Array.IndexOf(_buffer, item, (int)(_read & _relativeMask), (int)LongCount);
                return (found == -1) ? found : found - (int)(_read & _relativeMask);
            }
            else
            {
                var start = _read & _relativeMask;
                var firstCopyLength = _capacity - start;
                var secondCopyLength = LongCount - firstCopyLength;

                var found = Array.IndexOf(_buffer, item, (int)(_read & _relativeMask), (int)firstCopyLength);
                if (found != -1)
                    return found - (int)(_read & _relativeMask);

                found = Array.IndexOf(_buffer, item, 0, (int)secondCopyLength);
                if (found != -1)
                    return found + (int)firstCopyLength;

                return -1;
            }
        }



        #region ICollection<T> Members

        public void Add(T item)
        {
            if (LongCount + 1 > _capacity)
                if (AllowOverflow)
                    _read = _write + 1 - _capacity;
                else
                    throw new OverflowException();

            _buffer[_write & _relativeMask] = item;
            _write++;
        }

        public void Clear()
        {
            _read = _write;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Read(array, arrayIndex, LongCount);
        }

        public int Count
        {
            get { return (int) LongCount; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for(var i = 0; i < LongCount; i++)
            {
                yield return _buffer[(_read + i) & _relativeMask];
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < LongCount; i++)
            {
                yield return _buffer[(_read + i) & _relativeMask];
            }
        }

        #endregion
    }
}
