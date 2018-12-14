using System;
using System.Text;

namespace ProcFsCore
{
    public static class Utf8Extensions
    {
        public static readonly Encoding Encoding = new UTF8Encoding(false);
        
        public static int IndexOf(this ReadOnlySpan<byte> source, char value) => source.IndexOf((byte) value);
        public static int IndexOf(this Span<byte> source, char value) => source.IndexOf((byte) value);
        
        public static int IndexOf(this ReadOnlySpan<byte> source, char value, int start) => start + source.Slice(start).IndexOf(value);
        public static int IndexOf(this Span<byte> source, char value, int start) => start + source.Slice(start).IndexOf(value);

        public static ReadOnlySpan<byte> Trim(this ReadOnlySpan<byte> source)
        {
            var startPos = 0;
            while (startPos < source.Length && Char.IsWhiteSpace((char)source[startPos]))
                ++startPos;
            
            if (startPos == source.Length)
                return default;
            
            var endPos = source.Length - 1;
            while (endPos >= startPos && Char.IsWhiteSpace((char)source[endPos]))
                --endPos;

            if (endPos < startPos)
                return default;

            return source.Slice(startPos, endPos - startPos + 1);
        }

        public static ReadOnlySpan<byte> Trim(this Span<byte> source) => ((ReadOnlySpan<byte>) source).Trim();

        public static void Replace(this Span<byte> source, char oldValue, char newValue)
        {
            if (oldValue == newValue)
                return;
            
            int index;
            while ((index = source.IndexOf(oldValue)) >= 0)
                source[index] = (byte)newValue;
        }

        public static ReadOnlyMemory<byte> ToUtf8(this string source) => Encoding.GetBytes(source);
        public static string ToUtf8String(this ReadOnlySpan<byte> source) => Encoding.GetString(source);
        public static string ToUtf8String(this Span<byte> source) => Encoding.GetString(source);
    }
}