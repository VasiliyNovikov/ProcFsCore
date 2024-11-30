using System;
using System.Text;
using System.Buffers.Text;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ProcFsCore;

public unsafe struct NetAddress
{
    private const int MaxAddressLength = 16; 
#pragma warning disable 649
    private fixed byte _data[MaxAddressLength];
#pragma warning restore 649
    private readonly int _length;

    private Span<byte> Data => CrossPlatformMemoryMarshal.CreateSpan(ref _data[0], _length);

    public NetAddressVersion Version => _length == 4 ? NetAddressVersion.IPv4 : NetAddressVersion.IPv6;

    public bool IsEmpty
    {
        get
        {
            foreach (var part in Data)
                if (part != 0)
                    return false;
            return true;
        }
    }

    private NetAddress(ReadOnlySpan<byte> address)
    {
        _length = address.Length;
        address.CopyTo(Data);    
    }

    [SkipLocalsInit]
    internal static NetAddress Parse(ReadOnlySpan<byte> addressString, NetAddressFormat format)
    {
        switch (format)
        {
            case NetAddressFormat.Hex:
            {
                Span<uint> address = stackalloc uint[MaxAddressLength >> 2];
                var addressLength = addressString.Length >> 3;
                for (var i = 0; i < addressLength; ++i)
                {
                    var hexPart = addressString.Slice(i << 3, 8);
#pragma warning disable CA1806
                    Utf8Parser.TryParse(hexPart, out uint addressPart, out _, 'x');
#pragma warning restore CA1806
                    address[i] = addressPart;
                }

                return new NetAddress(MemoryMarshal.Cast<uint, byte>(address.Slice(0, addressLength)));
            }
            case NetAddressFormat.Human:
            {
#if NETSTANDARD2_0
                    var addressStr = Encoding.UTF8.GetString(addressString);
                    var frameworkAddress = IPAddress.Parse(addressStr);
                    var addressBytes = frameworkAddress.GetAddressBytes();
                    return new NetAddress(addressBytes);
#else
                Span<char> addressStr = stackalloc char[64];
                Utf8Extensions.Encoding.GetChars(addressString, addressStr);
                var frameworkAddress = IPAddress.Parse(addressStr[..addressString.Length]);
                Span<byte> addressBytes = stackalloc byte[MaxAddressLength];
                frameworkAddress.TryWriteBytes(addressBytes, out var addressLength);
                return new NetAddress(addressBytes[..addressLength]);
#endif
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    public override string ToString() => ((IPAddress)this).ToString();

    public static implicit operator IPAddress(in NetAddress address) =>
#if NETSTANDARD2_0
        new(address.Data.ToArray());
#else
        new(address.Data);
#endif
}