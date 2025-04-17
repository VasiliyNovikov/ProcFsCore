using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ProcFsCore;

public unsafe struct NetHardwareAddress
{
    private const int Length = 6;
        
#pragma warning disable 649
    private fixed byte _data[Length];
#pragma warning restore 649
        
    private Span<byte> Data => MemoryMarshal.CreateSpan<byte>(ref _data[0], Length);

    public NetHardwareAddress(ReadOnlySpan<byte> address)
    {
        address.CopyTo(Data);    
    }
        
    [SkipLocalsInit]
    public static NetHardwareAddress Parse(ReadOnlySpan<byte> address)
    {
        Span<byte> addressBytes = stackalloc byte[Length];
        for (var i = 0; i < Length; ++i)
        {
            var hexPart = address.Slice(i * 3, 2);
            addressBytes[i] = AsciiParser.Parse<byte>(hexPart, 'x');
        }

        return new NetHardwareAddress(addressBytes);
    }

    private static readonly string[] ByteHexes = Enumerable.Range(0, 256).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)).ToArray();

    [SkipLocalsInit]
    public override string ToString()
    {
        Span<char> addressStr = stackalloc char[Length * 3 - 1];
        for (var i = 0; i < Data.Length; ++i)
        {
            if (i > 0)
                addressStr[i * 3 - 1] = ':';
            var hex = ByteHexes[Data[i]].AsSpan();
            hex.CopyTo(addressStr.Slice(i * 3, 2));
        }

        return addressStr.ToString();
    }
}