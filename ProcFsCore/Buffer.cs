using System;
using System.Buffers;
using System.IO;

namespace ProcFsCore
{
    public unsafe struct Buffer : IDisposable
    {
        public const int MinimumCapacity = 512;

        private byte[] _rentedBuffer;
        private fixed byte _fixedBuffer[MinimumCapacity];

        public int Length { get; private set; }

        public Span<byte> Span
        {
            get
            {
                if (Length == 0)
                    return default;
                
                if (_rentedBuffer == null)
                    fixed (byte* d = &_fixedBuffer[0])
                        return new Span<byte>(d, Length);
                
                return new Span<byte>(_rentedBuffer, 0, Length);
            }
        }

        public Buffer(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
            _rentedBuffer = Length > MinimumCapacity ? ArrayPool<byte>.Shared.Rent(Length) : null;
        }

        public void Resize(int newLength)
        {
            if (newLength < 0) throw new ArgumentOutOfRangeException(nameof(newLength));
            if (newLength > Length)
            {
                var currentBufferCapacity = Length > MinimumCapacity ? _rentedBuffer.Length : MinimumCapacity;
                if (newLength > currentBufferCapacity)
                {
                    var newBuffer = ArrayPool<byte>.Shared.Rent(newLength);
                    Span.CopyTo(newBuffer);
                    if (_rentedBuffer != null)
                        ArrayPool<byte>.Shared.Return(_rentedBuffer);
                    _rentedBuffer = newBuffer;
                }
            }

            Length = newLength;
        }

        public void Dispose()
        {
            if (_rentedBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_rentedBuffer);
                _rentedBuffer = null;
            }
            Length = 0;
        }


        public static Buffer FromStream(Stream stream, int? estimatedLength = null)
        {
            var actualEstimatedLength = estimatedLength ?? (stream.CanSeek ? (int)stream.Length : MinimumCapacity);
            if (actualEstimatedLength == 0)
                actualEstimatedLength = MinimumCapacity;
            
            var buffer = new Buffer(actualEstimatedLength);
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
}