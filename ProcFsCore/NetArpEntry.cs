using System.Collections.Generic;

namespace ProcFsCore
{
    public readonly struct NetArpEntry
    {
        public NetAddress Address { get; }
        public NetHardwareAddress HardwareAddress { get; }
        public string Mask { get; }
        public string Device { get; }

        private NetArpEntry(in NetAddress address, in NetHardwareAddress hardwareAddress, string mask, string device)
        {
            Address = address;
            HardwareAddress = hardwareAddress;
            Mask = mask;
            Device = device;
        }

        public override string ToString() => $"{Address.ToString()} {HardwareAddress.ToString()} {Mask} {Device}";

        internal static IEnumerable<NetArpEntry> GetAll(ProcFs instance)
        {
            var statReader = new Utf8FileReader(instance.PathFor("net/arp"), 1024);
            try
            {
                statReader.SkipLine();
                while (!statReader.EndOfStream)
                {
                    statReader.SkipWhiteSpaces();
                    var address = NetAddress.Parse(statReader.ReadWord(), NetAddressFormat.Human);
                    statReader.SkipWord();
                    statReader.SkipWord();
                    var hardwareAddress = NetHardwareAddress.Parse(statReader.ReadWord());
                    var maskBytes = statReader.ReadWord();
                    var mask = maskBytes.Length == 1 && maskBytes[0] == '*' ? "*" : maskBytes.ToUtf8String();
                    var device = statReader.ReadStringWord();
                    yield return new NetArpEntry(address, hardwareAddress, mask, device);
                }
            }
            finally
            {
                statReader.Dispose();
            }
        }
    }
}