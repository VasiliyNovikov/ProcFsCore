using System.Collections.Generic;
using System.IO;
using NetworkingPrimitivesCore;

namespace ProcFsCore;

public readonly struct NetArpEntry
{
    public IPv4Address Address { get; }
    public MACAddress HardwareAddress { get; }
    public string Mask { get; }
    public string Device { get; }

    private NetArpEntry(in IPv4Address address, in MACAddress hardwareAddress, string mask, string device)
    {
        Address = address;
        HardwareAddress = hardwareAddress;
        Mask = mask;
        Device = device;
    }

    public override string ToString() => $"{Address} {HardwareAddress} {Mask} {Device}";

    internal static IEnumerable<NetArpEntry> GetAll(string netPath)
    {
        using var statReader = new AsciiFileReader(Path.Combine(netPath, "arp"), 1024);
        statReader.SkipLine();
        while (!statReader.EndOfStream)
        {
            statReader.SkipWhiteSpaces();
            var address = IPv4Address.Parse(statReader.ReadWord());
            statReader.SkipWord();
            statReader.SkipWord();
            var hardwareAddress = MACAddress.Parse(statReader.ReadWord());
            var maskBytes = statReader.ReadWord();
            var mask = maskBytes.Length == 1 && maskBytes[0] == '*' ? "*" : maskBytes.ToAsciiString();
            var device = statReader.ReadStringWord();
            yield return new NetArpEntry(address, hardwareAddress, mask, device);
        }
    }
}