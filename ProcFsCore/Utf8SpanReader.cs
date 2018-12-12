using System;
using System.Buffers.Text;

namespace ProcFsCore
{
    public ref struct Utf8SpanReader
    {
        private ReadOnlySpan<byte> _span;

        public Utf8SpanReader(ReadOnlySpan<byte> span)
        {
            _span = span;
        }

        public bool EndOfSpan => _span.Length == 0;

        public ReadOnlySpan<byte> ReadFragment(ReadOnlySpan<byte> separators)
        {
            if (EndOfSpan)
                return default;
            
            var separatorPos = _span.IndexOfAny(separators);
            if (separatorPos < 0)
            {
                var result = _span;
                _span = default;
                return result;
            }
            else
            {
                var result = _span.Slice(0, separatorPos);
                _span = _span.Slice(separatorPos + 1);
                while (!_span.IsEmpty && separators.IndexOf(_span[0]) >= 0)
                    _span = _span.Slice(1);
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
    }
}