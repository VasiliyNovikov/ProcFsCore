using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

internal struct AsciiFileReader(string fileName, int initialBufferSize = 0) : IDisposable
{
    private static readonly SearchValues<byte> WhiteSpaces = SearchValues.Create(" \t\n"u8);
    private static readonly SearchValues<byte> LineSeparators = SearchValues.Create("\n"u8);

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
                return;
            }

            var newBuffer = ArrayPool<byte>.Shared.Rent(_buffer.Length + 1);
            bufferSpan.CopyTo(newBuffer);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindStart(SearchValues<byte> separators, bool fragmentOrSeparators, int startIndex = 0)
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
                return startIndex + start;
            if (_hasReadToTheEnd)
                return -1;
            startIndex += span.Length;
            ReadToBuffer();
        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindFragmentStart(SearchValues<byte> separators, int startIndex = 0) => FindStart(separators, true, startIndex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindSeparatorsStart(SearchValues<byte> separators) => FindStart(separators, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipAll()
    {
        _bufferedStart = 0;
        _bufferedEnd = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Skip(int count)
    {
        _bufferedStart += count;
        if (_bufferedStart == _bufferedEnd)
            SkipAll();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Skip(SearchValues<byte> separators, bool fragmentOrSeparators)
    {
        var start = FindStart(separators, !fragmentOrSeparators);
        if (start < 0)
            SkipAll();
        else
            Skip(start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipSeparators(SearchValues<byte> separators) => Skip(separators, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWord(SearchValues<byte> separators)
    {
        Skip(separators, true);
        SkipSeparators(separators);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWord() => SkipWord(WhiteSpaces);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipLine() => SkipWord(LineSeparators);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWhiteSpaces() => SkipSeparators(WhiteSpaces);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadWord(SearchValues<byte> separators)
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
    public ReadOnlySpan<byte> ReadLine() => ReadWord(LineSeparators);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadWord() => ReadWord(WhiteSpaces);

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