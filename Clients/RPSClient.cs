using System.Net.Sockets;
using System.Reflection.Metadata;

public class RPSClient : Client<RPSMatch>
{
    protected override Type MatchType { get; set; } = typeof(RPSMatch);
    public RPSClient(Socket socket, int programID, int parialClient):base(socket, programID, parialClient)
    {
        Handles = new Dictionary<int, _packetScripts>() 
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