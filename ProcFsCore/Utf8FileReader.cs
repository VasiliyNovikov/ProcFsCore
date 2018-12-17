using System;
using System.Buffers.Text;
using System.IO;

namespace ProcFsCore
{
    public struct Utf8FileReader : IDisposable
    {
        internal static readonly ReadOnlyMemory<byte> DefaultWhiteSpaces = " \n \t\v\f\r\x0085".ToUtf8();
        internal static readonly ReadOnlyMemory<byte> DefaultLineSeparators = "\n\r".ToUtf8();

        private readonly Stream _stream;
        private readonly ReadOnlyMemory<byte> _whiteSpaces;
        private readonly ReadOnlyMemory<byte> _lineSeparators;

        private Buffer _buffer;
        private int _lockedStart;
        private int _bufferedStart;
        private int _bufferedEnd;
        private bool _endOfStream;

        public bool EndOfStream => _endOfStream && _bufferedStart == _bufferedEnd;

        public Utf8FileReader(string fileName, int initialBufferSize = Buffer.MinimumCapacity, ReadOnlyMemory<byte>? whiteSpaces = null, ReadOnlyMemory<byte>? lineSeparators = null)
        {
            _stream = File.OpenRead(fileName);
            _whiteSpaces = whiteSpaces ?? DefaultWhiteSpaces;
            _lineSeparators = lineSeparators ?? DefaultLineSeparators;

            _buffer = new Buffer(initialBufferSize);
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
                var bytesRead = _stream.Read(bufferSpan.Slice(_bufferedEnd));
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
        
        public void SkipSeparators(ReadOnlySpan<byte> separators)
        {
            EnsureReadToBuffer();
            while (!EndOfStream && separators.IndexOf(_buffer.Span[_bufferedStart]) >= 0)
            {
                ConsumeBuffer(1);
                EnsureReadToBuffer();
            }
        }
        
        public void SkipFragment(ReadOnlySpan<byte> separators, bool isSingleString = false)
        {
            if (EndOfStream)
                return;

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

                ConsumeBuffer(_bufferedEnd - _bufferedStart);
                EnsureReadToBuffer();
            }

            if (!EndOfStream)
                SkipSeparators(separators);
        }
        
        public unsafe void SkipFragment(char separator)
        {
            var separatorsBuff = stackalloc byte[1] {(byte) separator};
            var separators = new ReadOnlySpan<byte>(separatorsBuff, 1);
            SkipFragment(separators);
        }
        
        public ReadOnlySpan<byte> ReadFragment(ReadOnlySpan<byte> separators)
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
        
        public unsafe ReadOnlySpan<byte> ReadFragment(char separator)
        {
            var separatorsBuff = stackalloc byte[1] {(byte) separator};
            var separators = new ReadOnlySpan<byte>(separatorsBuff, 1);
            return ReadFragment(separators);
        }

        public ReadOnlySpan<byte> ReadToEnd()
        {
            while (!_endOfStream)
                ReadToBuffer();
            var result = _buffer.Span.Slice(_bufferedStart, _bufferedEnd);
            ConsumeBuffer(_bufferedEnd - _bufferedStart);
            return result;
        }

        public ReadOnlySpan<byte> ReadLine() => ReadFragment(_lineSeparators.Span);
        public void SkipLine() => SkipFragment(_lineSeparators.Span);

        public ReadOnlySpan<byte> ReadWord() => ReadFragment(_whiteSpaces.Span);
        public void SkipWord() => SkipFragment(_whiteSpaces.Span);
        public void SkipWhiteSpaces() => SkipSeparators(_whiteSpaces.Span);

        public string ReadStringWord() => ReadWord().ToUtf8String();

        public short ReadInt16(char format = default)
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out short result, out _, format))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int16 value");
        }
        
        public int ReadInt32(char format = default)
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out int result, out _, format))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int32 value");
        }
        
        public long ReadInt64(char format = default)
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out long result, out _, format))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int64 value");
        }

        public override string ToString() => _buffer.Span.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).ToUtf8String();
    }

    public ref struct Utf8SpanReader
    {
        private readonly ReadOnlySpan<byte> _span;
        private readonly ReadOnlySpan<byte> _whiteSpaces;
        private readonly ReadOnlySpan<byte> _lineSeparators;
        
        private int _position;
        
        public bool EndOfSpan => _position == _span.Length;

        public Utf8SpanReader(ReadOnlySpan<byte> span, ReadOnlyMemory<byte>? whiteSpaces = null, ReadOnlyMemory<byte>? lineSeparators = null)
        {
            _span = span;
            _whiteSpaces = (whiteSpaces ?? Utf8FileReader.DefaultWhiteSpaces).Span;
            _lineSeparators = (lineSeparators ?? Utf8FileReader.DefaultLineSeparators).Span;
            _position = 0;
        }
        
        public void SkipSeparators(ReadOnlySpan<byte> separators)
        {
            while (!EndOfSpan && separators.IndexOf(_span[_position]) >= 0)
                ++_position;
        }
        
        public void SkipFragment(ReadOnlySpan<byte> separators)
        {
            if (EndOfSpan)
                return;

            var separatorPos = _span.Slice(_position).IndexOfAny(separators);

            if (separatorPos < 0)
            {
                _position = _span.Length;
                return;
            }

            _position += separatorPos + 1;
            SkipSeparators(separators);
        }
        
        public ReadOnlySpan<byte> ReadFragment(ReadOnlySpan<byte> separators)
        {
            if (EndOfSpan)
                return default;

            var span = _span.Slice(_position);
            var separatorPos = span.IndexOfAny(separators);

            if (separatorPos < 0)
            {
                _position = _span.Length;
                return span;
            }

            var result = span.Slice(0, separatorPos);
            _position += separatorPos + 1;
            SkipSeparators(separators);
            return result;
        }
        
        public unsafe ReadOnlySpan<byte> ReadFragment(char separator)
        {
            var separatorsBuff = stackalloc byte[1] {(byte) separator};
            var separators = new ReadOnlySpan<byte>(separatorsBuff, 1);
            return ReadFragment(separators);
        }

        public ReadOnlySpan<byte> ReadToEnd()
        {
            var result = _span.Slice(_position);
            _position = _span.Length;
            return result;
        }

        public ReadOnlySpan<byte> ReadLine() => ReadFragment(_lineSeparators);
        public void SkipLine() => SkipFragment(_lineSeparators);

        public ReadOnlySpan<byte> ReadWord() => ReadFragment(_whiteSpaces);
        public void SkipWord() => SkipFragment(_whiteSpaces);
        public void SkipWhiteSpaces() => SkipSeparators(_whiteSpaces);

        public string ReadStringWord() => ReadWord().ToUtf8String();

        public short ReadInt16(char format = default)
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out short result, out _, format))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int16 value");
        }
        
        public int ReadInt32(char format = default)
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out int result, out _, format))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int32 value");
        }
        
        public long ReadInt64(char format = default)
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out long result, out _, format))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int64 value");
        }

        public override string ToString() => _span.ToUtf8String();
    }
}