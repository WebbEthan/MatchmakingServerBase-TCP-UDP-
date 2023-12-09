using System.Net.Sockets;

public class MessagerClient : Client<MSGMatch> // link the Matchtype to the Clienttype
{
    // pass the Socket, programID, and partialClient into the base
    public MessagerClient(Socket socket, int programID, int partialClient):base(socket, programID, partialClient)
    {
        // set the handle methods
        Handles = new Dictionary<int, PacketScripts>()
        {
            { 0, _msg } // when packet with packettype 0 comes in the _msg function is called
        };
    }
    // Example method
    private void _msg(Packet packet, ProtocolType protocolType)
    {
        CurrentMatch.SendToAll(packet, protocolType);
    }
}