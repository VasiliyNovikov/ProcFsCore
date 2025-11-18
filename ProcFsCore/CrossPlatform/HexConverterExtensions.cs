#if !NET10_0_OR_GREATER
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

public static class ConvertExtensions
{
    private static ReadOnlySpan<byte> Hexes1 => "0123456789abcdef"u8;
    private static ReadOnlySpan<byte> Hexes2 => "0123456789ABCDEF"u8;
    private static readonly int[] HexLookup = new int[65536];

    static ConvertExtensions()
    {
        Array.Fill(HexLookup, -1);
        for (var lo = 0; lo < 16; ++lo)
            for (var hi = 0; hi < 16; ++hi)
            {
                var value = lo + (hi << 4);
                HexLookup[Index(lo, hi, Hexes1, Hexes1)] = value;
                HexLookup[Index(lo, hi, Hexes1, Hexes2)] = value;
                HexLookup[Index(lo, hi, Hexes2, Hexes1)] = value;
                HexLookup[Index(lo, hi, Hexes2, Hexes2)] = value;
            }

        return;

        static ushort Index(int lo, int hi, ReadOnlySpan<byte> hexes1, ReadOnlySpan<byte> hexes2)
        {
            return (ushort)(hexes1[hi] + (hexes2[lo] << 8));
        }
    }

    extension(Convert)
    {
        [SkipLocalsInit]
        public static OperationStatus FromHexString(ReadOnlySpan<byte> utf8Source, Span<byte> destination, out int bytesConsumed, out int bytesWritten)
        {
            if (utf8Source.Length % 2 != 0 || destination.Length < utf8Source.Length / 2)
            {
                bytesConsumed = 0;
                bytesWritten = 0;
                return OperationStatus.InvalidData;
            }

            var hexSource = MemoryMarshal.Cast<byte, ushort>(utf8Source);
            for (var i = 0; i < hexSource.Length; ++i)
            {
                var value = HexLookup[hexSource[i]];
                if (value == -1)
                {
                    bytesConsumed = 0;
                    bytesWritten = 0;
                    return OperationStatus.InvalidData;
                }
                destination[i] = (byte)value;
            }

            bytesConsumed = utf8Source.Length;
            bytesWritten = utf8Source.Length / 2;
            return OperationStatus.Done;
        }
    }
}
#endif