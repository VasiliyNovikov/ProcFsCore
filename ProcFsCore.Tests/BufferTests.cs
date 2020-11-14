using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProcFsCore.Tests
{
    [TestClass]
    public class BufferTests
    {
        private readonly Random _rnd = new Random();

        [TestMethod]
        public void Buffer_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[32];
            _rnd.NextBytes(data);
            using var buffer = new Buffer<byte>(data.Length, 64);
            data.CopyTo(buffer.Span);
            Assert.IsTrue(data.SequenceEqual(buffer.Span));
        }

        [TestMethod]
        public void Buffer_No_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[128];
            _rnd.NextBytes(data);
            using var buffer = new Buffer<byte>(data.Length);
            data.CopyTo(buffer.Span);
            Assert.IsTrue(data.SequenceEqual(buffer.Span));
        }

        [TestMethod]
        public void Buffer_Resize_Low_Low_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[16];
            _rnd.NextBytes(data);
            using var buffer = new Buffer<byte>(data.Length, 512);
            data.CopyTo(buffer.Span);
            buffer.Resize(data.Length * 4);
            Assert.IsTrue(data.SequenceEqual(buffer.Span.Slice(0, data.Length)));
        }

        [TestMethod]
        public void Buffer_Resize_High_High_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[128];
            _rnd.NextBytes(data);
            using var buffer = new Buffer<byte>(data.Length, 64);
            data.CopyTo(buffer.Span);
            buffer.Resize(data.Length * 8);
            Assert.IsTrue(data.SequenceEqual(buffer.Span.Slice(0, data.Length)));
        }

        [TestMethod]
        public void Buffer_Resize_Low_High_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[16];
            _rnd.NextBytes(data);
            using var buffer = new Buffer<byte>(data.Length, 64);
            data.CopyTo(buffer.Span);
            buffer.Resize(128);
            Assert.IsTrue(data.SequenceEqual(buffer.Span.Slice(0, data.Length)));
        }

        [TestMethod]
        public void Buffer_Resize_High_Low_Capacity_Test()
        {
            Span<byte> data = stackalloc byte[128];
            _rnd.NextBytes(data);
            using var buffer = new Buffer<byte>(data.Length, 64);
            data.CopyTo(buffer.Span);
            buffer.Resize(16);
            Assert.IsTrue(data.Slice(0, buffer.Span.Length).SequenceEqual(buffer.Span));
        }
    }
}