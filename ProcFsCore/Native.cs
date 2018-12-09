using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcFsCore
{
    public static class Native
    {
        [DllImport("libc", EntryPoint = "getpid")]
        public static extern int GetPid();

        [DllImport("libc", EntryPoint = "sysconf", SetLastError = true)]
        public static extern int SystemConfig(SystemConfigName name);
        
        public enum SystemConfigName
        {
            PageSize = 1,
            TicksPerSecond = 2
        }
        
        [DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
        private static extern IntPtr ReadLink(string path, StringBuilder buffer, IntPtr bufferSize);

        public static string ReadLink(string path)
        {
            var buffer = new StringBuilder(256);
            while (true)
            {
                var readSize = ReadLink(path, buffer, new IntPtr(buffer.Capacity)).ToInt32();
                if (readSize < 0)
                    throw new Win32Exception();
                if (readSize < buffer.Capacity)
                    return buffer.ToString(0, readSize);

                buffer.Capacity *= 2;
            }
        }
    }
}