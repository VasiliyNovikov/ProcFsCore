namespace ProcFsCore;

public class ProcFsMemory
{
    private readonly ProcFs _instance;
        
    internal ProcFsMemory(ProcFs instance) => _instance = instance;
        
    public MemoryStatistics Statistics() => MemoryStatistics.Get(_instance);
}