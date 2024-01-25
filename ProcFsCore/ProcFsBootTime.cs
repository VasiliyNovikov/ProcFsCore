using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

public class ProcFsBootTime
{
    private static readonly ReadOnlyMemory<byte> BtimeStr = "btime ".ToUtf8();

    private readonly ProcFs _instance;
    private readonly string _statPath;
    private readonly TimeSpan _bootTimeCacheInterval = TimeSpan.FromSeconds(0.5);
    private readonly Stopwatch _bootTimeCacheTimer = new();
    private DateTime? _bootTimeUtc;

    internal ProcFsBootTime(ProcFs instance)
    {
        _instance = instance;
        _statPath = instance.PathFor("stat");
    }

    public DateTime UtcValue
    {
        get
        {
            lock (_bootTimeCacheTimer)
            {
                if (_bootTimeUtc == null || _bootTimeCacheTimer.Elapsed > _bootTimeCacheInterval)
                {
                    _bootTimeUtc = _instance.IsDefault ? GetPreciseSystemValueUtc() : GetStatValueUtc(_statPath);
                    _bootTimeCacheTimer.Restart();
                }
                return _bootTimeUtc.Value;
            }
        }
    }

    private static DateTime GetStatValueUtc(string statPath)
    {
        var statReader = new Utf8FileReader(statPath, 4096);
        try
        {
            statReader.SkipFragment(BtimeStr.Span, true);
            if (statReader.EndOfStream)
                throw new NotSupportedException();

            var bootTimeSeconds = statReader.ReadInt64();
            return DateTime.UnixEpoch + TimeSpan.FromSeconds(bootTimeSeconds);
        }
        finally
        {
            statReader.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime GetPreciseSystemValueUtc()
    {
        const int iterations = 3;
        var bootTimeNanosecondsSinceEpochSum = 0L;
        ComputeOnce(); // warmup
        for (var i = 0; i < iterations; ++i)
            bootTimeNanosecondsSinceEpochSum += ComputeOnce();

        var bootTimeNanosecondsSinceEpoch = bootTimeNanosecondsSinceEpochSum / iterations;
        return DateTime.UnixEpoch + TimeSpan.FromTicks(bootTimeNanosecondsSinceEpoch / 100);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static long ComputeOnce()
        {
            var nanosecondsSinceEpoch1 = Native.ClockGetTimeNanoseconds(Native.ClockId.RealTime);
            var nanosecondsSinceBoot = Native.ClockGetTimeNanoseconds(Native.ClockId.BootTime);
            var nanosecondsSinceEpoch2 = Native.ClockGetTimeNanoseconds(Native.ClockId.RealTime);
            return (nanosecondsSinceEpoch1 + nanosecondsSinceEpoch2) / 2 - nanosecondsSinceBoot;
        }
    }
}