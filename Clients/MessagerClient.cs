using System.Net.Sockets;

public class MessagerClient : Client<MSGMatch>
{
    public MessagerClient(Socket socket, int programID, int partialClient):base(socket, programID, partialClient)
    {
        Handles = new Dictionary<int, PacketScripts>()
        {
            { 0, CurrentMatch.SendToAll }
        };
        WelcomeMSG = "Welcome to the Messaging server";
    }
    
}