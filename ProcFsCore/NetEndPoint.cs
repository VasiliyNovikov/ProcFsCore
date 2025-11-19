using System;
using System.Buffers;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using NetworkingPrimitivesCore;

namespace ProcFsCore;

public readonly struct NetEndPoint<TAddress>
    where TAddress : unmanaged, IIPAddress<TAddress>
{
    private static readonly SearchValues<byte> AddressPortSeparator = SearchValues.Create(":"u8);

    public TAddress Address { get; }
    public int Port { get; }
    public bool IsEmpty => Address == default && Port == 0;

    private NetEndPoint(in TAddress address, int port)
    {
        Address = address;
        Port = port;
    }

    [SkipLocalsInit]
    internal static NetEndPoint<TAddress> Read(in AsciiFileReader reader)
    {
        ref var readerRef = ref Unsafe.AsRef(in reader);
        return new NetEndPoint<TAddress>(FromHexString(readerRef.ReadWord(AddressPortSeparator)), readerRef.ReadInt32('x'));
    }

    private static TAddress FromHexString(ReadOnlySpan<byte> addressHex)
    {
        return typeof(TAddress) == typeof(IPv4Address)
            ? FromHexString<uint>(addressHex)
            : FromHexString<UInt128>(addressHex);
    }

    private static TAddress FromHexString<TUInt>(ReadOnlySpan<byte> addressHex)
        where TUInt : unmanaged, IBinaryInteger<TUInt>, IUnsignedNumber<TUInt>
    {
        return TUInt.TryParse(addressHex, NumberStyles.HexNumber, null, out var addressInt)
            ? Unsafe.BitCast<TUInt, TAddress>(addressInt)
            : throw new FormatException($"Invalid address format: {addressHex.ToAsciiString()}");
    }

    public override string? ToString() => ((IPEndPoint?)this)?.ToString();

    public static implicit operator IPEndPoint(in NetEndPoint<TAddress> endPoint) => new((IPAddress)endPoint.Address, endPoint.Port);
}