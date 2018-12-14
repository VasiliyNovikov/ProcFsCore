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
            using (var statReader = new Utf8FileReader(NetDevPath))
            {
                statReader.SkipLine();
                statReader.SkipLine();
                while (!statReader.EndOfStream)
                {
                    statReader.SkipWhiteSpaces();
                    var interfaceName = statReader.ReadFragment(InterfaceNameSeparators.Span).ToUtf8String();
                    
                    var receiveBytes = statReader.ReadInt64();
                    var receivePackets = statReader.ReadInt64();
                    var receiveErrors = statReader.ReadInt64();
                    var receiveDrops = statReader.ReadInt64();

                    for (var i = 0; i < ReceiveColumnCount - 4; ++i)
                        statReader.SkipWord();
                    
                    var transmitBytes = statReader.ReadInt64();
                    var transmitPackets = statReader.ReadInt64();
                    var transmitErrors = statReader.ReadInt64();
                    var transmitDrops = statReader.ReadInt64();

                    statReader.SkipLine();
                    yield return new NetStatistics(interfaceName,
                                                   new Direction(receiveBytes, receivePackets, receiveErrors, receiveDrops),
                                                   new Direction(transmitBytes, transmitPackets, transmitErrors, transmitDrops));
                }
            }
        }

        public struct  Direction
        {
            internal Direction(long bytes, long packets, long errors, long drops)
            {
                Bytes = bytes;
                Packets = packets;
                Errors = errors;
                Drops = drops;
            }

            public long Bytes { get; }
            public long Packets { get; }
            public long Errors { get; }
            public long Drops { get; }
        }
    }
}