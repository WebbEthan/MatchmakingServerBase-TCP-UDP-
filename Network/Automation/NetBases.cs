using System.Net.Sockets;
using Network.data;

namespace Network.Automation
{
    public abstract class NetType : Match
    {
        public NetType(MatchInitializer initializer) : base(initializer) {}
        public abstract void Send(Packet packet, ProtocolType protocol);
    }
    public class Star : NetType
    {
        public Star(MatchInitializer initializer) : base(initializer) {}
        public override void Send(Packet packet, ProtocolType protocol)
        {
            SendToAll(packet, protocol);
        }
    }
    public class Point : NetType
    {
        public Point(MatchInitializer initializer) : base(initializer) { MaxClients = 2; }
        public override void Send(Packet packet, ProtocolType protocol)
        {
            SendToAll(packet, protocol);
        }
    }
    public class Mesh : NetType
    {
        public Mesh(MatchInitializer initializer) : base(initializer) {}
        public override void Send(Packet packet, ProtocolType protocol)
        {
            SentToClient(packet.ReadString(), packet, protocol);
        }
    }
}