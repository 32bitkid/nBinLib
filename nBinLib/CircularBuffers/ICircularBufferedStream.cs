using System.IO;

namespace nBinLib.CircularBuffers {
    public interface ICircularBufferedStream {
        bool AllowOverflow { get; set; }
        bool CanRead { get; }
        bool CanSeek { get; }
        bool CanWrite { get; }
        long Length { get; }
        long Position { get; set; }
        void Flush();
        int Read(byte[] buffer, int offset, int count);
        int Peek(byte[] buffer, int offset, int count);
        long Seek(long offset, SeekOrigin origin);
        void SetLength(long value);
        void Write(byte[] buffer, int offset, int count);
        int Scan(byte i);
        void Close();
        void Dispose();
        byte this[int i] { get; }
    }
}