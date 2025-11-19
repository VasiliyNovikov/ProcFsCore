using System;
using System.Collections.Generic;
using NetworkingPrimitivesCore;

namespace ProcFsCore;

public class ProcFsNetServices
{
    private readonly string _netPath;

    internal ProcFsNetServices(string netPath) => _netPath = netPath;

    public IEnumerable<NetService<IPv4Address, uint>> Tcp() => NetService<IPv4Address, uint>.GetTcp(_netPath);
    public IEnumerable<NetService<IPv4Address, uint>> Udp() => NetService<IPv4Address, uint>.GetUdp(_netPath);
    public IEnumerable<NetService<IPv4Address, uint>> Raw() => NetService<IPv4Address, uint>.GetRaw(_netPath);

    public IEnumerable<NetService<IPv6Address, UInt128>> Tcp6() => NetService<IPv6Address, UInt128>.GetTcp(_netPath);
    public IEnumerable<NetService<IPv6Address, UInt128>> Udp6() => NetService<IPv6Address, UInt128>.GetUdp(_netPath);
    public IEnumerable<NetService<IPv6Address, UInt128>> Raw6() => NetService<IPv6Address, UInt128>.GetRaw(_netPath);

    public IEnumerable<UnixService> Unix() => UnixService.GetAll(_netPath);
}