using System;
using System.Collections.Generic;

namespace ProcFsCore
{
    public class ProcFsNet
    {
        private readonly ProcFs _instance;
        private readonly Lazy<int> _statReceiveColumnCount;

        internal int StatReceiveColumnCount => _statReceiveColumnCount.Value;

        public ProcFsNetServices Services { get; }

        public IEnumerable<NetStatistics> Statistics() => NetStatistics.GetAll(_instance, StatReceiveColumnCount);

        public IEnumerable<NetArpEntry> Arp() => NetArpEntry.GetAll(_instance);

        internal ProcFsNet(ProcFs instance)
        {
            _instance = instance;
            _statReceiveColumnCount = new Lazy<int>(() => NetStatistics.GetReceiveColumnCount(instance));
            Services = new ProcFsNetServices(instance);
        }
    }
}