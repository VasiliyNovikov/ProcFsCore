using System.Collections.Generic;

namespace ProcFsCore;

public class ProcFsNetServices
{
    private readonly string _netPath;

    internal ProcFsNetServices(string netPath) => _netPath = netPath;

    public IEnumerable<NetService> Tcp(NetAddressVersion addressVersion) => NetService.GetTcp(_netPath, addressVersion);
    public IEnumerable<NetService> Udp(NetAddressVersion addressVersion) => NetService.GetUdp(_netPath, addressVersion);
    public IEnumerable<NetService> Raw(NetAddressVersion addressVersion) => NetService.GetRaw(_netPath, addressVersion);
    public IEnumerable<NetService> Unix() => NetService.GetUnix(_netPath);
}