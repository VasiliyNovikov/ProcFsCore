using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class NetArpTests : ProcFsTestsBase
    {
        [TestMethod]
        public void NetArpEntry_Get_Test()
        {
            var entries = ProcFs.Default.Net.Arp().ToList();
            Assert.IsTrue(entries.Count > 0);
        }

        [TestMethod]
        public void NetArpEntry_Parse_Test()
        {
            var testProcFs = TestProcFs();
            var entries = testProcFs.Net.Arp().ToList();
            Assert.AreEqual(5, entries.Count);

            static void VerifyEntry(in NetArpEntry entry, string address, string hardwareAddress, string device)
            {
                Assert.AreEqual(IPAddress.Parse(address), entry.Address);
                Assert.AreEqual(hardwareAddress, entry.HardwareAddress.ToString());
                Assert.AreEqual("*", entry.Mask);
                Assert.AreEqual(device, entry.Device);
            }

            VerifyEntry(entries[0], "10.164.164.4", "4c:77:6d:ab:7a:bf", "br1");
            VerifyEntry(entries[1], "10.164.164.14", "a8:0c:0d:1e:56:0b", "br1");
            VerifyEntry(entries[2], "10.10.144.66", "f2:00:00:00:20:00", "br0");
            VerifyEntry(entries[3], "10.164.165.92", "54:e1:ad:30:cb:ad", "br1");
            VerifyEntry(entries[4], "10.10.144.70", "f2:00:00:00:11:00", "br0");
        }
    }
}