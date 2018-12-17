using System;
using System.Buffers.Text;
using System.Net;
using System.Runtime.InteropServices;

namespace ProcFsCore
{
    public unsafe struct NetAddress
    {
        private const int MaxAddressLength = 16; 
#pragma warning disable 649
        private fixed byte _data[MaxAddressLength];
#pragma warning restore 649
        private readonly int _length;

        private Span<byte> Data => MemoryMarshal.CreateSpan(ref _data[0], _length);

        public NetAddressVersion Version => _length == 4 ? NetAddressVersion.IPv4 : NetAddressVersion.IPv6;
        
        public bool IsEmpty
        {
            get
            {
                foreach (var part in Data)
                    if (part != 0)
                        return false;
                return true;
            }
        }

        public NetAddress(ReadOnlySpan<byte> address)
        {
            _length = address.Length;
            address.CopyTo(Data);    
        }

        public static NetAddress Parse(ReadOnlySpan<byte> hexAddress)
        {
            Span<uint> address = stackalloc uint[MaxAddressLength >> 2];
            var addressLength = hexAddress.Length >> 3;
            for (var i = 0; i < addressLength; ++i)
            {
                var hexPart = hexAddress.Slice(i << 3, 8);
                Utf8Parser.TryParse(hexPart, out uint addressPart, out _, 'x');
                address[i] = addressPart;
            }

            return new NetAddress(MemoryMarshal.Cast<uint, byte>(address.Slice(0, addressLength)));
        }
        
        public override string ToString() => ((IPAddress)this).ToString();

        public static implicit operator IPAddress(in NetAddress address) => new IPAddress(address.Data);
    }
}