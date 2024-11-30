using System;
using System.IO;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace ProcFsCore.Benchmarks;

[SkipLocalsInit]
public class FileStreamBenchmarks
{
    private const string Path = "/proc/stat";
    private const int BufferSize = 256;
    private const int NumberOfReads = 4;
        
    [Benchmark]
    public int FileStream_Read_Proc_Stat()
    {
        var size = 0;
        Span<byte> buffer = stackalloc byte[BufferSize];
        using var file = File.OpenRead(Path);
        for (var i = 0; i < NumberOfReads; ++i)
            size += file.Read(buffer);
        return size;
    }
        
    [Benchmark]
    public int LightFileStream_Read_Proc_Stat()
    {
        var size = 0;
        Span<byte> buffer = stackalloc byte[BufferSize];
        using var file = LightFileStream.OpenRead(Path);
        for (var i = 0; i < NumberOfReads; ++i)
            size += file.Read(buffer);
        return size;
    }
}