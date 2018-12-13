using BenchmarkDotNet.Attributes;

namespace ProcFsCore.Benchmarks
{
    public class ProcessByIdBenchmarks : BaseBenchmarks
    {
        [Benchmark]
        public void Framework_GetProcessById()
        {
            var process = System.Diagnostics.Process.GetProcessById(1);
            Use(process.Id);
            Use(process.ProcessName);
            Use(process.StartTime);
            Use(process.VirtualMemorySize64);
            Use(process.WorkingSet64);
            Use(process.UserProcessorTime);
            Use(process.PrivilegedProcessorTime);
        }
        
        [Benchmark]
        public void ProcFs_GetProcessById()
        {
            var process = new Process(1);
            process.Refresh();
            Use(process.Pid);
            Use(process.Name);
            Use(process.StartTimeUtc);
            Use(process.VirtualMemorySize);
            Use(process.ResidentSetSize);
            Use(process.UserProcessorTime);
            Use(process.KernelProcessorTime);
        }
    }
}