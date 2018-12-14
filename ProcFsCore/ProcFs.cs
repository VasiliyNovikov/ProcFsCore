using System;
using System.Collections.Generic;
using System.IO;

namespace ProcFsCore
{
    public static partial class ProcFs
    {
        internal const string RootPath = "/proc";
        private const string StatPath = RootPath + "/stat";
        
        public static readonly int TicksPerSecond = Native.SystemConfig(Native.SystemConfigName.TicksPerSecond);

        private static readonly ReadOnlyMemory<byte> BtimeStr = "btime ".ToUtf8();
        public static DateTime BootTimeUtc
        {
            get
            {
                // '/proc/stat -> btime' gets the boot time.
                // btime is the time of system boot in seconds since the Unix epoch.
                // It includes suspended time and is updated based on the system time (settimeofday).
                using (var statReader = new Utf8FileReader(StatPath))
                {
                    statReader.SkipFragment(BtimeStr.Span, true);
                    if (statReader.EndOfStream)
                        throw new NotSupportedException();
                    
                    var bootTimeSeconds = statReader.ReadInt64();
                    return DateTime.UnixEpoch + TimeSpan.FromSeconds(bootTimeSeconds);
                }
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