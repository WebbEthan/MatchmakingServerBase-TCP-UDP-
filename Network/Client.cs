using System.Net.Sockets;
// Used to allow scripting bettween match and client types
public abstract class Client<matchType> : ClientDataStore where matchType : Match
{
    // The current match the client is in
    protected matchType CurrentMatch;
    public bool IsHost = false;
    public Client(Socket socket, int programID, int partialClient):base(socket, programID, partialClient)
    {

    }
    // Leaves match on disconnection
    public override void Disconnect()
    {
        if (CurrentMatch != null)
        {
            CurrentMatch.RemoveClient(MatchRefrenceForClient);
        }
        base.Disconnect();
    }
    // TCP Handling is pushed here to allow convertion from type Match to typeof matchType
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
                        MatchRefrenceForClient = packet.ReadString();
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
                    if (packet.Packaged)
                    {
                        data = packet.Data.GetRange(packet.PacketLength + 1, packet.Data.Count - packet.PacketLength - 1).ToArray();
                    }
                    else
                    {
                        data = packet.Data.GetRange(packet.PacketLength, packet.Data.Count - packet.PacketLength).ToArray();
                    }
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