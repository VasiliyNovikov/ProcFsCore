using System;
using System.Diagnostics;

namespace ProcFsCore;

public class ProcFsBootTime
{
    private static readonly ReadOnlyMemory<byte> BtimeStr = "btime ".ToUtf8();
    private readonly string _statPath;
    private readonly TimeSpan _bootTimeCacheInterval = TimeSpan.FromSeconds(0.5);
    private readonly Stopwatch _bootTimeCacheTimer = new();
    private DateTime? _bootTimeUtc;

    internal ProcFsBootTime(ProcFs instance) => _statPath = instance.PathFor("stat");

    public DateTime UtcValue
    {
        get
        {
            lock (_bootTimeCacheTimer)
            {
                if (_bootTimeUtc == null || _bootTimeCacheTimer.Elapsed > _bootTimeCacheInterval)
                {
                    var statReader = new Utf8FileReader(_statPath, 4096);
                    try
                    {
                        statReader.SkipFragment(BtimeStr.Span, true);
                        if (statReader.EndOfStream)
                            throw new NotSupportedException();

                        var bootTimeSeconds = statReader.ReadInt64();
                        _bootTimeUtc = DateTime.UnixEpoch + TimeSpan.FromSeconds(bootTimeSeconds);
                        _bootTimeCacheTimer.Restart();
                    }
                    finally
                    {
                        statReader.Dispose();
                    }
                }

                return _bootTimeUtc.Value;
            }
        }
    }
}