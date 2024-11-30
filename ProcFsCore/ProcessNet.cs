using System.Collections.Generic;

namespace ProcFsCore;

public readonly struct ProcessNet
{
    private readonly ProcFs _instance;
    private readonly int _pid;

    internal ProcessNet(ProcFs instance, int pid)
    {
        _instance = instance;
        _pid = pid;
    }

    public IEnumerable<NetStatistics> Statistics => NetStatistics.Get(_instance, _pid, _instance.Net.StatReceiveColumnCount);
}