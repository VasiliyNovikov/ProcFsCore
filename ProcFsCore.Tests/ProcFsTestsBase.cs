using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    public class ProcFsTestsBase
    {
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