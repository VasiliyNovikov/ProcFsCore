using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests;

[TestClass]
public class BootTimeTests : ProcFsTestsBase
{
    [TestMethod]
    public void BootTime_Get_Test()
    {
        var bootTime = ProcFs.Default.BootTimeUtc;
        Assert.IsTrue(bootTime > new DateTime(2020, 1, 1));
        Assert.IsTrue(bootTime < DateTime.UtcNow);
    }

    [TestMethod]
    public void BootTime_Parse_Test()
    {
        var testProcFs = TestProcFs();
        var bootTime = testProcFs.BootTimeUtc;
        Assert.AreEqual(1699882628, (int)(bootTime - DateTime.UnixEpoch).TotalSeconds);
    }
}