using System.Net.Sockets;

public class MessagerClient : Client<MSGMatch>
{
    protected override Type MatchType { get; set; } = typeof(MSGMatch);
    public MessagerClient(Socket socket, int programID, int partialClient):base(socket, programID, partialClient)
    {
        Handles = new Dictionary<int, _packetScripts>()
        {
            { 0, _msg }
        };
        WelcomeMSG = "Welcome to the Messaging server";
    }
    private void _msg(Packet packet, ProtocolType type)
    {
        CurrentMatch.SendToAll(packet, type);
    }
}