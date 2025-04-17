namespace System.Net;

internal static class IPAddressExtensions
{
    extension(IPAddress address)
    {
#if NETSTANDARD2_0
        public static IPAddress Parse(ReadOnlySpan<char> ipSpan) => IPAddress.Parse(ipSpan.ToString());

        public bool TryWriteBytes(Span<byte> destination, out int bytesWritten)
        {
            Span<byte> addressBytes = address.GetAddressBytes();
            if (addressBytes.Length > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = addressBytes.Length;
            addressBytes.CopyTo(destination);
            return true;
        }

        public static IPAddress FromBytes(ReadOnlySpan<byte> addressBytes) => new(addressBytes.ToArray());
#else
        public static IPAddress FromBytes(ReadOnlySpan<byte> addressBytes) => new(addressBytes);
#endif
    }
}