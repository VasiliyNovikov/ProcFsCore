using System;
using System.Runtime.CompilerServices;

namespace ProcFsCore;

public readonly struct LightFileStream : IDisposable
{
    private readonly int _descriptor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LightFileStream(string path, LightFileStreamAccess mode) => _descriptor = Native.Open(path, (int)mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => Native.Close(_descriptor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(Span<byte> buffer) => Native.Read(_descriptor, buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(ReadOnlySpan<byte> buffer) => Native.Write(_descriptor, buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LightFileStream OpenRead(string path) => new(path, LightFileStreamAccess.ReadOnly);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LightFileStream OpenWrite(string path) => new(path, LightFileStreamAccess.WriteOnly);
}