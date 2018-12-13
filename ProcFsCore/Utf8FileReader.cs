using System;
using System.Buffers.Text;
using System.IO;

namespace ProcFsCore
{
    public struct Utf8FileReader : IDisposable
    {
        private readonly Stream _stream;
        private Buffer _buffer;
        private int _bufferedStart;
        private int _bufferedEnd;
        private bool _endOfStream;

        public bool EndOfStream => _endOfStream && _bufferedStart == _bufferedEnd;

        public Utf8FileReader(string fileName, int initialBufferSize = Buffer.MinimumCapacity)
        {
            _stream = File.OpenRead(fileName);
            _buffer = new Buffer(initialBufferSize);
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

            if (_bufferedStart > 0)
            {
                bufferSpan.Slice(_bufferedStart).CopyTo(bufferSpan);
                _bufferedEnd -= _bufferedStart;
                _bufferedStart = 0;
                ReadToBuffer();
                return;
            }
            
            _buffer.Resize(_buffer.Length * 2);
            ReadToBuffer();
        }

        private void EnsureReadToBuffer(int minCount = 0)
        {
            if (_bufferedStart + minCount >= _bufferedEnd)
                ReadToBuffer();
        }

        private void ConsumeBuffer(int count)
        {
            _bufferedStart += count;
            if (_bufferedStart == _bufferedEnd)
            {
                _bufferedStart = 0;
                _bufferedEnd = 0;
            }
        }
        
        public int SkipSeparators(ReadOnlySpan<byte> separators, bool consume = true)
        {
            var count = 0;
            EnsureReadToBuffer();
            while (!EndOfStream && separators.IndexOf(_buffer.Span[_bufferedStart]) >= 0)
            {
                ++count;
                if (consume)
                {
                    ConsumeBuffer(1);
                    EnsureReadToBuffer();
                }
                else
                    EnsureReadToBuffer(count);
            }

            return count;
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
                var result = _buffer.Span.Slice(_bufferedStart, _bufferedEnd - _bufferedStart);
                ConsumeBuffer(_bufferedEnd - _bufferedStart);
                return result;
            }
            else
            {
                var result = _buffer.Span.Slice(_bufferedStart, separatorPos);
                ConsumeBuffer(separatorPos + 1);
                SkipSeparators(separators);
                return result;
            }
        }
        
        public unsafe ReadOnlySpan<byte> ReadFragment(char separator)
        {
            var separatorsBuff = stackalloc byte[1] {(byte) separator};
            var separators = new ReadOnlySpan<byte>(separatorsBuff, 1);
            return ReadFragment(separators);
        }

        private static readonly ReadOnlyMemory<byte> LineSeparators = "\n\r".ToUtf8();
        public ReadOnlySpan<byte> ReadLine() => ReadFragment(LineSeparators.Span);

        private static readonly ReadOnlyMemory<byte> WhiteSpaces = " \n \t\v\f\r\x0085".ToUtf8();
        public ReadOnlySpan<byte> ReadWord() => ReadFragment(WhiteSpaces.Span);
        public void SkipWhiteSpaces() => SkipSeparators(WhiteSpaces.Span);

        public string ReadStringWord() => ReadWord().ToUtf8String();

        public short ReadInt16()
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out short result, out _))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int16 value");
        }
        
        public int ReadInt32()
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out int result, out _))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int32 value");
        }
        
        public long ReadInt64()
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out long result, out _))
                return result;
            throw new FormatException($"{word.ToUtf8String()} is not valid Int64 value");
        }

        public override string ToString() => _buffer.Span.Slice(_bufferedStart, _bufferedEnd - _bufferedStart).ToUtf8String();
    }
}