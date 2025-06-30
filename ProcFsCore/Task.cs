using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace ProcFsCore;

public struct Task
{
    private static readonly SearchValues<byte> ProcessNameEndSeparator = SearchValues.Create(")"u8);

    private readonly string _basePath;
    private bool _initialized;

    internal ProcFs Instance { get; }
    internal string Path => field ??= System.IO.Path.Combine(_basePath, $"{Id}");

    public int Id { get; }

    public string Name
    {
        get
        {
            EnsureInitialized();
            return field!;
        }
        private set;
    }

    public TaskState State
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public int ParentId
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public int GroupId
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public int SessionId
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public long MinorFaults
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public long MajorFaults
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public double UserProcessorTime
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public double KernelProcessorTime
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public short Priority
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public short Nice
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public int ThreadCount
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public long StartTimeTicks
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public long VirtualMemorySize
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public long ResidentSetSize
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        private set;
    }

    public string CommandLine
    {
        get
        {
            if (field is null)
            {
                try
                {
                    using var reader = new AsciiFileReader(System.IO.Path.Combine(Path, "cmdline"), 256);
                    field = reader.ReadToEnd().Trim((byte)'\0').ToAsciiString();
                }
                catch (IOException)
                {
                    field = "";
                }
            }
            return field;
        }
        private set;
    }
        
    private DateTime? _startTimeUtc;
    public DateTime StartTimeUtc
    {
        get
        {
            _startTimeUtc ??= Instance.BootTimeUtc + TimeSpan.FromSeconds(StartTimeTicks / (double) Native.TicksPerSecond);
            return _startTimeUtc.Value;
        }
    }

    public IEnumerable<Link> OpenFiles
    {
        get
        {
            foreach (var linkFile in Directory.EnumerateFiles(System.IO.Path.Combine(Path, "fd")))
                yield return Link.Read(linkFile);
        }
    }

    public TaskIO IO => TaskIO.Get(Path);

    public ProcFsNet Net => field ??= new(Instance, Path);

    internal Task(ProcFs instance, string basePath, int id)
    {
        Instance = instance;
        _basePath = basePath;
        _initialized = false;
        Id = id;
    }

    private void EnsureInitialized()
    {
        if (!_initialized) 
            Refresh();
    }

    public void Refresh()
    {
        _initialized = false;
        CommandLine = null!;
        _startTimeUtc = null;
        var statPath = System.IO.Path.Combine(Path, "stat");
        using var statReader = new AsciiFileReader(statPath, 512);
        // See http://man7.org/linux/man-pages/man5/proc.5.html /proc/[pid]/stat section

        // (1) id
        statReader.SkipWord();

        // (2) name
        Name = statReader.ReadWord(ProcessNameEndSeparator)[1..].ToAsciiString();
        statReader.SkipWhiteSpaces();

        // (3) state
        State = GetTaskState((char) statReader.ReadWord()[0]);

        // (4) ppid
        ParentId = statReader.ReadInt32();

        // (5) pgrp
        GroupId = statReader.ReadInt32();

        // (6) session
        SessionId = statReader.ReadInt32();

        // (7) tty_nr
        statReader.SkipWord();

        // (8) tpgid
        statReader.SkipWord();

        // (9) flags
        statReader.SkipWord();

        // (10) minflt
        MinorFaults = statReader.ReadInt64();

        // (11) cminflt
        statReader.SkipWord();

        // (12) majflt
        MajorFaults = statReader.ReadInt64();

        // (13) cmajflt
        statReader.SkipWord();

        // (14) utime
        UserProcessorTime = statReader.ReadInt64() / (double) Native.TicksPerSecond;

        // (15) stime
        KernelProcessorTime = statReader.ReadInt64() / (double) Native.TicksPerSecond;

        // (16) cutime
        statReader.SkipWord();

        // (17) cstime
        statReader.SkipWord();

        // (18) priority
        Priority = statReader.ReadInt16();

        // (19) nice
        Nice = statReader.ReadInt16();

        // (20) num_threads
        ThreadCount = statReader.ReadInt32();

        // (21) itrealvalue
        statReader.SkipWord();

        // (22) starttime
        StartTimeTicks = statReader.ReadInt64();

        // (23) vsize
        VirtualMemorySize = statReader.ReadInt64();

        // (24) rss
        ResidentSetSize = statReader.ReadInt64() * Environment.SystemPageSize;

        _initialized = true;
    }

    internal static IEnumerable<Task> GetAll(ProcFs instance, string basePath)
    {
        foreach (var idPath in Directory.EnumerateDirectories(basePath))
            if (int.TryParse(System.IO.Path.GetFileName(idPath), out var id))
                yield return new Task(instance, basePath, id);
    }

    private static TaskState GetTaskState(char state)
    {
        switch (state)
        {
            case 'R':
                return TaskState.Running;
            case 'S':
                return TaskState.Sleeping;
            case 'D':
                return TaskState.Waiting;
            case 'Z':
                return TaskState.Zombie;
            case 'T':
                return TaskState.Stopped;
            case 't':
                return TaskState.TracingStop;
            case 'x':
            case 'X':
                return TaskState.Dead;
            case 'K':
                return TaskState.WakeKill;
            case 'W':
                return TaskState.Waking;
            case 'P':
                return TaskState.Parked;
            default:
                return TaskState.Unknown;
        }
    }
}