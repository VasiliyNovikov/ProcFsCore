using System;
using System.Buffers;
using System.Globalization;

namespace ProcFsCore;

public readonly struct Link
{
    public LinkType Type { get; }
    public string? Path { get; }
    public int INode { get; }

    private Link(LinkType type, string? path, int iNode)
    {
        Type = type;
        Path = path;
        INode = iNode;
    }

    private static ReadOnlySpan<byte> SocketLinkStart => "socket:["u8;
    private static ReadOnlySpan<byte> PipeLinkStart => "pipe:["u8;
    private static ReadOnlySpan<byte> AnonLinkStart => "anon_inode:["u8;
        
    public static Link Read(string linkPath)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(256);
        try
        {
            int linkLength;
            while (!Native.ReadLink(linkPath, buffer, out linkLength))
            {
                var newBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length + 1);
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = newBuffer;
            }
            return Parse(buffer.AsSpan(0, linkLength));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static Link Parse(Span<byte> linkText)
    {
        if (linkText.StartsWith(SocketLinkStart))
        {
            var iNode = AsciiParser.Parse<int>(linkText.Slice(SocketLinkStart.Length, linkText.Length - SocketLinkStart.Length - 1));
            return new Link(LinkType.Socket, null, iNode);
        }

        if (linkText.StartsWith(PipeLinkStart))
        {
            var iNode = AsciiParser.Parse<int>(linkText.Slice(PipeLinkStart.Length, linkText.Length - PipeLinkStart.Length - 1));
            return new Link(LinkType.Pipe, null, iNode);
        }

        if (linkText.StartsWith(AnonLinkStart))
            return new Link(LinkType.Anon, linkText.Slice(AnonLinkStart.Length, linkText.Length - AnonLinkStart.Length - 1).ToAsciiString(), 0);

        return new Link(LinkType.File, linkText.ToAsciiString(), 0);
    }

    public override string ToString() => $"{Type}:[{Path ?? INode.ToString(CultureInfo.InvariantCulture)}]";
}

public enum LinkType
{
    File,
    Socket,
    Pipe,
    Anon
}