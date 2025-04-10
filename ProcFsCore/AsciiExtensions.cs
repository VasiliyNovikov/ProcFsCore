using System;
using System.Text;

namespace ProcFsCore;

internal static class AsciiExtensions
{
    public static readonly ASCIIEncoding Encoding = new();

    public static int IndexOf(this ReadOnlySpan<byte> source, char value) => source.IndexOf((byte) value);
    public static int IndexOf(this Span<byte> source, char value) => IndexOf((ReadOnlySpan<byte>) source, value);
        
    public static int IndexOf(this ReadOnlySpan<byte> source, char value, int start) => start + source[start..].IndexOf(value);
    public static int IndexOf(this Span<byte> source, char value, int start) => IndexOf((ReadOnlySpan<byte>) source, value, start);

    public static string ToAsciiString(this ReadOnlySpan<byte> source) => Encoding.GetString(source);
    public static string ToAsciiString(this Span<byte> source) => Encoding.GetString(source);
}