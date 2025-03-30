using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

public static class AsciiParser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParse<T>(ReadOnlySpan<byte> source, out T value, char format = '\0')
    {
        if (typeof(T) == typeof(byte))
        {
            var result = Utf8Parser.TryParse(source, out byte typedValue, out _, format);
            value = (T) (object) typedValue;
            return result;
        }
        if (typeof(T) == typeof(short))
        {
            var result = Utf8Parser.TryParse(source, out short typedValue, out _, format);
            value = (T) (object) typedValue;
            return result;
        }
        if (typeof(T) == typeof(ushort))
        {
            var result = Utf8Parser.TryParse(source, out ushort typedValue, out _, format);
            value = (T) (object) typedValue;
            return result;
        }
        if (typeof(T) == typeof(int))
        {
            var result = Utf8Parser.TryParse(source, out int typedValue, out _, format);
            value = (T) (object) typedValue;
            return result;
        }
        if (typeof(T) == typeof(uint))
        {
            var result = Utf8Parser.TryParse(source, out uint typedValue, out _, format);
            value = (T) (object) typedValue;
            return result;
        }
        if (typeof(T) == typeof(long))
        {
            var result = Utf8Parser.TryParse(source, out long typedValue, out _, format);
            value = (T) (object) typedValue;
            return result;
        }
        if (typeof(T) == typeof(ulong))
        {
            var result = Utf8Parser.TryParse(source, out ulong typedValue, out _, format);
            value = (T) (object) typedValue;
            return result;
        }
        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Parse<T>(ReadOnlySpan<byte> source, char format = '\0')
    {
        return TryParse(source, out T result, format)
            ? result
            : throw new FormatException($"{source.ToAsciiString()} is not valid {typeof(T).Name} value");
    }
}