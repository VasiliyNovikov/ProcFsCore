using System;
using System.Collections.Generic;
using System.IO;

namespace ProcFsCore;

public struct Process
{
    private Task _processTask;

    private string ThreadBasePath => Path.Combine(_processTask.Path, "task");

    public readonly int Pid => _processTask.Id;
    public string Name => _processTask.Name;
    public TaskState State => _processTask.State;
    public int ParentPid => _processTask.ParentId;
    public int GroupId => _processTask.GroupId;
    public int SessionId => _processTask.SessionId;
    public long MinorFaults => _processTask.MinorFaults;
    public long MajorFaults => _processTask.MajorFaults;
    public double UserProcessorTime => _processTask.UserProcessorTime;
    public double KernelProcessorTime => _processTask.KernelProcessorTime;
    public short Priority => _processTask.Priority;
    public short Nice => _processTask.Nice;
    public int ThreadCount => _processTask.ThreadCount;
    public long StartTimeTicks => _processTask.StartTimeTicks;
    public long VirtualMemorySize => _processTask.VirtualMemorySize;
    public long ResidentSetSize => _processTask.ResidentSetSize;
    public string CommandLine => _processTask.CommandLine;
    public DateTime StartTimeUtc => _processTask.StartTimeUtc;
    public IEnumerable<Link> OpenFiles => _processTask.OpenFiles;
    public TaskIO IO => _processTask.IO;
    public ProcFsNet Net => _processTask.Net;
    public IEnumerable<Task> Threads => Task.GetAll(_processTask.Instance, ThreadBasePath);

    private Process(Task processTask) => _processTask = processTask;

    internal Process(ProcFs instance, int pid)
        : this(new(instance, instance.RootPath, pid))
    {
    }

    public void Refresh() => _processTask.Refresh();

    public Task Thread(int id) => new(_processTask.Instance, ThreadBasePath, id);

    internal static IEnumerable<Process> GetAll(ProcFs instance)
    {
        foreach (var task in Task.GetAll(instance, instance.RootPath))
            yield return new Process(task);
    }
}