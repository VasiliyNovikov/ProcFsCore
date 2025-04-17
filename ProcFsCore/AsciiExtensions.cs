using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProcFsCore;

[SuppressMessage("Performance", "CA1822: Mark members as static", Justification = "False positive")]
internal static class AsciiExtensions
{
    public static readonly ASCIIEncoding Encoding = new();

    extension(ReadOnlySpan<byte> source)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value) => source.IndexOf((byte) value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value, int start) => start + source[start..].IndexOf(value);

        public string ToAsciiString() => Encoding.GetString(source);
    }

    extension(Span<byte> source)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value) => ((ReadOnlySpan<byte>) source).IndexOf(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value, int start) => ((ReadOnlySpan<byte>) source).IndexOf(value, start);

        public string ToAsciiString() => Encoding.GetString(source);
        
    }
}