using System;
using System.Buffers.Text;

namespace ProcFsCore
{
    public ref struct Utf8SpanReader
    {
        private readonly ReadOnlySpan<byte> _span;
        private int _position;

        public Utf8SpanReader(ReadOnlySpan<byte> span)
        {
            _span = span;
            _position = 0;
        }

        public int Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _span.Length)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _position = value;
            }
        }

        public bool EndOfSpan => _position == _span.Length;

        public void SkipSeparators(ReadOnlySpan<byte> separators)
        {
            while (!EndOfSpan && separators.IndexOf(_span[_position]) >= 0)
                ++_position;
        }

        public ReadOnlySpan<byte> ReadFragment(ReadOnlySpan<byte> separators)
        {
            if (EndOfSpan)
                return default;

            var remainingSpan = _span.Slice(_position);
            var separatorPos = remainingSpan.IndexOfAny(separators);
            if (separatorPos < 0)
            {
                var result = remainingSpan;
                _position = _span.Length;
                return result;
            }
            else
            {
                var result = remainingSpan.Slice(0, separatorPos);
                _position += separatorPos + 1;
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

        public override string ToString() => _span.Slice(_position).ToUtf8String();
    }
}