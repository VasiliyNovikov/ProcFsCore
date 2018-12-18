using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    public class ProcFsTestsBase
    {
        protected static string GetTestProcFsFile(string path) => Path.Combine(Environment.CurrentDirectory, path.Substring(1));

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
}