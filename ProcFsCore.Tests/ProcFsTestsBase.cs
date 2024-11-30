using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests;

public class ProcFsTestsBase
{
    protected static ProcFs TestProcFs() => new(Path.Combine(Environment.CurrentDirectory, "proc"));

    protected static void RetryOnAssert(Action action, int count = 3)
    {
        for (var i = 0; i < count - 1; i++)
        {
            try
            {
                action();
                return;
            }
            catch (AssertFailedException)
            {
            }
        }
            
        action();
    }
}