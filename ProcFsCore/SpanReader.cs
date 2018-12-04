using System;
using System.Buffers.Text;
using System.Text;

namespace ProcFsCore
{
    public ref struct Utf8SpanReader
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
        
        private ReadOnlySpan<byte> _span;

        public Utf8SpanReader(ReadOnlySpan<byte> span)
        {
            _span = span;
        }

        public bool EndOfSpan => _span.Length == 0;
        
        public ReadOnlySpan<byte> ReadWord()
        {
            if (EndOfSpan)
                return default;
            
            var spacePos = _span.IndexOf((byte) ' ');
            if (spacePos < 0)
            {
                _span = default;
                return _span;
            }

            var result = _span.Slice(0, spacePos);
            _span = _span.Slice(spacePos + 1);
            return result;
        }

        public string ReadStringWord() => Encoding.GetString(ReadWord());

        public short ReadInt16()
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out short result, out _))
                return result;
            throw new FormatException($"{Encoding.GetString(word)} is not valid Int16 value");
        }
        
        public int ReadInt32()
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out int result, out _))
                return result;
            throw new FormatException($"{Encoding.GetString(word)} is not valid Int32 value");
        }
        
        public long ReadInt64()
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out long result, out _))
                return result;
            throw new FormatException($"{Encoding.GetString(word)} is not valid Int64 value");
        }

        public ulong ReadUInt64()
        {
            var word = ReadWord();
            if (Utf8Parser.TryParse(word, out ulong result, out _))
                return result;
            throw new FormatException($"{Encoding.GetString(word)} is not valid UInt64 value");
        }
    }
}