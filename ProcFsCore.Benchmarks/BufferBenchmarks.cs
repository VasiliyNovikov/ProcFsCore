using System;
using System.Buffers;
using BenchmarkDotNet.Attributes;

namespace ProcFsCore.Benchmarks
{
    public class BufferBenchmarks
    {
        private static void Stack_Benchmark(int size)
        {
            Span<byte> buffer = stackalloc byte[size];
            buffer[0] = 1;
        }

        private static void ArrayPool_Benchmark(int size)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            Span<byte> span = buffer;
            span[0] = 1;
            ArrayPool<byte>.Shared.Return(buffer);
        }

        [Benchmark]
        public void Stack_256() => Stack_Benchmark(256);

        [Benchmark]
        public void ArrayPool_256() => ArrayPool_Benchmark(256);

        [Benchmark]
        public void Stack_512() => Stack_Benchmark(512);

        [Benchmark]
        public void ArrayPool_512() => ArrayPool_Benchmark(512);

        [Benchmark]
        public void Stack_1024() => Stack_Benchmark(1024);

        [Benchmark]
        public void ArrayPool_1024() => ArrayPool_Benchmark(1024);

        [Benchmark]
        public void Stack_2048() => Stack_Benchmark(2048);

        [Benchmark]
        public void ArrayPool_2048() => ArrayPool_Benchmark(2048);

        [Benchmark]
        public void Stack_4096() => Stack_Benchmark(4096);

        [Benchmark]
        public void ArrayPool_4096() => ArrayPool_Benchmark(4096);
    }
}