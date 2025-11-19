using System;
using System.Collections.Generic;
using NetworkingPrimitivesCore;

namespace ProcFsCore;

public class ProcFsNetServices
{
    private readonly string _netPath;

    internal ProcFsNetServices(string netPath) => _netPath = netPath;

    public IEnumerable<NetService<IPv4Address>> Tcp() => NetService<IPv4Address>.GetTcp(_netPath);
    public IEnumerable<NetService<IPv4Address>> Udp() => NetService<IPv4Address>.GetUdp(_netPath);
    public IEnumerable<NetService<IPv4Address>> Raw() => NetService<IPv4Address>.GetRaw(_netPath);

    public IEnumerable<NetService<IPv6Address>> Tcp6() => NetService<IPv6Address>.GetTcp(_netPath);
    public IEnumerable<NetService<IPv6Address>> Udp6() => NetService<IPv6Address>.GetUdp(_netPath);
    public IEnumerable<NetService<IPv6Address>> Raw6() => NetService<IPv6Address>.GetRaw(_netPath);

    public IEnumerable<UnixService> Unix() => UnixService.GetAll(_netPath);
}