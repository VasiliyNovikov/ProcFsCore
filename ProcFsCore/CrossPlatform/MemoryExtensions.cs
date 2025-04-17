#if NETSTANDARD2_0
using System.Buffers;
using System.Runtime.CompilerServices;

namespace System;

internal static class MemoryExtensions
{
    extension<T>(ReadOnlySpan<T> span) where T : IEquatable<T>?
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(SearchValues<T> values) => span.IndexOfAny(values.Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAnyExcept(SearchValues<T> values) => span.IndexOfAnyExceptImpl(values.Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> Trim(ReadOnlySpan<T> trimChars) => span.TrimImpl(trimChars);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> Trim(T trimChar) => span.TrimImpl(trimChar);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int IndexOfAnyExceptImpl(ReadOnlySpan<T> values)
        {
            for (var i = 0; i < span.Length; ++i)
                if (values.IndexOf(span[i]) < 0)
                    return i;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<T> TrimImpl(params ReadOnlySpan<T> trimChars)
        {
            var startPos = 0;
            while (startPos < span.Length && trimChars.IndexOf(span[startPos]) >= 0)
                ++startPos;

            if (startPos == span.Length)
                return default;

            var endPos = span.Length - 1;
            while (endPos >= startPos && trimChars.IndexOf(span[endPos]) >= 0)
                --endPos;

            if (endPos < startPos)
                return default;

            return span.Slice(startPos, endPos - startPos + 1);
        }
    }

    extension<T>(Span<T> span) where T : IEquatable<T>?
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(SearchValues<T> values) => span.IndexOfAny(values.Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAnyExcept(SearchValues<T> values) => span.IndexOfAnyExceptImpl(values.Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> Trim(ReadOnlySpan<T> trimChars) => span.TrimImpl(trimChars);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> Trim(T trimChar) => span.TrimImpl(trimChar);
    }
}
#endif