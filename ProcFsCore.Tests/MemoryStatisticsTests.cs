using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests;

[TestClass]
public class MemoryStatisticsTests : ProcFsTestsBase
{
    [TestMethod]
    public void MemoryStatistics_Test()
    {
        var stats = ProcFs.Default.Memory.Statistics();
        Assert.IsGreaterThan(0, stats.Total);
        Assert.IsGreaterThan(0, stats.Available);
        Assert.IsGreaterThan(0, stats.Free);
        Assert.IsGreaterThan(stats.Available, stats.Total);
        Assert.IsGreaterThan(stats.Free, stats.Total);
    }
}