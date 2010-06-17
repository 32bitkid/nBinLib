namespace nBinLib.Reader
{
    public interface IBitReader
    {
        int Read(int length, out uint val);
        int Read(int length, out ulong val);
        int Read(byte[] buffer, int offset, int length);

        bool ReadBool();
        bool PeekBool();

        int Peek(int length, out uint val);
        int Peek(int length, out ulong val);
        int Skip(int length);

        int ByteAlign();
    }
}