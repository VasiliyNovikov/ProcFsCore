#if NETSTANDARD2_0
using System.Runtime.InteropServices;

namespace System.Text;

internal static class EncodingExtensions
{
    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            return encoding.GetString(bytesPtr, bytes.Length);
    }

    public static unsafe int GetChars(this Encoding encoding, ReadOnlySpan<byte> bytes, Span<char> chars)
    {
        fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
        fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
            return encoding.GetChars(bytesPtr, bytes.Length, charsPtr, chars.Length);
    }
}
#endif