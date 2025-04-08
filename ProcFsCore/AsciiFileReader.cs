using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

internal struct AsciiFileReader(string fileName, int initialBufferSize = 0)
{
    private static ReadOnlySpan<byte> WhiteSpaces => " \n\t\v\f\r"u8;
    private static ReadOnlySpan<byte> LineSeparators => "\n\r"u8;

    private readonly LightFileStream _stream = LightFileStream.OpenRead(fileName);

    private byte[] _buffer = ArrayPool<byte>.Shared.Rent(initialBufferSize);
    private int _bufferedStart = 0;
    private int _bufferedEnd = 0;
    private bool _hasReadToTheEnd = false;

    private Span<byte> BufferSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer;
    }

    private Span<byte> DataSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.AsSpan(_bufferedStart, _bufferedEnd - _bufferedStart);
    }

    public bool EndOfStream
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _hasReadToTheEnd && _bufferedStart == _bufferedEnd;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_buffer);
        _stream.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadToBuffer()
    {
        while (!_hasReadToTheEnd)
        {
            var bufferSpan = BufferSpan;
            if (_bufferedEnd < _buffer.Length)
            {
                var bytesRead = _stream.Read(bufferSpan[_bufferedEnd..]);
                _bufferedEnd += bytesRead;
                _hasReadToTheEnd = bytesRead == 0;
                return;
            }

            if (_bufferedStart > 0)
            {
                var dataSpan = bufferSpan[_bufferedStart..];
                dataSpan.CopyTo(bufferSpan);
                _bufferedStart = 0;
                _bufferedEnd = dataSpan.Length;
            }
            else
            {
                var newBuffer = ArrayPool<byte>.Shared.Rent(_buffer.Length + 1);
                bufferSpan.CopyTo(newBuffer);
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = newBuffer;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindStart(ReadOnlySpan<byte> separators, bool fragmentOrSeparators, int startIndex)
    {
        if (_bufferedStart == _bufferedEnd)
            ReadToBuffer();
        do
        {
            var span = DataSpan[startIndex..];
            var start = fragmentOrSeparators
                ? span.IndexOfAnyExcept(separators)
                : span.IndexOfAny(separators);
            if (start >= 0)
                return start + startIndex;
            if (_hasReadToTheEnd)
                return -1;
            startIndex += span.Length;
            ReadToBuffer();
        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindFragmentStart(ReadOnlySpan<byte> separators, int startIndex = 0) => FindStart(separators, true, startIndex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindSeparatorsStart(ReadOnlySpan<byte> separators, int startIndex = 0) => FindStart(separators, false, startIndex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Skip(int count)
    {
        _bufferedStart += count;
        if (_bufferedStart == _bufferedEnd)
            SkipAll();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipAll()
    {
        _bufferedStart = 0;
        _bufferedEnd = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipSeparators(ReadOnlySpan<byte> separators)
    {
        var fragmentStart = FindFragmentStart(separators);
        if (fragmentStart < 0)
            SkipAll();
        else
            Skip(fragmentStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipSeparator(char separator) => SkipSeparators([(byte) separator]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipFragment(scoped ReadOnlySpan<byte> separators)
    {
        var separatorsStart = FindSeparatorsStart(separators);
        if (separatorsStart < 0)
            SkipAll();
        else
        {
            Skip(separatorsStart);
            SkipSeparators(separators);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipFragment(char separator) => SkipFragment([(byte) separator]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipLine() => SkipFragment(LineSeparators);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWord() => SkipFragment(WhiteSpaces);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWhiteSpaces() => SkipSeparators(WhiteSpaces);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadFragment(scoped ReadOnlySpan<byte> separators)
    {
        if (EndOfStream)
            return default;

        var separatorsStart = FindSeparatorsStart(separators);
        if (separatorsStart < 0)
        {
            var result = DataSpan;
            SkipAll();
            return result;
        }
        else
        {
            var fragmentStart = FindFragmentStart(separators, separatorsStart);
            var result = DataSpan[..separatorsStart];
            if (fragmentStart < 0)
                SkipAll();
            else
                Skip(fragmentStart);
            return result;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadFragment(char separator) => ReadFragment([(byte) separator]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadLine() => ReadFragment(LineSeparators);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadWord() => ReadFragment(WhiteSpaces);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadStringWord() => ReadWord().ToAsciiString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T Read<T>(char format = '\0') => AsciiParser.Parse<T>(ReadWord(), format);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16(char format = '\0') => Read<short>(format);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32(char format = '\0') => Read<int>(format);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64(char format = '\0') => Read<long>(format);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadToEnd()
    {
        while (!_hasReadToTheEnd)
            ReadToBuffer();
        var result = DataSpan;
        SkipAll();
        return result;
    }

    public override string ToString() => DataSpan.ToAsciiString();
}