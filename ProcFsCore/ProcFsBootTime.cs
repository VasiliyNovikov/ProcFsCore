using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

public class ProcFsBootTime
{
    private const long NanosecondsPerTick =  1_000_000 / TimeSpan.TicksPerMillisecond;
    private static ReadOnlySpan<byte> BtimeStr => "btime"u8;

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
        using var statReader = new AsciiFileReader(statPath, 4096);
        while (!statReader.EndOfStream)
        {
            if (statReader.ReadWord().SequenceEqual(BtimeStr))
            {
                var bootTimeSeconds = statReader.ReadInt64();
                return DateTimeExtensions.UnixEpoch + TimeSpan.FromSeconds(bootTimeSeconds);
            }
            statReader.SkipLine();
        }
        throw new NotSupportedException();
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
        return DateTimeExtensions.UnixEpoch + TimeSpan.FromTicks(bootTimeNanosecondsSinceEpoch / NanosecondsPerTick);

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