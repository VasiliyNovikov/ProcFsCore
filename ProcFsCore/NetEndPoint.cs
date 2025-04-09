using System.Buffers;
using System.Net;

namespace ProcFsCore;

public readonly struct NetEndPoint
{
    private static readonly SearchValues<byte> AddressPortSeparator = SearchValues.Create(":"u8);

    public NetAddress Address { get; }
    public int Port { get; }
    public bool IsEmpty => Address.IsEmpty && Port == 0;

    private NetEndPoint(in NetAddress address, int port)
    {
        Address = address;
        Port = port;
    }

    internal static NetEndPoint Read(ref AsciiFileReader statReader)
    {
        return new NetEndPoint(NetAddress.Parse(statReader.ReadWord(AddressPortSeparator), NetAddressFormat.Hex), statReader.ReadInt32('x'));
    }

    public override string? ToString() => ((IPEndPoint?)this)?.ToString();

    public static implicit operator IPEndPoint(in NetEndPoint endPoint) => new(endPoint.Address, endPoint.Port);
}