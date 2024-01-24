using System.Collections.Generic;

namespace ProcFsCore
{
    public class ProcFsDisk
    {
        private readonly ProcFs _instance;
        
        internal ProcFsDisk(ProcFs instance) => _instance = instance;

        public IEnumerable<DiskStatistics> Statistics() => DiskStatistics.GetAll(_instance);
    }
}