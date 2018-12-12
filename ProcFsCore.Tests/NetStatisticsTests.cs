using System;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class NetStatisticsTests : ProcFsTestsBase
    {
        [TestMethod]
        public void NetStatistics_All_Test()
        {
            RetryOnAssert(() =>
            {

                var stats = ProcFs.Net.Statistics().ToDictionary(stat => stat.InterfaceName);
                var expectedStats = NetworkInterface.GetAllNetworkInterfaces()
                    .ToDictionary(iface => iface.Name, iface => iface.GetIPStatistics());
                foreach (var ifaceAndStat in expectedStats)
                {
                    var expectedStat = ifaceAndStat.Value;
                    var actualStat = stats[ifaceAndStat.Key];

                    long AdjustStat(long value) => Math.Min(UInt32.MaxValue, value);

                    Assert.AreEqual(expectedStat.BytesReceived, AdjustStat(actualStat.Receive.Bytes), 100000);
                    Assert.AreEqual(expectedStat.UnicastPacketsReceived, actualStat.Receive.Packets, 100);
                    Assert.AreEqual(expectedStat.IncomingPacketsDiscarded, actualStat.Receive.Drops);
                    Assert.AreEqual(expectedStat.IncomingPacketsWithErrors, actualStat.Receive.Errors);
                    
                    Assert.AreEqual(expectedStat.BytesSent, AdjustStat(actualStat.Transmit.Bytes), 100000);
                    Assert.AreEqual(expectedStat.UnicastPacketsSent, actualStat.Transmit.Packets, 100);
                    Assert.AreEqual(expectedStat.OutgoingPacketsDiscarded, actualStat.Transmit.Drops);
                    Assert.AreEqual(expectedStat.OutgoingPacketsWithErrors, actualStat.Transmit.Errors);
                }
            }, 10);
        }
    }
}