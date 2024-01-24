using System.Collections.Generic;

namespace ProcFsCore;

public class ProcFsNetServices
{
    private readonly ProcFs _instance;

    internal ProcFsNetServices(ProcFs instance) => _instance = instance;

    public IEnumerable<NetService> Tcp(NetAddressVersion addressVersion) => NetService.GetTcp(_instance, addressVersion);
    public IEnumerable<NetService> Udp(NetAddressVersion addressVersion) => NetService.GetUdp(_instance, addressVersion);
    public IEnumerable<NetService> Raw(NetAddressVersion addressVersion) => NetService.GetRaw(_instance, addressVersion);
    public IEnumerable<NetService> Unix() => NetService.GetUnix(_instance);
}