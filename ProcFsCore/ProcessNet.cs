using System.Collections.Generic;

namespace ProcFsCore;

public readonly struct ProcessNet
{
    private readonly int _pid;

    internal ProcessNet(int pid) => _pid = pid;

    IEnumerable<NetStatistics> Statistics => NetStatistics.Get(_pid);
}