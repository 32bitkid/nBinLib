using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using nBinLib.Reader;
using NUnit.Framework;

namespace nBinLib.Tests.BitReaders
{
    [TestFixture]
    class BitReader64Test
    {
        private Stream _stream;
        private IBitReader _br;

        [TestCase(1, new byte[] { 1 }, Result = 0u)]
        [TestCase(2, new byte[] { 1 }, Result = 0u)]
        [TestCase(3, new byte[] { 1 }, Result = 0u)]
        [TestCase(4, new byte[] { 1 }, Result = 0u)]
        [TestCase(5, new byte[] { 1 }, Result = 0u)]
        [TestCase(6, new byte[] { 1 }, Result = 0u)]
        [TestCase(7, new byte[] { 1 }, Result = 0u)]
        [TestCase(8, new byte[] { 1 }, Result = 1u)]

        [TestCase(1, new byte[] { 128 }, Result = 1u)]
        [TestCase(2, new byte[] { 128 }, Result = 2u)]
        [TestCase(3, new byte[] { 128 }, Result = 4u)]
        [TestCase(4, new byte[] { 128 }, Result = 8u)]
        [TestCase(5, new byte[] { 128 }, Result = 16u)]
        [TestCase(6, new byte[] { 128 }, Result = 32u)]
        [TestCase(7, new byte[] { 128 }, Result = 64u)]
        [TestCase(8, new byte[] { 128 }, Result = 128u)]

        [TestCase(1, new byte[] { 255 }, Result = 1u)]
        [TestCase(2, new byte[] { 255 }, Result = 3u)]
        [TestCase(3, new byte[] { 255 }, Result = 7u)]
        [TestCase(4, new byte[] { 255 }, Result = 15u)]
        [TestCase(5, new byte[] { 255 }, Result = 31u)]
        [TestCase(6, new byte[] { 255 }, Result = 63u)]
        [TestCase(7, new byte[] { 255 }, Result = 127u)]
        [TestCase(8, new byte[] { 255 }, Result = 255u)]

        public uint ShouldReadAByte(int length, byte[] data)
        {
            _stream = new MemoryStream(data);
            _br = new BitReader64(_stream);

            uint actual;
            _br.Peek(length, out actual);
            return actual;
        }
    }
}