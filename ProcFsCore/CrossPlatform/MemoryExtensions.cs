#if NETSTANDARD2_0
using System.Buffers;
using System.Runtime.CompilerServices;

namespace System;

internal static class MemoryExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAny<T>(this ReadOnlySpan<T> span, SearchValues<T> values) where T : IEquatable<T>?
    {
        return span.IndexOfAny(values.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAny<T>(this Span<T> span, SearchValues<T> values) where T : IEquatable<T>?
    {
        return span.IndexOfAny(values.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, SearchValues<T> values) where T : IEquatable<T>?
    {
        return IndexOfAnyExceptImpl(span, values.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAnyExcept<T>(this Span<T> span, SearchValues<T> values) where T : IEquatable<T>?
    {
        return IndexOfAnyExceptImpl(span, values.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int IndexOfAnyExceptImpl<T>(ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>?
    {
        for (var i = 0; i < span.Length; ++i)
            if (values.IndexOf(span[i]) < 0)
                return i;
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> Trim<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> trimChars) where T : IEquatable<T>?
    {
        return TrimImpl(source, trimChars);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> Trim<T>(this Span<T> source, ReadOnlySpan<T> trimChars) where T : IEquatable<T>?
    {
        return TrimImpl(source, trimChars);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> Trim<T>(this ReadOnlySpan<T> source, T trimChar) where T : IEquatable<T>?
    {
        return TrimImpl(source, trimChar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> Trim<T>(this Span<T> source, T trimChar) where T : IEquatable<T>?
    {
        return TrimImpl(source, trimChar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<T> TrimImpl<T>(ReadOnlySpan<T> source, params ReadOnlySpan<T> trimChars) where T : IEquatable<T>?
    {
        var startPos = 0;
        while (startPos < source.Length && trimChars.IndexOf(source[startPos]) >= 0)
            ++startPos;

        if (startPos == source.Length)
            return default;

        var endPos = source.Length - 1;
        while (endPos >= startPos && trimChars.IndexOf(source[endPos]) >= 0)
            --endPos;

        if (endPos < startPos)
            return default;

        return source.Slice(startPos, endPos - startPos + 1);
    }
}
#endif