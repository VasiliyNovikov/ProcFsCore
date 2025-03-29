using System;
using System.Buffers.Text;
using System.Collections.Generic;

namespace ProcFsCore;

public struct CpuStatistics
{
    private static ReadOnlySpan<byte> CpuNumberStart => "cpu"u8;

    public short? CpuNumber { get; private set; }
    public double UserTime { get; private set; }
    public double NiceTime { get; private set; }
    public double KernelTime { get; private set; }
    public double IdleTime { get; private set; }
    public double IrqTime { get; private set; }
    public double SoftIrqTime { get; private set; }

    internal static IEnumerable<CpuStatistics> GetAll(ProcFs instance)
    {
        var statReader = new Utf8FileReader(instance.PathFor("stat"), 4096);
        try
        {
            while (!statReader.EndOfStream)
            {
                var cpuStr = statReader.ReadWord();
                if (!cpuStr.StartsWith(CpuNumberStart))
                    yield break;

                var cpuNumberStr = cpuStr[CpuNumberStart.Length..];
                short? cpuNumber;
                if (cpuNumberStr.Length == 0)
                    cpuNumber = null;
                else
                {
#pragma warning disable CA1806
                    Utf8Parser.TryParse(cpuNumberStr, out short num, out _);
#pragma warning restore CA1806
                    cpuNumber = num;
                }

                var ticksPerSecond = (double)Native.TicksPerSecond;
                var userTime = statReader.ReadInt64() / ticksPerSecond;
                var niceTime = statReader.ReadInt64() / ticksPerSecond;
                var kernelTime = statReader.ReadInt64() / ticksPerSecond;
                var idleTime = statReader.ReadInt64() / ticksPerSecond;
                statReader.SkipWord();
                var irqTime = statReader.ReadInt64() / ticksPerSecond;
                var softIrqTime = statReader.ReadInt64() / ticksPerSecond;

                statReader.SkipLine();

                yield return new CpuStatistics
                {
                    CpuNumber = cpuNumber,
                    UserTime = userTime,
                    NiceTime = niceTime,
                    KernelTime = kernelTime,
                    IdleTime = idleTime,
                    IrqTime = irqTime,
                    SoftIrqTime = softIrqTime
                };
            }
        }
        finally
        {
            statReader.Dispose();
        }
    }
}