using System;
using System.Buffers;
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
                        _commandLine = File.ReadAllText($"{ProcFs.RootPath}/{Pid}/cmdline").Replace('\0', ' ').Trim();
                    }
                    catch (IOException)
                    {
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
        
        public Process(int pid)
            : this()
        {
            Pid = pid;
            Refresh();
        }
        
        public void Refresh()
        {
            _commandLine = null;
            _startTimeUtc = null;
            Span<byte> statStr = stackalloc byte[512];
            byte[] statBuffer = null;
            var statLength = 0;
            try
            {
                using (var statStream = File.OpenRead($"{ProcFs.RootPath}/{Pid}/stat"))
                {
                    while (true)
                    {
                        var readBytes = statStream.Read(statStr.Slice(statLength));
                        statLength += readBytes;
                        if (statLength <= statStr.Length)
                            break;
                        statBuffer = ArrayPool<byte>.Shared.Rent(statStr.Length * 2);
                        statStr.CopyTo(statBuffer);
                        statStr = statBuffer;
                    }
                }
                
                // See http://man7.org/linux/man-pages/man5/proc.5.html /proc/[pid]/stat section
                statStr = statStr.Slice(0, statLength);
                var statReader = new Utf8SpanReader(statStr);
                
                // (1) pid
                statReader.ReadWord();
                
                // (2) name
                var name = statReader.ReadWord();
                Name = Utf8SpanReader.Encoding.GetString(name.Slice(1, name.Length - 2));
                
                // (3) state
                State = GetProcessState((char)statReader.ReadWord()[0]);
                
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
                UserProcessorTime = statReader.ReadUInt64() / (double)ProcFs.TicksPerSecond;
                
                // (15) stime
                KernelProcessorTime = statReader.ReadUInt64() / (double)ProcFs.TicksPerSecond;
                
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
            finally
            {
                if (statBuffer != null)
                    ArrayPool<byte>.Shared.Return(statBuffer);
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