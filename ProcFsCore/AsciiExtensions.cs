using System;
using System.Text;

namespace ProcFsCore;

public static class AsciiExtensions
{
    public static readonly ASCIIEncoding Encoding = new();

    public static int IndexOf(this ReadOnlySpan<byte> source, char value) => source.IndexOf((byte) value);
    public static int IndexOf(this Span<byte> source, char value) => IndexOf((ReadOnlySpan<byte>) source, value);
        
    public static int IndexOf(this ReadOnlySpan<byte> source, char value, int start) => start + source[start..].IndexOf(value);
    public static int IndexOf(this Span<byte> source, char value, int start) => IndexOf((ReadOnlySpan<byte>) source, value, start);

    public static ReadOnlySpan<byte> Trim(this ReadOnlySpan<byte> source, params ReadOnlySpan<byte> trimChars)
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

    public static ReadOnlySpan<byte> Trim(this Span<byte> source, params ReadOnlySpan<byte> trimChars) => ((ReadOnlySpan<byte>) source).Trim(trimChars);

    public static string ToAsciiString(this ReadOnlySpan<byte> source) => Encoding.GetString(source);
    public static string ToAsciiString(this Span<byte> source) => Encoding.GetString(source);
}