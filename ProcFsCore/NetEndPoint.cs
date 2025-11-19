using System;
using System.Buffers;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using NetworkingPrimitivesCore;

namespace ProcFsCore;

public readonly struct NetEndPoint<TAddress, TUInt>
    where TAddress : unmanaged, IIPAddress<TAddress, TUInt>
    where TUInt : unmanaged, IBinaryInteger<TUInt>, IUnsignedNumber<TUInt>
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
    internal static NetEndPoint<TAddress, TUInt> Read(in AsciiFileReader reader)
    {
        ref var readerRef = ref Unsafe.AsRef(in reader);
        return new NetEndPoint<TAddress, TUInt>(FromHexString(readerRef.ReadWord(AddressPortSeparator)), readerRef.ReadInt32('x'));
    }

    private static TAddress FromHexString(ReadOnlySpan<byte> addressHex)
    {
        return TUInt.TryParse(addressHex, NumberStyles.HexNumber, null, out var addressInt)
            ? Unsafe.BitCast<TUInt, TAddress>(addressInt)
            : throw new FormatException($"Invalid address format: {addressHex.ToAsciiString()}");
    }

    public override string? ToString() => ((IPEndPoint?)this)?.ToString();

    public static implicit operator IPEndPoint(in NetEndPoint<TAddress, TUInt> endPoint) => new((IPAddress)endPoint.Address, endPoint.Port);
}