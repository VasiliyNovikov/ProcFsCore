using System;

namespace ProcFsCore;

public struct Utf8FileReader : IUtf8Reader
{
    internal static readonly ReadOnlyMemory<byte> DefaultWhiteSpaces = " \n\t\v\f\r"u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> DefaultLineSeparators = "\n\r"u8.ToArray();

    private readonly LightFileStream _stream;
    private readonly ReadOnlyMemory<byte> _whiteSpaces;
    private readonly ReadOnlyMemory<byte> _lineSeparators;

    private Buffer<byte> _buffer;
    private int _lockedStart;
    private int _bufferedStart;
    private int _bufferedEnd;
    private bool _endOfStream;

    public ReadOnlySpan<byte> WhiteSpaces => _whiteSpaces.Span;
    public ReadOnlySpan<byte> LineSeparators => _lineSeparators.Span;

    public bool EndOfStream => _endOfStream && _bufferedStart == _bufferedEnd;

    public Utf8FileReader(string fileName, int initialBufferSize = 0, ReadOnlyMemory<byte>? whiteSpaces = null, ReadOnlyMemory<byte>? lineSeparators = null)
    {
        _stream = LightFileStream.OpenRead(fileName);
        _whiteSpaces = whiteSpaces ?? DefaultWhiteSpaces;
        _lineSeparators = lineSeparators ?? DefaultLineSeparators;

        _buffer = new Buffer<byte>(initialBufferSize);
        _lockedStart = -1;
        _bufferedStart = 0;
        _bufferedEnd = 0;
        _endOfStream = false;
    }

    public void Dispose()
    {
        _buffer.Dispose();
        _stream.Dispose();
    }

    private void ReadToBuffer()
    {
        if (_endOfStream)
            return;

        var bufferSpan = _buffer.Span;
        if (_bufferedEnd < _buffer.Length)
        {
            var bytesRead = _stream.Read(bufferSpan[_bufferedEnd..]);
            _bufferedEnd += bytesRead;
            _endOfStream = bytesRead == 0;
            return;
        }

        if (_bufferedStart > 0 && _lockedStart == -1 || _lockedStart > 0)
        {
            var start = _lockedStart == -1 ? _bufferedStart : _lockedStart;
            bufferSpan.Slice(start).CopyTo(bufferSpan);
            _bufferedEnd -= start;
            _bufferedStart -= start;
            if (_lockedStart > 0)
                _lockedStart = 0;
            ReadToBuffer();
            return;
        }
            
        _buffer.Resize(_buffer.Length * 2);
        ReadToBuffer();
    }

    private void EnsureReadToBuffer()
    {
        if (_bufferedStart == _bufferedEnd)
            ReadToBuffer();
    }

    private void LockBuffer()
    {
        _lockedStart = _bufferedStart;
    }

    private void UnlockBuffer()
    {
        _lockedStart = -1;
        if (_bufferedStart == _bufferedEnd && _bufferedStart != 0)
            _bufferedStart = _bufferedEnd = 0;
    }

    private void ConsumeBuffer(int count)
    {
        _bufferedStart += count;
        if (_bufferedStart == _bufferedEnd && _lockedStart == -1)
        {
            _bufferedStart = 0;
            _bufferedEnd = 0;
        }
    }
        
    public void SkipSeparators(scoped ReadOnlySpan<byte> separators)
    {
        EnsureReadToBuffer();
        while (!EndOfStream && separators.IndexOf(_buffer.Span[_bufferedStart]) >= 0)
        {
            ConsumeBuffer(1);
            EnsureReadToBuffer();
        }
    }
        
    public void SkipFragment(scoped ReadOnlySpan<byte> separators, bool isSingleString = false)
    {
        if (EndOfStream)
            return;

        var consumePadding = isSingleString ? separators.Length : 0;
        EnsureReadToBuffer();

        while (!_endOfStream)
        {

            var separatorPos = isSingleString
                ? _buffer.Span.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).IndexOf(separators)
                : _buffer.Span.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).IndexOfAny(separators);
            if (separatorPos >= 0)
            {
                ConsumeBuffer(separatorPos);
                break;
            }

            ConsumeBuffer(Math.Max(_bufferedEnd - _bufferedStart - consumePadding, 0));
            ReadToBuffer();
        }

        if (!EndOfStream)
            SkipSeparators(separators);
    }
        
    public ReadOnlySpan<byte> ReadFragment(scoped ReadOnlySpan<byte> separators)
    {
        if (EndOfStream)
            return default;

        EnsureReadToBuffer();

        var separatorPos = -1;
        while (!_endOfStream)
        {
            separatorPos = _buffer.Span.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).IndexOfAny(separators);
            if (separatorPos >= 0)
                break;
            ReadToBuffer();
        }

        if (separatorPos < 0)
        {
            var resultLength = _bufferedEnd - _bufferedStart;
            LockBuffer();
            ConsumeBuffer(_bufferedEnd - _bufferedStart);
            var result = _buffer.Span.Slice(_lockedStart, resultLength);
            UnlockBuffer();
            return result;
        }
        else
        {
            LockBuffer();
            ConsumeBuffer(separatorPos + 1);
            SkipSeparators(separators);
            var result = _buffer.Span.Slice(_lockedStart, separatorPos);
            UnlockBuffer();
            return result;
        }
    }
        
    public ReadOnlySpan<byte> ReadToEnd()
    {
        while (!_endOfStream)
            ReadToBuffer();
        var result = _buffer.Span.Slice(_bufferedStart, _bufferedEnd);
        ConsumeBuffer(_bufferedEnd - _bufferedStart);
        return result;
    }

    public override string ToString() => _buffer.Span.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).ToUtf8String();
}