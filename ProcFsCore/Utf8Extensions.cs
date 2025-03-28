using System;
using System.Text;

namespace ProcFsCore;

public static class Utf8Extensions
{
    public static readonly UTF8Encoding Encoding = new(false);
        
    public static int IndexOf(this ReadOnlySpan<byte> source, char value) => source.IndexOf((byte) value);
    public static int IndexOf(this Span<byte> source, char value) => IndexOf((ReadOnlySpan<byte>) source, value);
        
    public static int IndexOf(this ReadOnlySpan<byte> source, char value, int start) => start + source[start..].IndexOf(value);
    public static int IndexOf(this Span<byte> source, char value, int start) => IndexOf((ReadOnlySpan<byte>) source, value, start);

    private static readonly Func<char, bool> WhiteSpacePredicate = Char.IsWhiteSpace;

    public static ReadOnlySpan<byte> Trim(this ReadOnlySpan<byte> source, Func<char, bool>? predicate = null)
    {
        predicate ??= WhiteSpacePredicate;
            
        var startPos = 0;
        while (startPos < source.Length && predicate((char)source[startPos]))
            ++startPos;
            
        if (startPos == source.Length)
            return default;
            
        var endPos = source.Length - 1;
        while (endPos >= startPos && predicate((char)source[endPos]))
            --endPos;

        if (endPos < startPos)
            return default;

        return source.Slice(startPos, endPos - startPos + 1);
    }

    public static ReadOnlySpan<byte> Trim(this Span<byte> source, Func<char, bool>? predicate = null) => ((ReadOnlySpan<byte>) source).Trim(predicate);

    public static string ToUtf8String(this ReadOnlySpan<byte> source) => Encoding.GetString(source);
    public static string ToUtf8String(this Span<byte> source) => Encoding.GetString(source);
}