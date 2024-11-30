using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests;

[TestClass]
public class NativeTests
{
    [TestMethod]
    public void Test_ReadLink()
    {
        var fileName = $"/proc/{Native.GetPid()}/stat";
        using (File.OpenRead(fileName))
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, 12345));
            var links = Directory.EnumerateFiles($"/proc/{Native.GetPid()}/fd")
                .Select(l =>
                {
                    using var linkBuffer = Native.ReadLink(l);
                    return linkBuffer.Span.ToUtf8String();
                })
                .ToHashSet();
            Assert.IsTrue(links.Contains(fileName));
            Assert.IsTrue(links.Any(l => l.StartsWith("socket:[", StringComparison.Ordinal)));
        }
    }
}