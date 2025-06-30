using System.IO;

namespace ProcFsCore;

public readonly struct TaskIO
{
    public readonly Direction Read;
    public readonly Direction Write;
        
    private TaskIO(in Direction read, in Direction write)
    {
        Read = read;
        Write = write;
    }

    internal static TaskIO Get(string basePath)
    {
        using var statReader = new AsciiFileReader(Path.Combine(basePath, "io"), 256);
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
                
        return new TaskIO(new Direction(readCharacters, readBytes, readSysCalls),
                          new Direction(writeCharacters, writeBytes, writeSysCalls));
    }

    public readonly struct Direction(long characters, long bytes, long sysCalls)
    {
        public long Characters => characters;
        public long Bytes => bytes;
        public long SysCalls => sysCalls;
    }
}