using System;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

internal struct AsciiSpanReader : IAsciiReader
{
    private readonly unsafe byte* _pointer;
    private readonly int _length;

    private int _position;

    private unsafe ReadOnlySpan<byte> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_pointer, _length);
    }

    public bool EndOfStream
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _position == _length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe AsciiSpanReader(ReadOnlySpan<byte> span)
    {
        fixed(byte* ptr = &span.GetPinnableReference())
            _pointer = ptr;
        _length = span.Length;
        _position = 0;
    }

    public void Dispose()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipSeparators(scoped ReadOnlySpan<byte> separators)
    {
        var span = Span;
        while (!EndOfStream && separators.IndexOf(span[_position]) >= 0)
            ++_position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipFragment(scoped ReadOnlySpan<byte> separators, bool isSingleString = false)
    {
        if (EndOfStream)
            return;

        var separatorPos = isSingleString 
            ? Span[_position..].IndexOf(separators)
            : Span[_position..].IndexOfAny(separators);

        if (separatorPos < 0)
        {
            _position = _length;
            return;
        }

        _position += separatorPos + 1;
        SkipSeparators(separators);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadFragment(scoped ReadOnlySpan<byte> separators)
    {
        if (EndOfStream)
            return default;

        var span = Span[_position..];
        var separatorPos = span.IndexOfAny(separators);

        if (separatorPos < 0)
        {
            _position = _length;
            return span;
        }

        var result = span[..separatorPos];
        _position += separatorPos + 1;
        SkipSeparators(separators);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadToEnd()
    {
        var result = Span[_position..];
        _position = _length;
        return result;
    }

    public override string ToString() => Span.ToAsciiString();
}