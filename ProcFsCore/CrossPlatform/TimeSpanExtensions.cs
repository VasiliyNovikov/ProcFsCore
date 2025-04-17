#if NETSTANDARD2_0
namespace System;

internal static class TimeSpanExtensions
{
    private const long NanosecondsPerTickValue = 1_000_000 / TimeSpan.TicksPerMillisecond;

    extension(TimeSpan)
    {
        public static long NanosecondsPerTick => NanosecondsPerTickValue;
    }
}
#endif