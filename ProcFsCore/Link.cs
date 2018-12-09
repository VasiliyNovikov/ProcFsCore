using System;

namespace ProcFsCore
{
    public struct Link
    {
        public LinkType Type { get; }
        public string Path { get; }
        public int INode { get; }

        private Link(LinkType type, string path, int iNode)
        {
            Type = type;
            Path = path;
            INode = iNode;
        }

        public static Link Read(string linkPath)
        {
            var linkText = Native.ReadLink(linkPath);
            const string socketLinkStart = "socket:[";
            const string pipeLinkStart = "pipe:[";
            const string anonLinkStart = "anon_inode:[";
            if (linkText.StartsWith(socketLinkStart))
                return new Link(LinkType.Socket, null, Int32.Parse(linkText.AsSpan(socketLinkStart.Length, linkText.Length - socketLinkStart.Length - 1)));
            if (linkText.StartsWith(pipeLinkStart))
                return new Link(LinkType.Pipe, null, Int32.Parse(linkText.AsSpan(pipeLinkStart.Length, linkText.Length - pipeLinkStart.Length - 1)));
            if (linkText.StartsWith(anonLinkStart))
                return new Link(LinkType.Anon, linkText.Substring(anonLinkStart.Length, linkText.Length - anonLinkStart.Length - 1), 0);
            return new Link(LinkType.File, linkText, 0);
        }

        public override string ToString() => $"{Type}:[{Path ?? INode.ToString()}]";
    }

    public enum LinkType
    {
        File,
        Socket,
        Pipe,
        Anon
    }
}