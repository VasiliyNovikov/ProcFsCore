using System;
using System.Text;
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

    private Span<byte> Data => MemoryMarshal.CreateSpan<byte>(ref _data[0], _length);

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
                    address[i] = AsciiParser.Parse<uint>(hexPart, 'x');
                }

                return new NetAddress(MemoryMarshal.Cast<uint, byte>(address[..addressLength]));
            }
            case NetAddressFormat.Human:
            {
                Span<char> addressStr = stackalloc char[64];
                var addressStrLen = AsciiExtensions.Encoding.GetChars(addressString, addressStr);
                var frameworkAddress = IPAddress.Parse(addressStr[..addressStrLen]);
                Span<byte> addressBytes = stackalloc byte[MaxAddressLength];
                frameworkAddress.TryWriteBytes(addressBytes, out var addressBytesLen);
                return new NetAddress(addressBytes[..addressBytesLen]);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    public override string ToString() => ((IPAddress)this).ToString();

    public static implicit operator IPAddress(in NetAddress address) => IPAddress.FromBytes(address.Data);
}