using System.Net;
using System.Net.Sockets;
// Used to allow scripting bettween match and client types
public abstract class Client<matchType> : ClientDataStore where matchType : Match
{
    protected matchType CurrentMatch;
    public bool IsHost = false;
    public Client(Socket socket, int programID, int partialClient):base(socket, programID, partialClient)
    {

    }
    protected override void HandleTCPData(byte[] data)
    {
        try
        {
            while(true)
            {
                using (Packet packet = new Packet(data))
                {
                    if (packet.PacketType == 255)
                    {
                        // Requests a match
                        string matchCode = packet.ReadString();
                        if (MatchMaker.RequestMatch<matchType>(matchCode, typeof(matchType), this, ref CurrentMatch, out IsHost))
                        {
                            // Returns data for match
                            using (Packet packet1 = new Packet(255))
                            {
                                packet1.Write(CurrentMatch.MatchCode);
                                packet1.Write(IsHost);
                                string[] matchData = CurrentMatch.GetClientIDs;
                                packet1.Write(matchData.Length);
                                foreach (string id in matchData)
                                {
                                    packet1.Write(id);
                                }
                                SendData(packet1, ProtocolType.Tcp);
                            }
                        }
                    }
                    else if (packet.PacketType == 252)
                    {
                        CurrentMatch.RemoveClient(MatchRefrenceForClient);
                    }
                    else
                    {
                        // Runs Handle
                        Handles[packet.PacketType](packet, ProtocolType.Tcp);
                    }
                    // Runs packets recieved in rececion
                    data = packet.data.GetRange(packet.PacketLength, packet.data.Count - packet.PacketLength).ToArray();
                    if (data.Length == 0)
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleWriter.WriteLine(ex.ToString(), ConsoleColor.Red);
        }
    }
}