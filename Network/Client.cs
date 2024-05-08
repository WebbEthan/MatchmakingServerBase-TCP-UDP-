using System.Net.Sockets;

namespace Network.data
{
    public struct SocketData
    {
        public Socket Socket;
        public int ProgramID;
        public int PartialClient;
        public Match Match;
    }
    // Used to allow scripting bettween match and client types
    public abstract class Client<matchType> : ClientDataStore where matchType : Match
    {
        // The current match the client is in
        protected matchType CurrentMatch;
        public bool IsHost = false;
        public Client(SocketData data):base(data.Socket, data.ProgramID, data.PartialClient)
        {
            CurrentMatch = (matchType)MatchMaker.FillerMatcher[typeof(matchType)];
        }
        // Leaves match on disconnection
        public override void Disconnect()
        {
            if (CurrentMatch.MatchCode != null)
            {
                CurrentMatch.RemoveClient(MatchRefrenceForClient);
            }
            base.Disconnect();
        }
        // TCP Handling is pushed here to allow convertion from type Match to typeof matchType
        /*
            unchangeable packet ID
            (255)unsigned - 0xFF - (-1)signed. // Used to request matches and send the match data to a client joining a match.
            (254)unsigned - 0xFE - (-2)signed. // Send to client when a client joins the match they are in.
            (253)unsigned - 0xFD - (-3)signed. // The Callback when the client successfully authenticates.
            (252)unsigned - 0xFC - (-4)signed. // Used to Indicate when the client request to leave their current match, and send to inform client that a client has left their match.
            (251)unsigned - 0xFB - (-5)signed. // Used to Inform a client they have been kicked from a match
            NOT IMPEMENTED (250)unsigned - 0xFA - (-6)signed. // Used to read response times.
        */
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
}