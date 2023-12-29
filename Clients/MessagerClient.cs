using System.Net.Sockets;
public class MessagerClient : Client<MSGMatch> // link the Matchtype to the Clienttype
{
    public MessagerClient(SocketData data):base(data)     // pass the SocketData into the base
    {
        // set the handle methods
        Handles = new Dictionary<int, PacketScripts>()      // DO NOT USES these binary values for packet id -> [0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA]
        {
            { 0, _msg } // when packet with packettype 0 comes in the _msg function is called
        };
    }
    private void _msg(Packet packet, ProtocolType protocolType)     // Example method
    {
        CurrentMatch.SendToAll(packet, protocolType);
    }
}