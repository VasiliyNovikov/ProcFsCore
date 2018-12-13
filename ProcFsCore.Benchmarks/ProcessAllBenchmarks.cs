using BenchmarkDotNet.Attributes;
using DiagnosticsProcess = System.Diagnostics.Process;

namespace ProcFsCore.Benchmarks
{
    public class ProcessAllBenchmarks : BaseBenchmarks
    {
        [Benchmark]
        public void Framework_GetAllProcesses()
        {
            foreach (var process in DiagnosticsProcess.GetProcesses())
            {
                try
                {
                    Use(process.Id);
                    Use(process.ProcessName);
                    Use(process.StartTime);
                    Use(process.VirtualMemorySize64);
                    Use(process.WorkingSet64);
                    Use(process.UserProcessorTime);
                    Use(process.PrivilegedProcessorTime);
                }
                catch
                {
                }
            }
        }
        
        [Benchmark]
        public void ProcFs_GetAllProcesses()
        {
            foreach (var process in ProcFs.Processes())
            {
                try
                {
                    Use(process.Pid);
                    Use(process.Name);
                    Use(process.StartTimeUtc);
                    Use(process.VirtualMemorySize);
                    Use(process.ResidentSetSize);
                    Use(process.UserProcessorTime);
                    Use(process.KernelProcessorTime);
                }
                catch
                {
                }
            }
        }
    }
}