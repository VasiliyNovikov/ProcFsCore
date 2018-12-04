using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace ProcFsCore
{
    internal static class Native
    {
        [DllImport("libc")]
        public static extern int getpid();

        [DllImport("libc", SetLastError = true)]
        public static extern int sysconf(SysConfName name);
        
        public enum SysConfName
        {
            _SC_CLK_TCK = 2
        }
    }
}