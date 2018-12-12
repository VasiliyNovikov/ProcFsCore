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
            using (var buffer = Buffer.FromFile(NetDevPath, 2048))
            {
                var bufferReader = new Utf8SpanReader(buffer.Span);
                bufferReader.ReadLine();
                var columnLineReader = new Utf8SpanReader(bufferReader.ReadLine());
                columnLineReader.ReadFragment('|');
                var receiveColumnsReader = new Utf8SpanReader(columnLineReader.ReadFragment('|'));
                var receiveColumnCount = 0;
                while (!receiveColumnsReader.ReadWord().IsEmpty)
                    ++receiveColumnCount;
                ReceiveColumnCount = receiveColumnCount;
            }
        }
        
        public string InterfaceName { get; }
        public Direction Receive { get; }
        public Direction Transmit { get; }

        private NetStatistics(string interfaceName, in Direction receive, in Direction transmit)
        {
            InterfaceName = interfaceName;
            Receive = receive;
            Transmit = transmit;
        }

        private static readonly ReadOnlyMemory<byte> InterfaceNameSeparators = ": ".ToUtf8();

        internal static IEnumerable<NetStatistics> GetAll()
        {
            using (var buffer = Buffer.FromFile(NetDevPath, 2048))
            {
                var bufferReader2 = new Utf8SpanReader(buffer.Span);
                bufferReader2.ReadLine();
                bufferReader2.ReadLine();
                var position = bufferReader2.Position;
                
                while (position < buffer.Length)
                {
                    var bufferReader = new Utf8SpanReader(buffer.Span) {Position = position};
                    bufferReader.SkipWhiteSpaces();
                    var interfaceName = bufferReader.ReadFragment(InterfaceNameSeparators.Span).ToUtf8String();
                    
                    var receiveBytes = bufferReader.ReadInt64();
                    var receivePackets = bufferReader.ReadInt64();
                    var receiveErrors = bufferReader.ReadInt64();
                    var receiveDrops = bufferReader.ReadInt64();

                    for (var i = 0; i < ReceiveColumnCount - 4; ++i)
                        bufferReader.ReadWord();
                    
                    var transmitBytes = bufferReader.ReadInt64();
                    var transmitPackets = bufferReader.ReadInt64();
                    var transmitErrors = bufferReader.ReadInt64();
                    var transmitDrops = bufferReader.ReadInt64();

                    bufferReader.ReadLine();
                    position = bufferReader.Position;
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