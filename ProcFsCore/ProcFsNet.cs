using System.Collections.Generic;

namespace ProcFsCore;

public class ProcFsNet
{
    private readonly ProcFs _instance;
    private readonly string _basePath;

    private string Path => field ??= System.IO.Path.Combine(_basePath, "net");

    public ProcFsNetServices Services => field ??= new(Path);

    public IEnumerable<NetStatistics> Statistics => NetStatistics.Get(Path, _instance.NetStatReceiveColumnCount);

    public IEnumerable<NetArpEntry> Arp => NetArpEntry.GetAll(Path);

    internal ProcFsNet(ProcFs instance, string basePath)
    {
        _instance = instance;
        _basePath = basePath;
    }
}