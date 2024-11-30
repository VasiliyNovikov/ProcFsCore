using System.Collections.Generic;

namespace ProcFsCore;

public class ProcFsCpu
{
    private readonly ProcFs _instance;

    internal ProcFsCpu(ProcFs instance) => _instance = instance;

    public IEnumerable<CpuStatistics> Statistics() => CpuStatistics.GetAll(_instance);
}