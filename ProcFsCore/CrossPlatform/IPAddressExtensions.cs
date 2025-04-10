using System.Runtime.CompilerServices;

namespace System.Net;

internal static class IPAddressExtensions
{
    public static IPAddress Parse(ReadOnlySpan<char> address)
    {
#if NETSTANDARD2_0
        return IPAddress.Parse(address.ToString());
#else
        return IPAddress.Parse(address);
#endif
    }

    public static IPAddress FromBytes(ReadOnlySpan<byte> address)
    {
#if NETSTANDARD2_0
        return new(address.ToArray());
#else
        return new(address);
#endif
    }

#if NETSTANDARD2_0
    public static bool TryWriteBytes(this IPAddress address, Span<byte> destination, out int bytesWritten)
    {
        Span<byte> addressBytes = address.GetAddressBytes();
        if (addressBytes.Length > destination.Length)
        {
            Unsafe.SkipInit(out bytesWritten);
            return false;
        }
        bytesWritten = addressBytes.Length;
        addressBytes.CopyTo(destination);
        return true;
    }
#endif
}