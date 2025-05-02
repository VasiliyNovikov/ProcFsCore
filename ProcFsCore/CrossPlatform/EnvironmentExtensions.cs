#if NETSTANDARD2_0
using ProcFsCore;

namespace System;

internal static class EnvironmentExtensions
{
    private static readonly int _processId = Native.GetPid();

    extension(Environment)
    {
        public static int ProcessId => _processId;
    }
}
#endif