using System.IO;
using nBinLib.CircularBuffers;
using NUnit.Framework;

namespace nBinLib.Tests.CircularBuffers
{
    [TestFixture]
    public class RingBufferedStreamTests
    {
        [Test]
        public void ShouldPerformSimpleRead()
        {
            var rb = new RingBufferedStream(3);
            rb.Write(new byte[] {1, 2, 3, 4, 5, 6, 7, 8}, 0, 8);
            var reader = new byte[8];
            rb.Read(reader, 0, 2);
            Assert.AreEqual(new byte[] {1, 2, 0, 0, 0, 0, 0, 0}, reader);
            rb.Read(reader, 2, 2);
            Assert.AreEqual(new byte[] { 1, 2, 3, 4, 0, 0, 0, 0 }, reader);
            rb.Read(reader, 4, 4);
            Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, reader);
        }

        [Test]
        public void ShouldWrapAround()
        {
            var reader = new byte[8];
            var rb = new RingBufferedStream(3);
            
            rb.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            rb.Read(reader, 0, 4);
            Assert.AreEqual(new byte[] { 1, 2, 3, 4, 0, 0, 0, 0 }, reader);

            rb.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 0, 8);

            rb.Read(reader, 0, 8);
            Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, reader);
        }

        [Test]
        public void ShouldSeekForward()
        {
            var reader = new byte[8];
            var rb = new RingBufferedStream(3);

            rb.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 0, 8);
            rb.Seek(2, SeekOrigin.Current);
            rb.Read(reader, 0, 1);
            Assert.AreEqual(3, reader[0]);
        }

        [Test]
        public void SeekShouldWrap()
        {
            var reader = new byte[8];
            var rb = new RingBufferedStream(3);

            rb.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 0, 8);
            rb.Seek(4, SeekOrigin.Current);
            rb.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 0, 4);
            rb.Seek(6, SeekOrigin.Current);
            rb.Read(reader, 0, 1);
            Assert.AreEqual(3, reader[0]);
        }
    }
}
