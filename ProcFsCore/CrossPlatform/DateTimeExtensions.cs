#if NETSTANDARD2_0
namespace System;

internal static class DateTimeExtensions
{
    private static readonly DateTime UnixEpochValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    extension(DateTime)
    {
        public static DateTime UnixEpoch => UnixEpochValue;
    }
}
#endif