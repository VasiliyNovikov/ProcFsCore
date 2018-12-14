using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class CpuStatisticsTests : ProcFsTestsBase
    {
        [TestMethod]
        public void CpuStatistics_All_Test()
        {
            var stats = ProcFs.Cpu.Statistics().ToList();
            Assert.AreEqual(Environment.ProcessorCount, stats.Count - 1);
            var i = -1;
            foreach (var stat in stats)
            {
                Assert.AreEqual(i, stat.CpuNumber ?? -1);
                Assert.IsTrue(stat.UserTime > 0);
                Assert.IsTrue(stat.NiceTime > 0);
                Assert.IsTrue(stat.KernelTime > 0);
                Assert.IsTrue(stat.IdleTime > 0);
                Assert.IsTrue(stat.SoftIrqTime > 0);
                ++i;
            }
        }
        
        [TestMethod]
        public void CpuStatistics_Adds_Up_Test()
        {
            var sw = new Stopwatch();
            sw.Start();
            var stats = ProcFs.Cpu.Statistics().ToList();
            var cpuTimeError = sw.Elapsed.TotalSeconds * Environment.ProcessorCount + 1.0 / ProcFs.TicksPerSecond;
                
            var wholeStat = stats[0];
            var totalUserTime = 0.0;
            var totalNiceTime = 0.0;
            var totalKernelTime = 0.0;
            var totalIdleTime = 0.0;
            var totalIrqTime = 0.0;
            var totalSoftIrqTime = 0.0;
            foreach (var stat in stats.Skip(1))
            {
                totalUserTime += stat.UserTime;
                totalNiceTime += stat.NiceTime;
                totalKernelTime += stat.KernelTime;
                totalIdleTime += stat.IdleTime;
                totalIrqTime += stat.IrqTime;
                totalSoftIrqTime += stat.SoftIrqTime;
            }
            
            Assert.AreEqual(wholeStat.UserTime, totalUserTime, cpuTimeError);
            Assert.AreEqual(wholeStat.NiceTime, totalNiceTime, cpuTimeError);
            Assert.AreEqual(wholeStat.KernelTime, totalKernelTime, cpuTimeError);
            Assert.AreEqual(wholeStat.IdleTime, totalIdleTime, cpuTimeError);
            Assert.AreEqual(wholeStat.IrqTime, totalIrqTime, cpuTimeError);
            Assert.AreEqual(wholeStat.SoftIrqTime, totalSoftIrqTime, cpuTimeError);
        }
    }
}