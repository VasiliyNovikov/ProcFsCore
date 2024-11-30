namespace System.Runtime.InteropServices;

public static class CrossPlatformMemoryMarshal
{
    public static Span<byte> CreateSpan(ref byte data, int length)
    {
#if NETSTANDARD2_0
        unsafe
        {
            fixed (byte* ptr = &data)
                return new Span<byte>(ptr, length);
        }
#else
        return MemoryMarshal.CreateSpan(ref data, length);
#endif
    }
}