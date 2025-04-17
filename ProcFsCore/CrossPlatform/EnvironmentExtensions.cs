#if NETSTANDARD2_0
using ProcFsCore;

namespace System;

internal static class EnvironmentExtensions
{
    private static int? _currentProcessIdValue;

    extension(Environment)
    {
        public static int ProcessId => _currentProcessIdValue ??= Native.GetPid();
    }
}
#endif