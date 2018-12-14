using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class MemoryStatisticsTests : ProcFsTestsBase
    {
        [TestMethod]
        public void MemoryStatistics_Test()
        {
            var stats = ProcFs.Memory.Statistics();
            Assert.IsTrue(stats.Total > 0, "Total > 0");
            Assert.IsTrue(stats.Available > 0, "Available > 0");
            Assert.IsTrue(stats.Free > 0, "Free > 0");
            Assert.IsTrue(stats.Total > stats.Available, "Total > Available");
            Assert.IsTrue(stats.Total > stats.Free, "Total > Free");
        }
    }
}