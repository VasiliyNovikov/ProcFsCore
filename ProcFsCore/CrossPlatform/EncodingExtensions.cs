#if NETSTANDARD2_0
namespace System.Text;

internal static class EncodingExtensions
{
    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return String.Empty;
        fixed (byte* bytesPtr = bytes)
            return encoding.GetString(bytesPtr, bytes.Length);
    }
}
#endif