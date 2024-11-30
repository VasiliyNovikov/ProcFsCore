using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("ProcFsCore.Tests")]

namespace ProcFsCore;

internal static class Native
{
    private const string LibC = "libc.so.6";

    public static readonly int TicksPerSecond = SystemConfig(SystemConfigName.TicksPerSecond);
    public static readonly int PageSize = SystemConfig(SystemConfigName.PageSize);

    [DllImport(LibC, EntryPoint = "getpid")]
    public static extern int GetPid();

    [DllImport(LibC, EntryPoint = "sysconf", SetLastError = true)]
    private static extern int SystemConfig(SystemConfigName name);

    private enum SystemConfigName
    {
        PageSize = 1,
        TicksPerSecond = 2
    }

    [DllImport(LibC, EntryPoint = "readlink", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern unsafe IntPtr ReadLink(string path, void* buffer, IntPtr bufferSize);
    public static unsafe Buffer<byte> ReadLink(string path)
    {
        var buffer = new Buffer<byte>(256);
        while (true)
        {
            fixed (void* bufferPtr = &buffer.Span.GetPinnableReference())
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

            buffer.Dispose();
            buffer = new Buffer<byte>(buffer.Length * 2);
        }
    }

    [DllImport(LibC, EntryPoint = "open", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int OpenRaw(string path, int flags);
    public static int Open(string path, int flags)
    {
        var descriptor = OpenRaw(path, flags);
        if (descriptor == -1)
            throw new Win32Exception();
        return descriptor;
    }

    [DllImport(LibC, EntryPoint = "close", SetLastError = true)]
    private static extern int CloseRaw(int descriptor);
    public static void Close(int descriptor)
    {
        if (CloseRaw(descriptor) == -1)
            throw new Win32Exception();  
    }
        
    [DllImport(LibC, EntryPoint = "read", SetLastError = true)]
    private static extern unsafe IntPtr Read(int descriptor, void* buffer, IntPtr bufferSize);
    public static unsafe int Read(int descriptor, Span<byte> buffer)
    {
        fixed (void* bufferPtr = &buffer.GetPinnableReference())
        {
            var bytesRead = Read(descriptor, bufferPtr, new IntPtr(buffer.Length)).ToInt32();
            if (bytesRead == -1)
                throw new Win32Exception();
            return bytesRead;
        }
    }

    [DllImport(LibC, EntryPoint = "write", SetLastError = true)]
    private static extern unsafe IntPtr Write(int descriptor, void* buffer, IntPtr bufferSize);
    public static unsafe int Write(int descriptor, ReadOnlySpan<byte> buffer)
    {
        fixed (void* bufferPtr = &buffer.GetPinnableReference())
        {
            var bytesWritten = Write(descriptor, bufferPtr, new IntPtr(buffer.Length)).ToInt32();
            if (bytesWritten == -1)
                throw new Win32Exception();
            return bytesWritten;
        }
    }

    [DllImport(LibC, EntryPoint = "clock_gettime", SetLastError = true)]
    private static extern int ClockGetTimeRaw(ClockId clockId, out TimeSpec timeSpec);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ClockGetTimeNanoseconds(ClockId clockId)
    {
        if (ClockGetTimeRaw(clockId, out var timeSpec) == -1)
            throw new Win32Exception();
        return timeSpec.Seconds * 1_000_000_000 + timeSpec.Nanoseconds;
    }

    public enum ClockId
    {
        RealTime = 0,
        BootTime = 7
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct TimeSpec
    {
        public readonly long Seconds;
        public readonly long Nanoseconds;
    }
}