using System;
using System.Collections.Generic;
using System.IO;

namespace ProcFsCore
{
    public struct Process
    {
        private static readonly int CurrentPid = Native.GetPid();
        
        public int Pid { get; }
        public string Name { get; private set; }
        public ProcessState State { get; private set; }
        public int ParentPid { get; private set; }
        public int GroupId { get; private set; }
        public int SessionId { get; private set; }
        public long MinorFaults { get; private set; }
        public long MajorFaults { get; private set; }
        public double UserProcessorTime { get; private set; }
        public double KernelProcessorTime { get; private set; }
        public short Priority { get; private set; }
        public short Nice { get; private set; }
        public int ThreadCount { get; private set; }
        public long StartTimeTicks { get; private set; }
        public long VirtualMemorySize { get; private set; }
        public long ResidentSetSize { get; private set; }

        private string _commandLine;
        public string CommandLine
        {
            get
            {
                if (_commandLine == null)
                {
                    try
                    {
                        using (var cmdLineBuffer = Buffer.FromFile($"{ProcFs.RootPath}/{Pid}/cmdline"))
                        {
                            cmdLineBuffer.Span.Replace('\0', ' ');
                            var cmdLineSpan = cmdLineBuffer.Span.Trim();
                            _commandLine = cmdLineSpan.IsEmpty ? "" : cmdLineSpan.ToUtf8String();
                        }
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
                if (_startTimeUtc == null)
                    _startTimeUtc = ProcFs.BootTimeUtc + TimeSpan.FromSeconds(StartTimeTicks / (double)ProcFs.TicksPerSecond);
                return _startTimeUtc.Value;
            }
        }

        public IEnumerable<Link> OpenFiles
        {
            get
            {
                foreach (var linkFile in Directory.EnumerateFiles($"{ProcFs.RootPath}/{Pid}/fd"))
                    yield return Link.Read(linkFile);
            }
        }
        
        public static Process Current => new Process(CurrentPid);
        
        public Process(int pid, bool initialize = true)
            : this()
        {
            Pid = pid;
            if (initialize)
                Refresh();
        }
        
        public void Refresh()
        {
            _commandLine = null;
            _startTimeUtc = null;
            using (var statStr = Buffer.FromFile($"{ProcFs.RootPath}/{Pid}/stat"))
            {
                // See http://man7.org/linux/man-pages/man5/proc.5.html /proc/[pid]/stat section
                var statReader = new Utf8SpanReader(statStr.Span);

                // (1) pid
                statReader.ReadWord();

                // (2) name
                var name = statReader.ReadWord();
                Name = name.Slice(1, name.Length - 2).ToUtf8String();

                // (3) state
                State = GetProcessState((char) statReader.ReadWord()[0]);

                // (4) ppid
                ParentPid = statReader.ReadInt32();

                // (5) pgrp
                GroupId = statReader.ReadInt32();

                // (6) session
                SessionId = statReader.ReadInt32();

                // (7) tty_nr
                statReader.ReadWord();

                // (8) tpgid
                statReader.ReadWord();

                // (9) flags
                statReader.ReadWord();

                // (10) minflt
                MinorFaults = statReader.ReadInt64();

                // (11) cminflt
                statReader.ReadWord();

                // (12) majflt
                MajorFaults = statReader.ReadInt64();

                // (13) cmajflt
                statReader.ReadWord();

                // (14) utime
                UserProcessorTime = statReader.ReadInt64() / (double) ProcFs.TicksPerSecond;

                // (15) stime
                KernelProcessorTime = statReader.ReadInt64() / (double) ProcFs.TicksPerSecond;

                // (16) cutime
                statReader.ReadWord();

                // (17) cstime
                statReader.ReadWord();

                // (18) priority
                Priority = statReader.ReadInt16();

                // (19) nice
                Nice = statReader.ReadInt16();

                // (20) num_threads
                ThreadCount = statReader.ReadInt32();

                // (21) itrealvalue
                statReader.ReadWord();

                // (22) starttime
                StartTimeTicks = statReader.ReadInt64();

                // (23) vsize
                VirtualMemorySize = statReader.ReadInt64();

                // (24) rss
                ResidentSetSize = statReader.ReadInt64() * Environment.SystemPageSize;
            }
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
}