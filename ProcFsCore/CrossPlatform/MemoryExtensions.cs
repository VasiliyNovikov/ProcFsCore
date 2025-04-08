#if NETSTANDARD2_0
namespace System;

public static class MemoryExtensions
{
    public static int IndexOfAnyExcept<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>?
    {
        return IndexOfAnyExcept((ReadOnlySpan<T>)span, values);
    }

    public static int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>?
    {
        for (var i = 0; i < span.Length; ++i)
            if (values.IndexOf(span[i]) < 0)
                return i;
        return -1;
    }
}
#endif