using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests;

[TestClass]
public class NetStatisticsTests : ProcFsTestsBase
{
    private static void NetStatistics_All_Test(Func<IEnumerable<NetStatistics>> getStats)
    {
        RetryOnAssert(() =>
        {
            var stats = getStats().ToDictionary(stat => stat.InterfaceName);
            var expectedStats = NetworkInterface.GetAllNetworkInterfaces()
                                                .ToDictionary(iface => iface.Name, iface => iface.GetIPStatistics());
            foreach (var (name, expectedStat) in expectedStats)
            {
                var actualStat = stats[name];

                Assert.AreEqual(expectedStat.UnicastPacketsReceived, actualStat.Receive.Packets, 100);
                Assert.AreEqual(expectedStat.BytesReceived, actualStat.Receive.Bytes, 100000);
                Assert.AreEqual(expectedStat.IncomingPacketsDiscarded, actualStat.Receive.Drops);
                Assert.AreEqual(expectedStat.IncomingPacketsWithErrors, actualStat.Receive.Errors);
                    
                Assert.AreEqual(expectedStat.UnicastPacketsSent, actualStat.Transmit.Packets, 100);
                Assert.AreEqual(expectedStat.BytesSent, actualStat.Transmit.Bytes, 100000);
                Assert.AreEqual(expectedStat.OutgoingPacketsDiscarded, actualStat.Transmit.Drops);
                Assert.AreEqual(expectedStat.OutgoingPacketsWithErrors, actualStat.Transmit.Errors);
            }
        }, 10);
    }

    [TestMethod]
    public void NetStatistics_All_Test() => NetStatistics_All_Test(ProcFs.Default.Net.Statistics);

    [TestMethod]
    public void NetStatistics_All_For_Process_Test() => NetStatistics_All_Test(() => ProcFs.Default.CurrentProcess.Net.Statistics);
}