using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests;

[TestClass]
public class NetServicesTests : ProcFsTestsBase
{
    private static void VerifyEndpoint(IPEndPoint expected, IPEndPoint actual)
    {
        Assert.AreEqual(expected.AddressFamily, actual.AddressFamily);
        Assert.AreEqual(expected.Port, actual.Port);
        if (expected.AddressFamily != AddressFamily.InterNetworkV6)
            Assert.AreEqual(expected.Address, actual.Address);
    }

    private static void VerifyState(TcpState expected, NetServiceState actual)
    {
        switch (expected)
        {
            case TcpState.Closed:
                Assert.AreEqual(NetServiceState.Closed, actual);
                break;
            case TcpState.CloseWait:
                Assert.AreEqual(NetServiceState.CloseWait, actual);
                break;
            case TcpState.Closing:
                Assert.AreEqual(NetServiceState.Closing, actual);
                break;
            case TcpState.DeleteTcb:
                Assert.AreEqual(NetServiceState.NewSynReceived, actual);
                break;
            case TcpState.Established:
                Assert.AreEqual(NetServiceState.Established, actual);
                break;
            case TcpState.FinWait1:
                Assert.AreEqual(NetServiceState.FinWait1, actual);
                break;
            case TcpState.FinWait2:
                Assert.AreEqual(NetServiceState.FinWait2, actual);
                break;
            case TcpState.LastAck:
                Assert.AreEqual(NetServiceState.LastAck, actual);
                break;
            case TcpState.Listen:
                Assert.AreEqual(NetServiceState.Listen, actual);
                break;
            case TcpState.SynReceived:
                Assert.AreEqual(NetServiceState.SynReceived, actual);
                break;
            case TcpState.SynSent:
                Assert.AreEqual(NetServiceState.SynSent, actual);
                break;
            case TcpState.TimeWait:
                Assert.AreEqual(NetServiceState.TimeWait, actual);
                break;
            case TcpState.Unknown:
                Assert.AreEqual(NetServiceState.Unknown, actual);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(expected), expected, null);
        }
    }
        
    [TestMethod]
    public void NetServices_Tcp_Connections_Test()
    {
        RetryOnAssert(() =>
        {
            var services = ProcFs.Default.Net.Services.Tcp(NetAddressVersion.IPv4)
                                                      .Concat(ProcFs.Default.Net.Services.Tcp(NetAddressVersion.IPv6))
                                                      .Where(s => s.State != NetServiceState.Listen)
                                                      .ToArray();
            var expectedServices = IPGlobalProperties.GetIPGlobalProperties()
                                                     .GetActiveTcpConnections()
                                                     .ToArray();
            Assert.AreEqual(expectedServices.Length, services.Length);
            for (var i = 0; i < services.Length; ++i)
            {
                var service = services[i];
                var expectedService = expectedServices[i];
                VerifyState(expectedService.State, service.State);
                VerifyEndpoint(expectedService.LocalEndPoint, service.LocalEndPoint);
                VerifyEndpoint(expectedService.RemoteEndPoint, service.RemoteEndPoint);
            }
        });
    }

    [TestMethod]
    public void NetServices_Tcp_Listeners_Test()
    {
        RetryOnAssert(() =>
        {
            var services = ProcFs.Default.Net.Services.Tcp(NetAddressVersion.IPv4)
                                                      .Concat(ProcFs.Default.Net.Services.Tcp(NetAddressVersion.IPv6))
                                                      .Where(s => s.State == NetServiceState.Listen)
                                                      .ToArray();
            var expectedServices = IPGlobalProperties.GetIPGlobalProperties()
                                                     .GetActiveTcpListeners();
            Assert.AreEqual(expectedServices.Length, services.Length);
            for (var i = 0; i < services.Length; ++i)
            {
                var service = services[i];
                var expectedService = expectedServices[i];
                VerifyEndpoint(expectedService, service.LocalEndPoint);
            }
        });
    }
        
    [TestMethod]
    public void NetServices_Udp_Test()
    {
        RetryOnAssert(() =>
        {
            var services = ProcFs.Default.Net.Services.Udp(NetAddressVersion.IPv4).Concat(ProcFs.Default.Net.Services.Udp(NetAddressVersion.IPv6)).ToArray();
            var expectedEndpoints = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            Assert.AreEqual(expectedEndpoints.Length, services.Length);
            for (var i = 0; i < services.Length; ++i)
            {
                var service = services[i];
                var expectedEndpoint = expectedEndpoints[i];
                VerifyEndpoint(expectedEndpoint, service.LocalEndPoint);
            }
        });
    }

    [TestMethod]
    public void NetServices_Unix_Test()
    {
        var services = ProcFs.Default.Net.Services.Unix().ToArray();
        foreach (var service in services)
            if (service.Path != null)
                Assert.IsTrue(service.Path.Length > 0);
    }

    [TestMethod]
    public void NetServices_Raw_Test()
    {
        var services = ProcFs.Default.Net.Services.Raw(NetAddressVersion.IPv4).Concat(ProcFs.Default.Net.Services.Raw(NetAddressVersion.IPv6)).ToArray();
        foreach (var service in services)
            Assert.IsTrue(!service.LocalEndPoint.IsEmpty || !service.RemoteEndPoint.IsEmpty);
    }
}