using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class DiskStatisticsTests : ProcFsTestsBase
    {
        [TestMethod]
        public void DiskStatistics_All_Test()
        {
            var hasTotalTime = false;
            foreach (var stat in ProcFs.Disk.Statistics())
            {
                Assert.IsNotNull(stat.DeviceName);
                
                void Verify(in DiskStatistics.Operation op)
                {
                    Assert.IsTrue(op.Count > 0, $"{stat.DeviceName} Count > 0");
                    Assert.IsTrue(op.Bytes > 0, $"{stat.DeviceName} Bytes > 0");
                    Assert.IsTrue(op.Time > 0, $"{stat.DeviceName} Time > 0");
                }
                
                Verify(stat.Reads);

                if (stat.TotalTime > 0)
                {
                    hasTotalTime = true;
                    //Assert.IsTrue(stat.TotalWeightedTime >= stat.TotalTime, $"{stat.DeviceName} TotalWeightedTime >= TotalTime");
                    Assert.IsTrue(stat.TotalWeightedTime > 0, $"{stat.DeviceName} TotalWeightedTime > 0");
                    Assert.AreEqual(stat.Reads.Time + stat.Writes.Time, stat.TotalWeightedTime, 0.5, $"{stat.DeviceName} Reads.Time + Writes.Time ≈ TotalWeightedTime");
                }
            }

            Assert.IsTrue(hasTotalTime, "TotalTime > 0 for at least one device");
        }
    }
}