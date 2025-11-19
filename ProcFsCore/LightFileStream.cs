using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

public readonly struct LightFileStream : IDisposable
{
    private readonly int _descriptor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LightFileStream(string path, LightFileStreamAccess mode)
    {
        try
        {
            _descriptor = Native.Open(path, (int)mode);
        }
        catch(Win32Exception e)
        {
            throw new IOException(e.Message, e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Native.Close(_descriptor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(Span<byte> buffer) => Native.Read(_descriptor, buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(ReadOnlySpan<byte> buffer) => Native.Write(_descriptor, buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LightFileStream OpenRead(string path) => new(path, LightFileStreamAccess.ReadOnly);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LightFileStream OpenWrite(string path) => new(path, LightFileStreamAccess.WriteOnly);
}