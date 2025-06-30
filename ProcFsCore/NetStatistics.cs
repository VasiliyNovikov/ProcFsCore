using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

public readonly struct NetStatistics
{
    private const string DevRelativePath = "dev";
    private static readonly SearchValues<byte> IfaceColumnHeaderSeparators = SearchValues.Create("|"u8);

    public string InterfaceName { get; }
    public readonly Direction Receive;
    public readonly Direction Transmit;

    private NetStatistics(string interfaceName, in Direction receive, in Direction transmit)
    {
        InterfaceName = interfaceName;
        Receive = receive;
        Transmit = transmit;
    }

    internal static int GetReceiveColumnCount(string netPath)
    {
        using var statReader = new AsciiFileReader(Path.Combine(netPath, DevRelativePath), 512);
        statReader.SkipLine();
        statReader.SkipWord(IfaceColumnHeaderSeparators);
        statReader.SkipWhiteSpaces();
        var receiveColumnCount = 0;
        while (true)
        {
            var column = statReader.ReadWord();
            if (column.IndexOf('|') >= 0)
            {
                if (column.Length != 0)
                    ++receiveColumnCount;
                break;
            }
            ++receiveColumnCount;
        }
        return receiveColumnCount;
    }

    internal static IEnumerable<NetStatistics> Get(string netPath, int receiveColumnCount)
    {
        using var statReader = new AsciiFileReader(Path.Combine(netPath, DevRelativePath), 2048);
        statReader.SkipLine();
        statReader.SkipLine();
        while (!statReader.EndOfStream)
        {
            statReader.SkipWhiteSpaces();
            var interfaceName = statReader.ReadWord()[..^1].ToAsciiString();

            var receive = Direction.Read(statReader);

            for (var i = 0; i < receiveColumnCount - 4; ++i)
                statReader.SkipWord();

            var transmit = Direction.Read(statReader);

            statReader.SkipLine();

            yield return new NetStatistics(interfaceName, receive, transmit);
        }
    }

    public readonly struct Direction
    {
        public long Bytes { get; }
        public long Packets { get; }
        public long Errors { get; }
        public long Drops { get; }

        private Direction(long bytes, long packets, long errors, long drops)
        {
            Bytes = bytes;
            Packets = packets;
            Errors = errors;
            Drops = drops;
        }

        internal static Direction Read(in AsciiFileReader reader)
        {
            ref var readerRef = ref Unsafe.AsRef(in reader);
            var bytes = readerRef.ReadInt64();
            var packets = readerRef.ReadInt64();
            var errors = readerRef.ReadInt64();
            var drops = readerRef.ReadInt64();
            return new Direction(bytes, packets, errors, drops);
        }
    }
}