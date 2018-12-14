using System;
using System.Collections.Generic;

namespace ProcFsCore
{
    public struct NetStatistics
    {
        private const string NetDevPath = ProcFs.RootPath + "/net/dev";

        private static readonly int ReceiveColumnCount;

        static NetStatistics()
        {
            using (var statReader = new Utf8FileReader(NetDevPath))
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
                ReceiveColumnCount = receiveColumnCount;
            }
        }
        
        public string InterfaceName { get; }
        public readonly Direction Receive;
        public readonly Direction Transmit;

        private NetStatistics(string interfaceName, in Direction receive, in Direction transmit)
        {
            InterfaceName = interfaceName;
            Receive = receive;
            Transmit = transmit;
        }

        private static readonly ReadOnlyMemory<byte> InterfaceNameSeparators = ": ".ToUtf8();

        internal static IEnumerable<NetStatistics> GetAll()
        {
            var statReader = new Utf8FileReader(NetDevPath);
            try
            {
                statReader.SkipLine();
                statReader.SkipLine();
                while (!statReader.EndOfStream)
                {
                    statReader.SkipWhiteSpaces();
                    var interfaceName = statReader.ReadFragment(InterfaceNameSeparators.Span).ToUtf8String();

                    var receive = Direction.Read(ref statReader);

                    for (var i = 0; i < ReceiveColumnCount - 4; ++i)
                        statReader.SkipWord();

                    var transmit = Direction.Read(ref statReader);

                    statReader.SkipLine();

                    yield return new NetStatistics(interfaceName, receive, transmit);
                }
            }
            finally
            {
                statReader.Dispose();
            }
        }

        public struct  Direction
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

            internal static Direction Read(ref Utf8FileReader reader)
            {
                var bytes = reader.ReadInt64();
                var packets = reader.ReadInt64();
                var errors = reader.ReadInt64();
                var drops = reader.ReadInt64();
                return new Direction(bytes, packets, errors, drops);
            }
        }
    }
}