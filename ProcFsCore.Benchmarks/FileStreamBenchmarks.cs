using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace ProcFsCore.Benchmarks
{
    public class FileStreamBenchmarks
    {
        private const string Path = "/proc/stat";
        private const int BufferSize = 256;
        private const int NumberOfReads = 4;
        
        [Benchmark]
        public void FileStream_Read_Proc_Stat()
        {
            Span<byte> buffer = stackalloc byte[BufferSize];
            using (var file = File.OpenRead(Path))
                for (var i = 0; i < NumberOfReads; ++i)
                    file.Read(buffer);
        }
        
        [Benchmark]
        public void LightFileStream_Read_Proc_Stat()
        {
            Span<byte> buffer = stackalloc byte[BufferSize];
            using (var file = LightFileStream.OpenRead(Path))
                for (var i = 0; i < NumberOfReads; ++i)
                    file.Read(buffer);
        }
    }
}