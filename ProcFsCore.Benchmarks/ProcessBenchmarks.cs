using System.Runtime.CompilerServices;

namespace ProcFsCore.Benchmarks;

public class BaseBenchmarks
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    protected static void Use<T>(T value)
    {
    }
}