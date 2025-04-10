using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace ProcFsCore;

public struct Process
{
    private static readonly SearchValues<byte> ProcessNameEndSeparator = SearchValues.Create(")"u8);

    private readonly ProcFs _instance;
    private bool _initialized;

    private int _pid;
    public int Pid
    {
        get
        {
            if (_pid == 0)
                EnsureInitialized();
            return _pid;
        }
    }

    private string? _name;
    public string Name
    {
        get
        {
            EnsureInitialized();
            return _name!;
        }
    }

    private ProcessState _state;
    public ProcessState State
    {
        get
        {
            EnsureInitialized();
            return _state;
        }
    }

    private int _parentPid;
    public int ParentPid
    {
        get
        {
            EnsureInitialized();
            return _parentPid;
        }
    }

    private int _groupId;
    public int GroupId
    {
        get
        {
            EnsureInitialized();
            return _groupId;
        }
    }

    private int _sessionId;
    public int SessionId
    {
        get
        {
            EnsureInitialized();
            return _sessionId;
        }
    }

    private long _minorFaults;
    public long MinorFaults
    {
        get
        {
            EnsureInitialized();
            return _minorFaults;
        }
    }

    private long _majorFaults;
    public long MajorFaults
    {
        get
        {
            EnsureInitialized();
            return _majorFaults;
        }
    }

    private double _userProcessorTime;
    public double UserProcessorTime
    {
        get
        {
            EnsureInitialized();
            return _userProcessorTime;
        }
    }

    private double _kernelProcessorTime;
    public double KernelProcessorTime
    {
        get
        {
            EnsureInitialized();
            return _kernelProcessorTime;
        }
    }

    private short _priority;
    public short Priority
    {
        get
        {
            EnsureInitialized();
            return _priority;
        }
    }

    private short _nice;
    public short Nice
    {
        get
        {
            EnsureInitialized();
            return _nice;
        }
    }

    private int _threadCount;
    public int ThreadCount
    {
        get
        {
            EnsureInitialized();
            return _threadCount;
        }
    }

    private long _startTimeTicks;
    public long StartTimeTicks
    {
        get
        {
            EnsureInitialized();
            return _startTimeTicks;
        }
    }

    private long _virtualMemorySize;
    public long VirtualMemorySize
    {
        get
        {
            EnsureInitialized();
            return _virtualMemorySize;
        }
    }

    private long _residentSetSize;
    public long ResidentSetSize
    {
        get
        {
            EnsureInitialized();
            return _residentSetSize;
        }
    }

    private string? _commandLine;
    public string CommandLine
    {
        get
        {
            if (_commandLine is null)
            {
                try
                {
                    using var reader = new AsciiFileReader(_instance.PathFor($"{Pid}/cmdline"), 256);
                    _commandLine = reader.ReadToEnd().Trim((byte)'\0').ToAsciiString();
                }
                catch (IOException)
                {
                    _commandLine = "";
                }
            }

            return _commandLine;
        }
    }
        
    private DateTime? _startTimeUtc;
    public DateTime StartTimeUtc
    {
        get
        {
            _startTimeUtc ??= _instance.BootTimeUtc + TimeSpan.FromSeconds(StartTimeTicks / (double) Native.TicksPerSecond);
            return _startTimeUtc.Value;
        }
    }

    public IEnumerable<Link> OpenFiles
    {
        get
        {
            foreach (var linkFile in Directory.EnumerateFiles(_instance.PathFor($"{Pid}/fd")))
                yield return Link.Read(linkFile);
        }
    }

    public ProcessIO IO => ProcessIO.Get(_instance, Pid);

    public ProcessNet Net => new(_instance, Pid);

    internal Process(ProcFs instance, int pid)
        : this()
    {
        _instance = instance;
        _pid = pid;
    }

    private void EnsureInitialized()
    {
        if (!_initialized) 
            Refresh();
    }

    public void Refresh()
    {
        _initialized = false;
        _commandLine = null;
        _startTimeUtc = null;
        var statPath = _instance.PathFor(Pid == 0 ? "self/stat" : $"{Pid}/stat"); // 0 - special case for current process of non-default instance
        using var statReader = new AsciiFileReader(statPath, 512);
        // See http://man7.org/linux/man-pages/man5/proc.5.html /proc/[pid]/stat section

        // (1) pid
        _pid = statReader.ReadInt32();

        // (2) name
        _name = statReader.ReadWord(ProcessNameEndSeparator)[1..].ToAsciiString();
        statReader.SkipWhiteSpaces();

        // (3) state
        _state = GetProcessState((char) statReader.ReadWord()[0]);

        // (4) ppid
        _parentPid = statReader.ReadInt32();

        // (5) pgrp
        _groupId = statReader.ReadInt32();

        // (6) session
        _sessionId = statReader.ReadInt32();

        // (7) tty_nr
        statReader.SkipWord();

        // (8) tpgid
        statReader.SkipWord();

        // (9) flags
        statReader.SkipWord();

        // (10) minflt
        _minorFaults = statReader.ReadInt64();

        // (11) cminflt
        statReader.SkipWord();

        // (12) majflt
        _majorFaults = statReader.ReadInt64();

        // (13) cmajflt
        statReader.SkipWord();

        // (14) utime
        _userProcessorTime = statReader.ReadInt64() / (double) Native.TicksPerSecond;

        // (15) stime
        _kernelProcessorTime = statReader.ReadInt64() / (double) Native.TicksPerSecond;

        // (16) cutime
        statReader.SkipWord();

        // (17) cstime
        statReader.SkipWord();

        // (18) priority
        _priority = statReader.ReadInt16();

        // (19) nice
        _nice = statReader.ReadInt16();

        // (20) num_threads
        _threadCount = statReader.ReadInt32();

        // (21) itrealvalue
        statReader.SkipWord();

        // (22) starttime
        _startTimeTicks = statReader.ReadInt64();

        // (23) vsize
        _virtualMemorySize = statReader.ReadInt64();

        // (24) rss
        _residentSetSize = statReader.ReadInt64() * Environment.SystemPageSize;

        _initialized = true;
    }

    internal static IEnumerable<Process> GetAll(ProcFs instance)
    {
        foreach (var pidPath in Directory.EnumerateDirectories(instance.RootPath))
            if (int.TryParse(Path.GetFileName(pidPath), out var pid))
                yield return new Process(instance, pid);
    }
        
    private static ProcessState GetProcessState(char state)
    {
        switch (state)
        {
            case 'R':
                return ProcessState.Running;
            case 'S':
                return ProcessState.Sleeping;
            case 'D':
                return ProcessState.Waiting;
            case 'Z':
                return ProcessState.Zombie;
            case 'T':
                return ProcessState.Stopped;
            case 't':
                return ProcessState.TracingStop;
            case 'x':
            case 'X':
                return ProcessState.Dead;
            case 'K':
                return ProcessState.WakeKill;
            case 'W':
                return ProcessState.Waking;
            case 'P':
                return ProcessState.Parked;
            default:
                return ProcessState.Unknown;
        }
    }
}