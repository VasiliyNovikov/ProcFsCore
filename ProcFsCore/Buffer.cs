using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

public struct Buffer<T> : IDisposable
    where T : unmanaged
{
    private T[]? _rentedBuffer;

    public int Length { get; private set; }

    public Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Length == 0 ? default : new Span<T>(_rentedBuffer, 0, Length);
    }

    public Buffer(int length, int capacity = 0)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        Length = length;
        if (length > capacity)
            capacity = length;
        _rentedBuffer = capacity == 0 ? null : ArrayPool<T>.Shared.Rent(capacity);
    }

    public void Resize(int newLength)
    {
        if (newLength < 0) throw new ArgumentOutOfRangeException(nameof(newLength));
        if (newLength > _rentedBuffer?.Length)
        {
            var newBuffer = ArrayPool<T>.Shared.Rent(newLength);
            if (_rentedBuffer != null)
            {
                Span.CopyTo(newBuffer);
                ArrayPool<T>.Shared.Return(_rentedBuffer);
            }
            _rentedBuffer = newBuffer;
        }
        Length = newLength;
    }

    public void Dispose()
    {
        if (_rentedBuffer == null)
            return;

        ArrayPool<T>.Shared.Return(_rentedBuffer);
        _rentedBuffer = null;
        Length = 0;
    }

    public static Buffer<byte> FromFile(string fileName, int sizePrediction = 0)
    {
        using var stream = LightFileStream.OpenRead(fileName);
        var buffer = new Buffer<byte>(sizePrediction);
        var totalReadBytes = 0;
        while (true)
        {
            var readBytes = stream.Read(buffer.Span.Slice(totalReadBytes));
            if (readBytes == 0)
                break;

            totalReadBytes += readBytes;
            if (totalReadBytes == buffer.Length)
                buffer.Resize(buffer.Length * 2);
        }
        buffer.Resize(totalReadBytes);
        return buffer;
    }
}