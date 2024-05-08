using System.Net.Sockets;
using Network.data;
public class RPSClient : Client<RPSMatch>
{
    public RPSClient(SocketData data):base(data)
    {
        Handles = new Dictionary<int, PacketScripts>() 
        {
            { 0, _msg },
            { 1, _submition }
        };
        WelcomeMSG = "Welcome to the Rock, Paper, Scissor server.";
    }
    private void _msg(Packet packet, ProtocolType protocolType)
    {
        CurrentMatch.SendToAll(packet, protocolType);
    }
    private void _submition(Packet packet, ProtocolType protocolType)
    {
        CurrentMatch.MakeGuess(packet, protocolType);
    }
}