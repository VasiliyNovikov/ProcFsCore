#if NETSTANDARD2_0
namespace System.Buffers;

internal static class SearchValues
{
    public static SearchValues<byte> Create(params ReadOnlySpan<byte> values) => new(values);
}

internal class SearchValues<T>(ReadOnlySpan<T> values) where T : IEquatable<T>?
{
    private readonly T[] _values = [.. values];
    internal ReadOnlySpan<T> Span => _values;
}
#endif