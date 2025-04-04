using System;
using System.Collections.Generic;

namespace ProcFsCore;

public readonly struct NetStatistics
{
    private const string NetDevRelativePath = "net/dev";
    private static ReadOnlySpan<byte> InterfaceNameSeparators => ": "u8;

    public string InterfaceName { get; }
    public readonly Direction Receive;
    public readonly Direction Transmit;

    private NetStatistics(string interfaceName, in Direction receive, in Direction transmit)
    {
        InterfaceName = interfaceName;
        Receive = receive;
        Transmit = transmit;
    }

    internal static int GetReceiveColumnCount(ProcFs instance)
    {
        var statReader = new AsciiFileReader(instance.PathFor(NetDevRelativePath), 512);
        try
        {
            statReader.SkipLine();
            statReader.SkipFragment('|');
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
        finally
        {
            statReader.Dispose();   
        }
    }

    internal static IEnumerable<NetStatistics> GetAll(ProcFs instance, int receiveColumnCount) => GetAll(instance.PathFor(NetDevRelativePath), receiveColumnCount);

    internal static IEnumerable<NetStatistics> Get(ProcFs instance, int pid, int receiveColumnCount) => GetAll(instance.PathFor($"{pid}/{NetDevRelativePath}"), receiveColumnCount);

    private static IEnumerable<NetStatistics> GetAll(string path, int receiveColumnCount)
    {
        var statReader = new AsciiFileReader(path, 2048);
        try
        {
            statReader.SkipLine();
            statReader.SkipLine();
            while (!statReader.EndOfStream)
            {
                statReader.SkipWhiteSpaces();
                var interfaceName = statReader.ReadFragment(InterfaceNameSeparators).ToAsciiString();

                var receive = Direction.Parse(ref statReader);

                for (var i = 0; i < receiveColumnCount - 4; ++i)
                    statReader.SkipWord();

                var transmit = Direction.Parse(ref statReader);

                statReader.SkipLine();

                yield return new NetStatistics(interfaceName, receive, transmit);
            }
        }
        finally
        {
            statReader.Dispose();
        }
    }

    public readonly struct  Direction
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

        internal static Direction Parse<TReader>(ref TReader reader)
            where TReader : struct, IAsciiReader
        {
            var bytes = reader.ReadInt64();
            var packets = reader.ReadInt64();
            var errors = reader.ReadInt64();
            var drops = reader.ReadInt64();
            return new Direction(bytes, packets, errors, drops);
        }
    }
}