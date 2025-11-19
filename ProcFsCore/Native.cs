using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("ProcFsCore.Tests")]

namespace ProcFsCore;

internal static unsafe partial class Native
{
    private const string LibC = "libc.so.6";

    public static readonly int TicksPerSecond = SystemConfig(SystemConfigName.TicksPerSecond);

    [LibraryImport(LibC, EntryPoint = "gettid")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int GetTid();

    [LibraryImport(LibC, EntryPoint = "sysconf", SetLastError = true)]
    private static partial int SystemConfig(SystemConfigName name);

    private enum SystemConfigName
    {
        TicksPerSecond = 2
    }

    [LibraryImport(LibC, EntryPoint = "readlink", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial IntPtr ReadLink(string path, void* buffer, IntPtr bufferSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ReadLink(string path, Span<byte> buffer, out int bytesRead)
    {
        fixed (void* bufferPtr = &buffer.GetPinnableReference())
        {
            bytesRead = ReadLink(path, bufferPtr, new IntPtr(buffer.Length)).ToInt32();
            return bytesRead < 0
                ? throw new Win32Exception()
                : bytesRead < buffer.Length;
        }
    }

    [LibraryImport(LibC, EntryPoint = "open", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial int OpenRaw(string path, int flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Open(string path, int flags)
    {
        var descriptor = OpenRaw(path, flags);
        if (descriptor == -1)
            throw new Win32Exception();
        return descriptor;
    }

    [LibraryImport(LibC, EntryPoint = "close", SetLastError = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial int CloseRaw(int descriptor);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Close(int descriptor)
    {
        if (CloseRaw(descriptor) == -1)
            throw new Win32Exception();  
    }
        
    [LibraryImport(LibC, EntryPoint = "read", SetLastError = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial IntPtr Read(int descriptor, void* buffer, IntPtr bufferSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Read(int descriptor, Span<byte> buffer)
    {
        fixed (void* bufferPtr = &buffer.GetPinnableReference())
        {
            var bytesRead = Read(descriptor, bufferPtr, new IntPtr(buffer.Length)).ToInt32();
            if (bytesRead == -1)
                throw new Win32Exception();
            return bytesRead;
        }
    }

    [LibraryImport(LibC, EntryPoint = "write", SetLastError = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial IntPtr Write(int descriptor, void* buffer, IntPtr bufferSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(int descriptor, ReadOnlySpan<byte> buffer)
    {
        fixed (void* bufferPtr = &buffer.GetPinnableReference())
        {
            var bytesWritten = Write(descriptor, bufferPtr, new IntPtr(buffer.Length)).ToInt32();
            if (bytesWritten == -1)
                throw new Win32Exception();
            return bytesWritten;
        }
    }

    [LibraryImport(LibC, EntryPoint = "clock_gettime", SetLastError = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressGCTransition]
    private static partial int ClockGetTimeRaw(ClockId clockId, out TimeSpec timeSpec);
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