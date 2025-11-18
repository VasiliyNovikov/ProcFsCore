using System;
using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
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
        var addressHex = readerRef.ReadWord(AddressPortSeparator);
        Span<byte> addressBytes = stackalloc byte[addressHex.Length / 2];
        if (Convert.FromHexString(addressHex, addressBytes, out _, out _) == OperationStatus.Done)
        {
            addressBytes.Reverse();
            return new NetEndPoint<TAddress>(MemoryMarshal.Read<TAddress>(addressBytes), readerRef.ReadInt32('x'));

        }
        throw new FormatException($"Invalid address format: {Encoding.ASCII.GetString(addressHex)}");
    }

    public override string? ToString() => ((IPEndPoint?)this)?.ToString();

    public static implicit operator IPEndPoint(in NetEndPoint<TAddress> endPoint) => new((IPAddress)endPoint.Address, endPoint.Port);
}