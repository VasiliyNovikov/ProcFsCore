using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;

namespace ProcFsCore
{
    public static partial class ProcFs
    {
        internal const string RootPath = "/proc";
        private const string StatPath = RootPath + "/stat";
        
        public static readonly int TicksPerSecond = Native.SystemConfig(Native.SystemConfigName.TicksPerSecond);

        private static readonly ReadOnlyMemory<byte> BtimeStr = "\nbtime ".ToUtf8();
        public static DateTime BootTimeUtc
        {
            get
            {
                // '/proc/stat -> btime' gets the boot time.
                // btime is the time of system boot in seconds since the Unix epoch.
                // It includes suspended time and is updated based on the system time (settimeofday).
                using (var statBuffer = Buffer.FromFile(StatPath, 8192))
                {
                    var btimeLineStart = statBuffer.Span.IndexOf(BtimeStr.Span);
                    if (btimeLineStart >= 0)
                    {
                        var btimeStart = btimeLineStart + BtimeStr.Length;
                        var btimeEnd = statBuffer.Span.IndexOf('\n', btimeStart);
                        if (btimeEnd > btimeStart && Utf8Parser.TryParse(statBuffer.Span.Slice(btimeStart, btimeEnd - btimeStart), out long bootTimeSeconds, out _))
                            return DateTime.UnixEpoch + TimeSpan.FromSeconds(bootTimeSeconds);
                    }
                }

                throw new NotSupportedException();
            }
        }
        
        public static IEnumerable<Process> Processes()
        {
            foreach (var pidPath in Directory.EnumerateDirectories(RootPath))
            {
                var pidDir = Path.GetFileName(pidPath);
                if (Int32.TryParse(pidDir, out var pid))
                    yield return new Process(pid);
            }
        }
    }
}