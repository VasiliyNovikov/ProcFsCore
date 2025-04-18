using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

    internal static IEnumerable<DiskStatistics> GetAll(ProcFs instance)
    {
        // http://man7.org/linux/man-pages/man5/proc.5.html
        // https://www.kernel.org/doc/Documentation/iostats.txt
        var diskStatsPath = instance.PathFor("diskstats");
        using var statsReader = new AsciiFileReader(diskStatsPath, 1024);
        while (!statsReader.EndOfStream)
        {
            statsReader.SkipWhiteSpaces();
            statsReader.SkipWord();
            statsReader.SkipWord();

            var deviceName = statsReader.ReadWord();

            var reads = Operation.Read(statsReader);
            var writes = Operation.Read(statsReader);

            statsReader.SkipWord();
            var totalTime = statsReader.ReadInt64() / 1_000_000.0;
            var totalWeightedTime = statsReader.ReadInt64() / 1_000_000.0;

            yield return new DiskStatistics(deviceName.ToAsciiString(), reads, writes, totalTime, totalWeightedTime);

            statsReader.SkipLine();
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

        internal static Operation Read(in AsciiFileReader reader)
        {
            ref var readerRef = ref Unsafe.AsRef(in reader);
            var count = readerRef.ReadInt64();
            var merged = readerRef.ReadInt64();
            var sectors = readerRef.ReadInt64();
            var time = readerRef.ReadInt64() / 1_000_000.0;
            return new Operation(count, merged, sectors * SectorSize, time);
        }
    }
}