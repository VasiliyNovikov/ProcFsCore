using System;

namespace ProcFsCore;

public readonly struct LightFileStream : IDisposable
{
    private readonly int _descriptor;

    private LightFileStream(string path, LightFileStreamAccess mode) => _descriptor = Native.Open(path, (int)mode);

    public void Dispose() => Native.Close(_descriptor);

    public int Read(Span<byte> buffer) => Native.Read(_descriptor, buffer);

    public int Write(ReadOnlySpan<byte> buffer) => Native.Write(_descriptor, buffer);

    public static LightFileStream OpenRead(string path) => new(path, LightFileStreamAccess.ReadOnly);

    public static LightFileStream OpenWrite(string path) => new(path, LightFileStreamAccess.WriteOnly);
}