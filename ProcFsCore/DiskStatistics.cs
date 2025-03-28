using System;
using System.Collections.Generic;

namespace ProcFsCore;

public readonly struct DiskStatistics
{
    private const long SectorSize = 512; 

    public string DeviceName { get; }

    public readonly Operation Reads;
    public readonly Operation Writes;

    public double TotalTime { get; }
    public double TotalWeightedTime { get; }

    private DiskStatistics(string deviceName, in Operation reads, in Operation writes, double totalTime, double totalWeightedTime)
    {
        DeviceName = deviceName;
        Reads = reads;
        Writes = writes;
        TotalTime = totalTime;
        TotalWeightedTime = totalWeightedTime;
    }

    private static ReadOnlySpan<byte> LoopDeviceStart => "loop"u8;

    internal static IEnumerable<DiskStatistics> GetAll(ProcFs instance)
    {
        // http://man7.org/linux/man-pages/man5/proc.5.html
        // https://www.kernel.org/doc/Documentation/iostats.txt
        var diskStatsPath = instance.PathFor("diskstats");
        var statsReader = new Utf8FileReader(diskStatsPath, 1024);
        try
        {
            while (!statsReader.EndOfStream)
            {
                var statLine = statsReader.ReadLine();
                var statReader = new Utf8SpanReader(statLine);
                try
                {
                    statReader.SkipWhiteSpaces();
                    statReader.SkipWord();
                    statReader.SkipWord();

                    var deviceName = statReader.ReadWord();

                    var reads = Operation.Parse(ref statReader);
                    var writes = Operation.Parse(ref statReader);

                    if (deviceName.StartsWith(LoopDeviceStart) &&
                        reads is { Count: 0, Merged: 0, Bytes: 0, Time: 0 } &&
                        writes is { Count: 0, Merged: 0, Bytes: 0, Time: 0 })
                        continue;

                    statReader.SkipWord();
                    var totalTime = statReader.ReadInt64() / 1_000_000.0;
                    var totalWeightedTime = statReader.ReadInt64() / 1_000_000.0;

                    yield return new DiskStatistics(deviceName.ToUtf8String(), reads, writes, totalTime, totalWeightedTime);
                }
                finally
                {
                    statReader.Dispose();
                }
            }
        }
        finally
        {
            statsReader.Dispose();
        }
    }

    public readonly struct Operation
    {
        public long Count { get; }
        public long Merged { get; }
        public long Bytes { get; }
        public double Time { get; }

        private Operation(long count, long merged, long bytes, double time)
        {
            Count = count;
            Merged = merged;
            Bytes = bytes;
            Time = time;
        }

        internal static Operation Parse<TReader>(ref TReader reader)
            where TReader : struct, IUtf8Reader
        {
            var count = reader.ReadInt64();
            var merged = reader.ReadInt64();
            var sectors = reader.ReadInt64();
            var time = reader.ReadInt64() / 1_000_000.0;
            return new Operation(count, merged, sectors * SectorSize, time);
        }
    }
}