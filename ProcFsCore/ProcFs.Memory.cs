﻿namespace ProcFsCore
{
    public static partial class ProcFs
    {
        public static class Memory
        {
            public static MemoryStatistics Statistics() => MemoryStatistics.Get();
        }
    }
}