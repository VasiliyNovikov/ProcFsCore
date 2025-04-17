#if NETSTANDARD2_0
using System.Runtime.InteropServices;

namespace System.Text;

internal static class EncodingExtensions
{
    extension(Encoding encoding)
    {
        public unsafe string GetString(ReadOnlySpan<byte> bytes)
        {
            if (bytes.IsEmpty)
                return string.Empty;
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
                return encoding.GetString(bytesPtr, bytes.Length);
        }

        public unsafe int GetChars(ReadOnlySpan<byte> bytes, Span<char> chars)
        {
            if (bytes.IsEmpty)
                return 0;
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                return encoding.GetChars(bytesPtr, bytes.Length, charsPtr, chars.Length);
        }
    }
}
#endif