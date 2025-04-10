namespace System.Runtime.InteropServices;

internal static class MemoryMarshalExtensions
{
    public static Span<T> CreateSpan<T>(scoped ref T data, int length) where T : unmanaged
    {
#if NETSTANDARD2_0
        unsafe
        {
            fixed (T* ptr = &data)
                return new Span<T>(ptr, length);
        }
#else
        return MemoryMarshal.CreateSpan(ref data, length);
#endif
    }
}