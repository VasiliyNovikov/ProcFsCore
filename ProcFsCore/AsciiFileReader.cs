using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

internal struct AsciiFileReader(string fileName, int initialBufferSize = 0) : IAsciiReader
{
    private readonly LightFileStream _stream = LightFileStream.OpenRead(fileName);

    private byte[] _buffer = ArrayPool<byte>.Shared.Rent(initialBufferSize);
    private int _lockedStart = -1;
    private int _bufferedStart = 0;
    private int _bufferedEnd = 0;
    private bool _endOfStream = false;

    private Span<byte> BufferSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer;
    }

    public bool EndOfStream
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _endOfStream && _bufferedStart == _bufferedEnd;
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
        if (_endOfStream)
            return;

        var bufferSpan = BufferSpan;
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
            bufferSpan[start..].CopyTo(bufferSpan);
            _bufferedEnd -= start;
            _bufferedStart -= start;
            if (_lockedStart > 0)
                _lockedStart = 0;
            ReadToBuffer();
            return;
        }

        var newBuffer = ArrayPool<byte>.Shared.Rent(_buffer.Length + 1);
        bufferSpan.CopyTo(newBuffer);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuffer;
        ReadToBuffer();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureReadToBuffer()
    {
        if (_bufferedStart == _bufferedEnd)
            ReadToBuffer();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LockBuffer() => _lockedStart = _bufferedStart;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnlockBuffer()
    {
        _lockedStart = -1;
        if (_bufferedStart == _bufferedEnd && _bufferedStart != 0)
            _bufferedStart = _bufferedEnd = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ConsumeBuffer(int count)
    {
        _bufferedStart += count;
        if (_bufferedStart == _bufferedEnd && _lockedStart == -1)
        {
            _bufferedStart = 0;
            _bufferedEnd = 0;
        }
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipSeparators(scoped ReadOnlySpan<byte> separators)
    {
        EnsureReadToBuffer();
        while (!EndOfStream && separators.IndexOf(BufferSpan[_bufferedStart]) >= 0)
        {
            ConsumeBuffer(1);
            EnsureReadToBuffer();
        }
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipFragment(scoped ReadOnlySpan<byte> separators, bool isSingleString = false)
    {
        if (EndOfStream)
            return;

        var consumePadding = isSingleString ? separators.Length : 0;
        EnsureReadToBuffer();

        while (!_endOfStream)
        {

            var separatorPos = isSingleString
                ? BufferSpan.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).IndexOf(separators)
                : BufferSpan.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).IndexOfAny(separators);
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
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadFragment(scoped ReadOnlySpan<byte> separators)
    {
        if (EndOfStream)
            return default;

        EnsureReadToBuffer();

        var separatorPos = -1;
        while (!_endOfStream)
        {
            separatorPos = BufferSpan.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).IndexOfAny(separators);
            if (separatorPos >= 0)
                break;
            ReadToBuffer();
        }

        if (separatorPos < 0)
        {
            var resultLength = _bufferedEnd - _bufferedStart;
            LockBuffer();
            ConsumeBuffer(_bufferedEnd - _bufferedStart);
            var result = BufferSpan.Slice(_lockedStart, resultLength);
            UnlockBuffer();
            return result;
        }
        else
        {
            LockBuffer();
            ConsumeBuffer(separatorPos + 1);
            SkipSeparators(separators);
            var result = BufferSpan.Slice(_lockedStart, separatorPos);
            UnlockBuffer();
            return result;
        }
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadToEnd()
    {
        while (!_endOfStream)
            ReadToBuffer();
        var result = BufferSpan.Slice(_bufferedStart, _bufferedEnd);
        ConsumeBuffer(_bufferedEnd - _bufferedStart);
        return result;
    }

    public override string ToString() => BufferSpan.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).ToAsciiString();
}