using System;
using System.Collections.Generic;
using System.IO;

namespace ProcFsCore
{
    public static class ProcFs
    {
        internal const string RootPath = "/proc";
        private const string StatPath = RootPath + "/stat";
        
        public static readonly int TicksPerSecond = Native.sysconf(Native.SysConfName._SC_CLK_TCK);
        
        public static DateTime BootTimeUtc
        {
            get
            {
                // '/proc/stat -> btime' gets the boot time.
                // btime is the time of system boot in seconds since the Unix epoch.
                // It includes suspended time and is updated based on the system time (settimeofday).
                var text = File.ReadAllText(StatPath);
                var btimeLineStart = text.IndexOf("\nbtime ", StringComparison.OrdinalIgnoreCase);
                if (btimeLineStart >= 0)
                {
                    var btimeStart = btimeLineStart + "\nbtime ".Length;
                    var btimeEnd = text.IndexOf('\n', btimeStart);
                    if (btimeEnd > btimeStart && Int64.TryParse(text.AsSpan(btimeStart, btimeEnd - btimeStart), out var bootTimeSeconds))
                        return DateTime.UnixEpoch + TimeSpan.FromSeconds(bootTimeSeconds);
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