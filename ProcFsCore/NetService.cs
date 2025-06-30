using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ProcFsCore;

public readonly struct NetService
{
    public NetServiceType Type { get; }
    public NetEndPoint LocalEndPoint { get; }
    public NetEndPoint RemoteEndPoint { get; }
    public string? Path { get; }
    public NetServiceState State { get; }
    public int INode { get; }

    private NetService(NetServiceType type, in NetEndPoint localEndPoint, in NetEndPoint remoteEndPoint, string? path, NetServiceState state, int iNode)
    {
        Type = type;
        LocalEndPoint = localEndPoint;
        RemoteEndPoint = remoteEndPoint;
        Path = path;
        State = state;
        INode = iNode;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Type);
        if (Type == NetServiceType.Unix)
        {
            builder.Append(' ');
            builder.Append(Path);
        }
        else
        {
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
        }
        builder.Append(CultureInfo.InvariantCulture, $" {State}/{(int)State} {INode}");
        return builder.ToString();
    }

    private static readonly string[,] NetServiceFiles = 
    {
        { "tcp", "tcp6" },
        { "udp", "udp6" },
        { "raw", "raw6" },
        { "unix", null! }
    };

    private static IEnumerable<NetService> GetAll(string netPath, NetServiceType type, NetAddressVersion? addressVersion)
    {
        var serviceFile = NetServiceFiles[(int) type, (addressVersion ?? NetAddressVersion.IPv4) == NetAddressVersion.IPv4 ? 0 : 1];
        using var statReader = new AsciiFileReader(System.IO.Path.Combine(netPath, serviceFile), 256);
        statReader.SkipLine();
        while (!statReader.EndOfStream)
        {
            statReader.SkipWhiteSpaces();
            statReader.SkipWord();
            if (type != NetServiceType.Unix)
            {
                var localEndPoint = NetEndPoint.Read(statReader);
                var remoteEndPoint = NetEndPoint.Read(statReader);
                var state = (NetServiceState)statReader.ReadInt16('x');
                        
                statReader.SkipWord();
                statReader.SkipWord();
                statReader.SkipWord();
                statReader.SkipWord();
                statReader.SkipWord();

                var iNode = statReader.ReadInt32();
                        
                yield return new NetService(type, localEndPoint, remoteEndPoint, null, state, iNode);
            }
            else
            {
                statReader.SkipWord();
                statReader.SkipWord();
                statReader.SkipWord();
                statReader.SkipWord();
                var state = (NetServiceState)statReader.ReadInt16('x');
                var iNode = statReader.ReadInt32();
                var path = statReader.EndOfStream ? null : statReader.ReadStringWord();

                yield return new NetService(type, default, default, path, state, iNode);
            }
            statReader.SkipLine();
        }
    }

    internal static IEnumerable<NetService> GetTcp(string netPath, NetAddressVersion addressVersion) => GetAll(netPath, NetServiceType.Tcp, addressVersion);

    internal static IEnumerable<NetService> GetUdp(string netPath, NetAddressVersion addressVersion) => GetAll(netPath, NetServiceType.Udp, addressVersion);

    internal static IEnumerable<NetService> GetRaw(string netPath, NetAddressVersion addressVersion) => GetAll(netPath, NetServiceType.Raw, addressVersion);

    internal static IEnumerable<NetService> GetUnix(string netPath) => GetAll(netPath, NetServiceType.Unix, null);
}