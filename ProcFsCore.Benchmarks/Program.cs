using BenchmarkDotNet.Running;

namespace ProcFsCore.Benchmarks
{
    internal static class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<ProcessAllBenchmarks>();
            //BenchmarkRunner.Run<NetStatisticsAllBenchmarks>();
        }
    }
}