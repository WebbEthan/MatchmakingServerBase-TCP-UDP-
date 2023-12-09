using System.Net.Sockets;

public class MessagerClient : Client<MSGMatch>
{
    public MessagerClient(Socket socket, int programID, int partialClient):base(socket, programID, partialClient)
    {
        Handles = new Dictionary<int, PacketScripts>()
        {
            { 0, _msg }
        };
    }
    private void _msg(Packet packet, ProtocolType protocolType)
    {
        CurrentMatch.SendToAll(packet, protocolType);
    }
}