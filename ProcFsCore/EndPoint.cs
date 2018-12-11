using System.Net;

namespace ProcFsCore
{
    public struct EndPoint
    {
        public Address Address { get; }
        public int Port { get; }

        public EndPoint(in Address address, int port)
        {
            Address = address;
            Port = port;
        }

        public override string ToString() => ((IPEndPoint)this).ToString();
        
        public static implicit operator IPEndPoint(in EndPoint endPoint) => new IPEndPoint(endPoint.Address, endPoint.Port);
    }
}