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
            Assert.IsTrue(stats.Total > 0);
            Assert.IsTrue(stats.Available > 0);
            Assert.IsTrue(stats.Free > 0);
            Assert.IsTrue(stats.Total > stats.Available);
            Assert.IsTrue(stats.Total > stats.Free);
            
            Assert.IsTrue(stats.SwapTotal > 0);
            Assert.IsTrue(stats.SwapFree > 0);
            Assert.IsTrue(stats.SwapTotal > stats.SwapFree);
        }
    }
}