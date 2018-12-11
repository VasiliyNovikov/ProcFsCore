using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class BufferTests
    {
        private readonly Random _rnd = new Random();
        
        [TestMethod]
        public void Buffer_Low_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[32];
            _rnd.NextBytes(data);
            using (var buffer = new Buffer(data.Length))
            {
                data.CopyTo(buffer.Span);
                Assert.IsTrue(data.SequenceEqual(buffer.Span));
            }
        }
        
        [TestMethod]
        public void Buffer_High_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[Buffer.MinimumCapacity * 2];
            _rnd.NextBytes(data);
            using (var buffer = new Buffer(data.Length))
            {
                data.CopyTo(buffer.Span);
                Assert.IsTrue(data.SequenceEqual(buffer.Span));
            }
        }
        
        [TestMethod]
        public void Buffer_Reallocate_Low_Low_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[16];
            _rnd.NextBytes(data);
            using (var buffer = new Buffer(data.Length))
            {
                data.CopyTo(buffer.Span);
                buffer.Resize(data.Length * 4);
                Assert.IsTrue(data.SequenceEqual(buffer.Span.Slice(0, data.Length)));
            }
        }
        
        [TestMethod]
        public void Buffer_Resize_High_High_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[Buffer.MinimumCapacity * 2];
            _rnd.NextBytes(data);
            using (var buffer = new Buffer(data.Length))
            {
                data.CopyTo(buffer.Span);
                buffer.Resize(data.Length * 8);
                Assert.IsTrue(data.SequenceEqual(buffer.Span.Slice(0, data.Length)));
            }
        }

        [TestMethod]
        public void Buffer_Resize_Low_High_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[16];
            _rnd.NextBytes(data);
            using (var buffer = new Buffer(data.Length))
            {
                data.CopyTo(buffer.Span);
                buffer.Resize(Buffer.MinimumCapacity * 2);
                Assert.IsTrue(data.SequenceEqual(buffer.Span.Slice(0, data.Length)));
            }
        }
        
        [TestMethod]
        public void Buffer_Resize_High_Low_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[Buffer.MinimumCapacity * 2];
            _rnd.NextBytes(data);
            using (var buffer = new Buffer(data.Length))
            {
                data.CopyTo(buffer.Span);
                buffer.Resize(16);
                Assert.IsTrue(data.Slice(0, buffer.Span.Length).SequenceEqual(buffer.Span));
            }
        }
    }
}