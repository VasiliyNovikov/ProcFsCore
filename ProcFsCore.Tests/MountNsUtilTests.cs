using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests;

[TestClass]
public class MountNsUtilTests
{
    [TestMethod]
    public void MountNsUtil_Scope_Test()
    {
        Assert.AreNotEqual(0, Directory.GetDirectories("/proc").Length);
        MountNsUtil.Scope(ctx =>
        {
            Assert.AreNotEqual(0, Directory.GetDirectories("/proc").Length);
            ctx.MountTemp("/proc");
            Assert.AreEqual(0, Directory.GetDirectories("/proc").Length);
        });
        Assert.AreNotEqual(0, Directory.GetDirectories("/proc").Length);
    }
}