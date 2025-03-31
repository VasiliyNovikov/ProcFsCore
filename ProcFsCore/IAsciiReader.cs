using System;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

internal interface IAsciiReader : IDisposable
{
    bool EndOfStream { get; }

    void SkipSeparators(scoped ReadOnlySpan<byte> separators);
    void SkipFragment(scoped ReadOnlySpan<byte> separators, bool isSingleString = false);
    ReadOnlySpan<byte> ReadFragment(scoped ReadOnlySpan<byte> separators);
    ReadOnlySpan<byte> ReadToEnd();
}

internal static class AsciiReaderDefaults
{
    public static ReadOnlySpan<byte> WhiteSpaces => " \n\t\v\f\r"u8;
    public static ReadOnlySpan<byte> LineSeparators => "\n\r"u8;
}

internal static class AsciiReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipFragment<TReader>(this ref TReader reader, char separator)
        where TReader: struct, IAsciiReader
    {
        reader.SkipFragment([(byte) separator]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> ReadFragment<TReader>(this ref TReader reader, char separator)
        where TReader: struct, IAsciiReader
    {
        return reader.ReadFragment([(byte) separator]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> ReadLine<TReader>(this ref TReader reader)
        where TReader: struct, IAsciiReader
    {
        return reader.ReadFragment(AsciiReaderDefaults.LineSeparators);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipLine<TReader>(this ref TReader reader)
        where TReader: struct, IAsciiReader
    {
        reader.SkipFragment(AsciiReaderDefaults.LineSeparators);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> ReadWord<TReader>(this ref TReader reader)
        where TReader: struct, IAsciiReader
    {
        return reader.ReadFragment(AsciiReaderDefaults.WhiteSpaces);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipWord<TReader>(this ref TReader reader)
        where TReader: struct, IAsciiReader
    {
        reader.SkipFragment(AsciiReaderDefaults.WhiteSpaces);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipWhiteSpaces<TReader>(this ref TReader reader)
        where TReader: struct, IAsciiReader
    {
        reader.SkipSeparators(AsciiReaderDefaults.WhiteSpaces);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipSeparator<TReader>(this ref TReader reader, char separator)
        where TReader: struct, IAsciiReader
    {
        reader.SkipSeparators([(byte) separator]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadStringWord<TReader>(this ref TReader reader)
        where TReader: struct, IAsciiReader
    {
        return reader.ReadWord().ToAsciiString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Read<TReader, T>(this ref TReader reader, char format = '\0')
        where TReader: struct, IAsciiReader
    {
        return AsciiParser.Parse<T>(reader.ReadWord(), format);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16<TReader>(this ref TReader reader, char format = '\0')
        where TReader: struct, IAsciiReader
    {
        return reader.Read<TReader, short>(format);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32<TReader>(this ref TReader reader, char format = '\0')
        where TReader: struct, IAsciiReader
    {
        return reader.Read<TReader, int>(format);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64<TReader>(this ref TReader reader, char format = '\0')
        where TReader: struct, IAsciiReader
    {
        return reader.Read<TReader, long>(format);
    }
}