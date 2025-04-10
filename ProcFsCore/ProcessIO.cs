namespace ProcFsCore;

public readonly struct ProcessIO
{
    public readonly Direction Read;
    public readonly Direction Write;
        
    private ProcessIO(in Direction read, in Direction write)
    {
        Read = read;
        Write = write;
    }

    internal static ProcessIO Get(ProcFs instance, int pid)
    {
        using var statReader = new AsciiFileReader(instance.PathFor($"{pid}/io"), 256);
        statReader.SkipWord();
        var readCharacters = statReader.ReadInt64();
        statReader.SkipWord();
        var writeCharacters = statReader.ReadInt64();
        statReader.SkipWord();
        var readSysCalls = statReader.ReadInt64();
        statReader.SkipWord();
        var writeSysCalls = statReader.ReadInt64();
        statReader.SkipWord();
        var readBytes = statReader.ReadInt64();
        statReader.SkipWord();
        var writeBytes = statReader.ReadInt64();
                
        return new ProcessIO(new Direction(readCharacters, readBytes, readSysCalls),
                             new Direction(writeCharacters, writeBytes, writeSysCalls));
    }

    public readonly struct Direction(long characters, long bytes, long sysCalls)
    {
        public long Characters => characters;
        public long Bytes => bytes;
        public long SysCalls => sysCalls;
    }
}