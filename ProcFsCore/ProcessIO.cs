using System;

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

    private static ReadOnlySpan<byte> StatNameSeparators => ": "u8;
    internal static ProcessIO Get(ProcFs instance, int pid)
    {
        var statReader = new AsciiFileReader(instance.PathFor($"{pid}/io"), 256);
        try
        {
            statReader.SkipFragment(StatNameSeparators);
            var readCharacters = statReader.ReadInt64();
            statReader.SkipFragment(StatNameSeparators);
            var writeCharacters = statReader.ReadInt64();
            statReader.SkipFragment(StatNameSeparators);
            var readSysCalls = statReader.ReadInt64();
            statReader.SkipFragment(StatNameSeparators);
            var writeSysCalls = statReader.ReadInt64();
            statReader.SkipFragment(StatNameSeparators);
            var readBytes = statReader.ReadInt64();
            statReader.SkipFragment(StatNameSeparators);
            var writeBytes = statReader.ReadInt64();
                
            return new ProcessIO(new Direction(readCharacters, readBytes, readSysCalls),
                new Direction(writeCharacters, writeBytes, writeSysCalls));
        }
        finally
        {
            statReader.Dispose();
        }
    }

    public readonly struct Direction(long characters, long bytes, long sysCalls)
    {
        public long Characters => characters;
        public long Bytes => bytes;
        public long SysCalls => sysCalls;
    }
}