using System.Net.NetworkInformation;
using BenchmarkDotNet.Attributes;

namespace ProcFsCore.Benchmarks;

public class NetStatisticsAllBenchmarks : BaseBenchmarks
{
    [Benchmark]
    public void Framework_NetStatistics_All()
    {
        foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
        {
            var stats = iface.GetIPStatistics();
            Use(stats.BytesReceived);
            Use(stats.UnicastPacketsReceived);
            Use(stats.IncomingPacketsDiscarded);
            Use(stats.IncomingPacketsWithErrors);
            Use(stats.BytesSent);
            Use(stats.UnicastPacketsSent);
            Use(stats.OutgoingPacketsDiscarded);
            Use(stats.OutgoingPacketsWithErrors);
        }
    }
        
    [Benchmark]
    public void ProcFs_NetStatistics_All()
    {
        foreach (var stats in ProcFs.Default.Net.Statistics)
        {
            Use(stats.Receive.Bytes);
            Use(stats.Receive.Packets);
            Use(stats.Receive.Drops);
            Use(stats.Receive.Errors);
            Use(stats.Transmit.Bytes);
            Use(stats.Transmit.Packets);
            Use(stats.Transmit.Drops);
            Use(stats.Transmit.Errors);
        }
    }
}