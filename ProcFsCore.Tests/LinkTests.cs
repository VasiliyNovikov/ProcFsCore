using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests;

[TestClass]
public class LinkTests
{
    [TestMethod]
    public void Test_ReadLink()
    {
        var fileName = $"/proc/{Environment.ProcessId}/stat";
        using (File.OpenRead(fileName))
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, 12345));
            var links = Directory.EnumerateFiles($"/proc/{Environment.ProcessId}/fd")
                                       .Select(Link.Read)
                                       .ToArray();
            Assert.IsTrue(links.Any(l => l.Path == fileName));
            Assert.IsTrue(links.Any(l => l.Type == LinkType.Socket));
        }
    }
}