using System;
using System.Net;

namespace ProcFsCore
{
    public enum AddressVersion
    {
        // ReSharper disable InconsistentNaming
        IPv4 = 4,
        IPv6 = 6
        // ReSharper restore InconsistentNaming
    }
    
    public unsafe struct Address
    {
#pragma warning disable 649
        private fixed byte _data[16];
#pragma warning restore 649
        private readonly int _length;

        private Span<byte> Data
        {
            get
            {
                fixed (byte* d = &_data[0])
                    return new Span<byte>(d, _length);
            }
        }

        public AddressVersion Version => _length == 4 ? AddressVersion.IPv4 : AddressVersion.IPv6; 

        public Address(ReadOnlySpan<byte> address)
        {
            _length = address.Length;
            address.CopyTo(Data);    
        }

        public override string ToString() => ((IPAddress)this).ToString();

        public static implicit operator IPAddress(in Address address) => new IPAddress(address.Data);
    }
}