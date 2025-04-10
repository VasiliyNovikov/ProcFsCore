namespace System;

internal static class DateTimeExtensions
{
#if NETSTANDARD2_0
    public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#else
    public static DateTime UnixEpoch => DateTime.UnixEpoch;
#endif
}