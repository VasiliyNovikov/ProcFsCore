using System.Collections.Generic;

namespace ProcFsCore
{
    public static partial class ProcFs
    {
        public static class Net
        {
            public static IEnumerable<NetStatistics> Statistics() => NetStatistics.GetAll();
        }
    }
}