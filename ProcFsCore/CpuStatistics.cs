using System;
using System.Buffers.Text;
using System.Collections.Generic;

namespace ProcFsCore
{
    public struct CpuStatistics
    {
        private const string StatPath = ProcFs.RootPath + "/stat";
        
        public short? CpuNumber { get; private set; }
        public double UserTime { get; private set; }
        public double NiceTime { get; private set; }
        public double KernelTime { get; private set; }
        public double IdleTime { get; private set; }
        public double IrqTime { get; private set; }
        public double SoftIrqTime { get; private set; }

        private static readonly ReadOnlyMemory<byte> CpuNumberStart = "cpu".ToUtf8();
        internal static IEnumerable<CpuStatistics> GetAll()
        {
            using (var buffer = Buffer.FromFile(StatPath, 8192))
            {
                var position = 0;
                while (position < buffer.Length)
                {
                    var bufferReader = new Utf8SpanReader(buffer.Span) {Position = position};
                    var cpuStr = bufferReader.ReadWord();
                    if (!cpuStr.StartsWith(CpuNumberStart.Span))
                        yield break;

                    var cpuNumberStr = cpuStr.Slice(CpuNumberStart.Length);
                    short? cpuNumber;
                    if (cpuNumberStr.Length == 0)
                        cpuNumber = null;
                    else
                    {
                        Utf8Parser.TryParse(cpuNumberStr, out short num, out _);
                        cpuNumber = num;
                    }
                    
                    var ticksPerSecond = (double) ProcFs.TicksPerSecond;
                    var userTime = bufferReader.ReadInt64() / ticksPerSecond;
                    var niceTime = bufferReader.ReadInt64() / ticksPerSecond;
                    var kernelTime = bufferReader.ReadInt64() / ticksPerSecond;
                    var idleTime = bufferReader.ReadInt64() / ticksPerSecond;
                    bufferReader.ReadWord();
                    var irqTime = bufferReader.ReadInt64() / ticksPerSecond;
                    var softIrqTime = bufferReader.ReadInt64() / ticksPerSecond;
                    
                    bufferReader.ReadLine();
                    position = bufferReader.Position;

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
        }
    }
}