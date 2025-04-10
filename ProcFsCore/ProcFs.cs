using System;
using System.Collections.Generic;
using System.IO;

namespace ProcFsCore;

public class ProcFs
{
    private const string DefaultRootPath = "/proc";

    public static readonly ProcFs Default = new();

    private readonly ProcFsBootTime _bootTime;

    internal readonly bool IsDefault;

    public string RootPath { get; }

    public DateTime BootTimeUtc => _bootTime.UtcValue;

    public ProcFsCpu Cpu { get; }

    public ProcFsDisk Disk { get; }
        
    public ProcFsMemory Memory { get; }

    public ProcFsNet Net { get; }

    public ProcFs(string rootPath = DefaultRootPath)
    {
        RootPath = rootPath;
        _bootTime = new ProcFsBootTime(this);
        Cpu = new ProcFsCpu(this);
        Disk = new ProcFsDisk(this);
        Memory = new ProcFsMemory(this);
        Net = new ProcFsNet(this);
        IsDefault = rootPath == DefaultRootPath;
    }

    internal string PathFor(string path) => Path.Combine(RootPath, path);

    public IEnumerable<Process> Processes() => ProcFsCore.Process.GetAll(this);

    public Process Process(int pid) => new(this, pid);

    public Process CurrentProcess => Process(IsDefault ? Native.CurrentProcessId : 0); // 0 - special case for current process of non-default instance
}