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

    /// <summary>
    /// Reads more data to buffer if any
    /// </summary>
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

    /// <summary>
    /// Finds starting (from current + startIndex) position of word or separators.
    /// Reads data into buffer if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindStart(SearchValues<byte> separators, bool wordOrSeparators, int startIndex = 0)
    {
        if (_bufferedStart == _bufferedEnd)
            ReadToBuffer();
        do
        {
            var span = DataSpan[startIndex..];
            var start = wordOrSeparators
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

    /// <summary>
    /// Finds starting (from current + startIndex) position of word.
    /// Reads data into buffer if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindWordStart(SearchValues<byte> separators, int startIndex = 0) => FindStart(separators, true, startIndex);

    /// <summary>
    /// Finds starting (from current) position of separators.
    /// Reads data into buffer if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindSeparatorsStart(SearchValues<byte> separators) => FindStart(separators, false);

    /// <summary>
    /// Skips everything currently in the buffer
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipAll()
    {
        _bufferedStart = 0;
        _bufferedEnd = 0;
    }

    /// <summary>
    /// Skips count number of bytes in teh buffer
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Skip(int count)
    {
        _bufferedStart += count;
        if (_bufferedStart == _bufferedEnd)
            SkipAll();
    }

    /// <summary>
    /// Skip word or separators 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Skip(SearchValues<byte> separators, bool wordOrSeparators)
    {
        var start = FindStart(separators, !wordOrSeparators);
        if (start < 0)
            SkipAll();
        else
            Skip(start);
    }

    /// <summary>
    /// Skips separators.
    /// Position is moved to the beginning of the word or to the end
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipSeparators(SearchValues<byte> separators) => Skip(separators, false);

    /// <summary>
    /// Skips word and separators after the word.
    /// Position is moved to the beginning of the next word or to the end
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWord(SearchValues<byte> separators)
    {
        Skip(separators, true);
        SkipSeparators(separators);
    }

    /// <summary>
    /// Skips word and whitespaces after the word.
    /// Position is moved to the beginning of the next word or to the end
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWord() => SkipWord(WhiteSpaces);

    /// <summary>
    /// Skips line and line break characters after the line.
    /// Position is moved to the beginning of the next line or to the end
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipLine() => SkipWord(LineSeparators);

    /// <summary>
    /// Skips whitespaces.
    /// Position is moved to the beginning of the word or to the end
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWhiteSpaces() => SkipSeparators(WhiteSpaces);

    /// <summary>
    /// Reads word and skips separators after the word.
    /// Position is moved to the beginning of the next word or to the end
    /// </summary>
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
            var nextWordStart = FindWordStart(separators, separatorsStart);
            var result = DataSpan[..separatorsStart]; // DataSpan calculation is altered by Skip/SkipAll so need to get it before
            if (nextWordStart < 0)
                SkipAll();
            else
                Skip(nextWordStart);
            return result;
        }
    }

    /// <summary>
    /// Reads word and skips whitespaces after the word.
    /// Position is moved to the beginning of the next word or to the end
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadWord() => ReadWord(WhiteSpaces);

    /// <summary>
    /// Reads line and skips line break characters after the line.
    /// Position is moved to the beginning of the next word or to the end
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadLine() => ReadWord(LineSeparators);

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