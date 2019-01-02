using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class StructUsingSemanticsTests
    {
        [TestMethod]
        public void StructUsingSemantics_InPlace_NoClosure_Test()
        {
            var verifier = new Verifier();
            using (var test = new Test(verifier.Verify))
            {
                test.Update();
                verifier.ExpectedValue = test.Value;
            }
        }

        [TestMethod]
        public void StructUsingSemantics_InPlace_Closure_Test()
        {
            var verifier = new Verifier();
            using (var test = new Test(val => verifier.Verify(val)))
            {
                test.Update();
                verifier.ExpectedValue = test.Value;
            }
        }
        
        [TestMethod]
        public void StructUsingSemantics_Separate_NoClosure_Test()
        {
            var verifier = new Verifier(false);
            var test = new Test(verifier.Verify);
            using (test)
            {
                test.Update();
                verifier.ExpectedValue = test.Value;
            }
        }

        [TestMethod]
        public void StructUsingSemantics_Separate_Closure_Test()
        {
            var verifier = new Verifier(false);
            var test = new Test(val => verifier.Verify(val));
            using (test)
            {
                test.Update();
                verifier.ExpectedValue = test.Value;
            }
        }
        
        [TestMethod]
        public void StructUsingSemantics_Iterator_InPlace_NoClosure_Test()
        {
            IEnumerable<bool> Iter()
            {
                var verifier = new Verifier();
                using (var test = new Test(verifier.Verify))
                {
                    yield return false;
                    test.Update();
                    verifier.ExpectedValue = test.Value;
                }
                yield return true;
            }

            foreach (var _ in Iter())
            {
            }
        }

        [TestMethod]
        public void StructUsingSemantics_Iterator_InPlace_Closure_Test()
        {
            IEnumerable<bool> Iter()
            {
                var verifier = new Verifier();
                using (var test = new Test(val => verifier.Verify(val)))
                {
                    yield return false;
                    test.Update();
                    verifier.ExpectedValue = test.Value;
                }
                yield return true;
            }

            foreach (var _ in Iter())
            {
            }
        }
        
        [TestMethod]
        public void StructUsingSemantics_Iterator_Separate_NoClosure_Test()
        {
            IEnumerable<bool> Iter()
            {
                var verifier = new Verifier(false);
                var test = new Test(verifier.Verify);
                using (test)
                {
                    yield return false;
                    test.Update();
                    verifier.ExpectedValue = test.Value;
                }
                yield return true;
            }

            foreach (var _ in Iter())
            {
            }
        }

        [TestMethod]
        public void StructUsingSemantics_Iterator_Separate_Closure_Test()
        {
            IEnumerable<bool> Iter()
            {
                var verifier = new Verifier(false);
                var test = new Test(val => verifier.Verify(val));
                using (test)
                {
                    yield return false;
                    test.Update();
                    verifier.ExpectedValue = test.Value;
                }
                yield return true;
            }

            foreach (var _ in Iter())
            {
            }
        }

        private class Verifier
        {
            private readonly bool _areEqual;
            public Guid ExpectedValue;
            
            public Verifier(bool areEqual = true)
            {
                _areEqual = areEqual;
            }

            public void Verify(Guid value)
            {
                if (_areEqual)
                    Assert.AreEqual(ExpectedValue, value);
                else
                    Assert.AreNotEqual(ExpectedValue, value);
            }
        }
        
        
        private struct Test : IDisposable
        {
            private readonly Action<Guid> _reportValueOnDispose;

            public Guid Value { get; private set; }
            
            public Test(Action<Guid> reportValueOnDispose)
                : this()
            {
                _reportValueOnDispose = reportValueOnDispose;
                Update();
            }

            public void Update()
            {
                Value = Guid.NewGuid();
            }

            public void Dispose()
            {
                _reportValueOnDispose(Value);
            }
        }
    }
}