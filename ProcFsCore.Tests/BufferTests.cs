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

        [TestMethod]
        public void Buffer_GC_Move_Test()
        {
            Allocate();
            
            var bufferRef = new BufferRef(16);
            using (bufferRef.Value)
            {
                var initialSpan = Fill(bufferRef, out var initialPtr);

                GC.Collect(GC.MaxGeneration);
                GC.Collect(GC.MaxGeneration);
                
                var finalSpan = Fill(bufferRef, out var finalPtr);

                Assert.IsTrue(initialSpan.SequenceEqual(finalSpan));
                Assert.AreEqual(initialPtr, finalPtr);
            }
        }

        private static void Allocate()
        {
            for (var i = 0; i < 64; i++)
                GC.KeepAlive(new byte[64]);
        }

        private unsafe Span<byte> Fill(BufferRef bufferRef, out IntPtr ptr)
        {
            var span = bufferRef.Value.Span;
            _rnd.NextBytes(span);
            fixed (byte* p = &span.GetPinnableReference())
                ptr = (IntPtr) p;
            return span;
        }

        private class BufferRef
        {
            public readonly Buffer Value;

            public BufferRef(int length) => Value = new Buffer(length);
        }

        [TestMethod]
        public void Span_Overlapped_Copy_Forward_Test()
        {
            Span<byte> data = stackalloc byte[0x2000];
            _rnd.NextBytes(data);
            
            Span<byte> span = stackalloc byte[0x3000];
            data.CopyTo(span);
            
            span.Slice(0, 0x2000).CopyTo(span.Slice(0x1000));
            Assert.IsTrue(span.Slice(0x1000).SequenceEqual(data));
        }

        [TestMethod]
        public void Span_Overlapped_Copy_Backwards_Test()
        {
            Span<byte> data = stackalloc byte[0x2000];
            _rnd.NextBytes(data);
            
            Span<byte> span = stackalloc byte[0x3000];
            data.CopyTo(span.Slice(0x1000));
            
            span.Slice(0x1000).CopyTo(span);
            Assert.IsTrue(span.Slice(0, 0x2000).SequenceEqual(data));
        }
    }
}