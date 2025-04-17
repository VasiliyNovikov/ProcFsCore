#if NETSTANDARD2_0
namespace System.Runtime.InteropServices;

internal static class MemoryMarshalExtensions
{
    extension(MemoryMarshal)
    {
        public static unsafe Span<T> CreateSpan<T>(scoped ref T reference, int length) where T : unmanaged
        {
            fixed (T* ptr = &reference)
                return new Span<T>(ptr, length);
        }
    }
}
#endif