using System;
using System.Buffers.Text;
using System.Globalization;

namespace ProcFsCore;

public readonly struct Link
{
    public LinkType Type { get; }
    public string? Path { get; }
    // ReSharper disable once InconsistentNaming
    public int INode { get; }

    private Link(LinkType type, string? path, int iNode)
    {
        Type = type;
        Path = path;
        INode = iNode;
    }

    private static readonly ReadOnlyMemory<byte> SocketLinkStart = "socket:[".ToUtf8();
    private static readonly ReadOnlyMemory<byte> PipeLinkStart = "pipe:[".ToUtf8();
    private static readonly ReadOnlyMemory<byte> AnonLinkStart = "anon_inode:[".ToUtf8();
        
    public static Link Read(string linkPath)
    {
        using var linkTextBuffer = Native.ReadLink(linkPath);
        var linkText = linkTextBuffer.Span;
        if (linkText.StartsWith(SocketLinkStart.Span))
        {
#pragma warning disable CA1806
            Utf8Parser.TryParse(linkText.Slice(SocketLinkStart.Length, linkText.Length - SocketLinkStart.Length - 1), out int iNode, out _);
#pragma warning restore CA1806
            return new Link(LinkType.Socket, null, iNode);
        }

        if (linkText.StartsWith(PipeLinkStart.Span))
        {
#pragma warning disable CA1806
            Utf8Parser.TryParse(linkText.Slice(PipeLinkStart.Length, linkText.Length - PipeLinkStart.Length - 1), out int iNode, out _);
#pragma warning restore CA1806
            return new Link(LinkType.Pipe, null, iNode);
        }

        if (linkText.StartsWith(AnonLinkStart.Span))
            return new Link(LinkType.Anon, linkText.Slice(AnonLinkStart.Length, linkText.Length - AnonLinkStart.Length - 1).ToUtf8String(), 0);

        return new Link(LinkType.File, linkText.ToUtf8String(), 0);
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