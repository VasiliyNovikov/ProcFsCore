using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using NetworkingPrimitivesCore;

namespace ProcFsCore;

public interface INetService
{
    NetServiceType Type { get; }
    NetServiceState State { get; }
    int INode { get; }
}

public readonly struct UnixService : INetService
{
    public NetServiceType Type => NetServiceType.Unix;
    public string? Path { get; }
    public NetServiceState State { get; }
    public int INode { get; }

    private UnixService(string? path, NetServiceState state, int iNode)
    {
        Path = path;
        State = state;
        INode = iNode;
    }

    public override string ToString() => $"{Type} {Path} {State}/{(int)State} {INode}";

    internal static IEnumerable<UnixService> GetAll(string netPath)
    {
        using var statReader = new AsciiFileReader(System.IO.Path.Combine(netPath, "unix"), 256);
        statReader.SkipLine();
        while (!statReader.EndOfStream)
        {
            statReader.SkipWhiteSpaces();
            statReader.SkipWord();
            statReader.SkipWord();
            statReader.SkipWord();
            statReader.SkipWord();
            statReader.SkipWord();
            var state = (NetServiceState)statReader.ReadInt16('x');
            var iNode = statReader.ReadInt32();
            var path = statReader.EndOfStream ? null : statReader.ReadStringWord();
            yield return new UnixService(path, state, iNode);
            statReader.SkipLine();
        }
    }
}

public readonly struct NetService<TAddress, TUInt> : INetService
    where TAddress : unmanaged, IIPAddress<TAddress, TUInt>
    where TUInt : unmanaged, IBinaryInteger<TUInt>, IUnsignedNumber<TUInt>
{
    public NetServiceType Type { get; }
    public NetEndPoint<TAddress, TUInt> LocalEndPoint { get; }
    public NetEndPoint<TAddress, TUInt> RemoteEndPoint { get; }
    public NetServiceState State { get; }
    public int INode { get; }

    private NetService(NetServiceType type, in NetEndPoint<TAddress, TUInt> localEndPoint, in NetEndPoint<TAddress, TUInt> remoteEndPoint, NetServiceState state, int iNode)
    {
        Type = type;
        LocalEndPoint = localEndPoint;
        RemoteEndPoint = remoteEndPoint;
        State = state;
        INode = iNode;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Type);
        if (!LocalEndPoint.IsEmpty)
        {
            builder.Append(' ');
            builder.Append(LocalEndPoint);
        }

        if (!RemoteEndPoint.IsEmpty)
        {
            builder.Append(' ');
            builder.Append(RemoteEndPoint);
        }
        builder.Append(CultureInfo.InvariantCulture, $" {State}/{(int)State} {INode}");
        return builder.ToString();
    }

    private static readonly string[,] NetServiceFiles = 
    {
        { "tcp", "tcp6" },
        { "udp", "udp6" },
        { "raw", "raw6" }
    };

    private static IEnumerable<NetService<TAddress, TUInt>> GetAll(string netPath, NetServiceType type)
    {
        var serviceFile = NetServiceFiles[(int) type, TAddress.Version == IPv4.Version ? 0 : 1];
        using var statReader = new AsciiFileReader(System.IO.Path.Combine(netPath, serviceFile), 256);
        statReader.SkipLine();
        while (!statReader.EndOfStream)
        {
            statReader.SkipWhiteSpaces();
            statReader.SkipWord();
            var localEndPoint = NetEndPoint<TAddress, TUInt>.Read(statReader);
            var remoteEndPoint = NetEndPoint<TAddress, TUInt>.Read(statReader);
            var state = (NetServiceState)statReader.ReadInt16('x');

            statReader.SkipWord();
            statReader.SkipWord();
            statReader.SkipWord();
            statReader.SkipWord();
            statReader.SkipWord();

            var iNode = statReader.ReadInt32();

            yield return new NetService<TAddress, TUInt>(type, localEndPoint, remoteEndPoint, state, iNode);
            statReader.SkipLine();
        }
    }

    internal static IEnumerable<NetService<TAddress, TUInt>> GetTcp(string netPath) => GetAll(netPath, NetServiceType.Tcp);

    internal static IEnumerable<NetService<TAddress, TUInt>> GetUdp(string netPath) => GetAll(netPath, NetServiceType.Udp);

    internal static IEnumerable<NetService<TAddress, TUInt>> GetRaw(string netPath) => GetAll(netPath, NetServiceType.Raw);
}