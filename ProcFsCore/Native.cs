using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

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
        private static extern unsafe IntPtr ReadLink(string path, byte* buffer, IntPtr bufferSize);

        public static unsafe Buffer ReadLink(string path)
        {
            var buffer = new Buffer(Buffer.MinimumCapacity);
            while (true)
            {
                fixed (byte* bufferPtr = &buffer.Span.GetPinnableReference())
                {

                    var readSize = ReadLink(path, bufferPtr, new IntPtr(buffer.Length)).ToInt32();
                    if (readSize < 0)
                        throw new Win32Exception();
                    if (readSize < buffer.Length)
                    {
                        buffer.Resize(readSize);
                        return buffer;
                    }
                }

                buffer.Resize(buffer.Length * 2);
            }
        }
    }
}