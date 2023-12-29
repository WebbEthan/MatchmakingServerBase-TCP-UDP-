using System.Net.Sockets;
public struct MatchInitializer
{
    public Type MatchType;
    public ClientDataStore HostClient;
    public string MatchCode;
}
public abstract class Match
{
    #region Data
    private Type _type;
    public string MatchCode;
    private ClientDataStore _hostClient;
    public Match(MatchInitializer initializer)
    {
        _type = initializer.MatchType;
        MatchCode = initializer.MatchCode;
        _hostClient = initializer.HostClient;
    }
    protected int MaxClients = 99;

    //List of client in the match and their refrences
    private Dictionary<string, ClientDataStore> _clients = new Dictionary<string, ClientDataStore>();
    public string[] GetClientIDs { get{ return _clients.Keys.ToArray(); } }
    public int ClientCount { get{ return _clients.Count + 1; }}
    #endregion
    #region Client Adding / Removing
    // Method for adding client
    public abstract bool TryClient(ClientDataStore client);
    // Checks that there is an available space for the client and add client to match, returns false if no space is available
    protected bool AddClient(ClientDataStore client)
    {
        if (_clients.Count < MaxClients - 1)
        {
            while (_clients.ContainsKey(client.MatchRefrenceForClient) || _hostClient.MatchRefrenceForClient == client.MatchRefrenceForClient)
            {
                client.MatchRefrenceForClient += "1";
            }
            using (Packet packet = new Packet(254))
            {
                packet.Write(client.MatchRefrenceForClient);
                SendToAll(packet, ProtocolType.Tcp);
            }
            _clients.Add(client.MatchRefrenceForClient, client);
            return true;
        }
        return false;
    }
    // Removes client from the match and sends that info to the other clients in match
    public void RemoveClient(string clientID, bool informClients = true)
    {
        if (clientID == _hostClient.MatchRefrenceForClient)
        {
            CloseMatch();
            return;
        }
        _clients.Remove(clientID);

        if (informClients)
        {
            using (Packet packet = new Packet(252))
            {
                packet.Write(clientID);
                SendToAll(packet, ProtocolType.Tcp);
            }
        }
    }
    public void KickClient(string clientID, string reason)
    {
        using (Packet packet = new Packet(251))
        {
            packet.Write(reason);
            _clients[clientID].SendData(packet, ProtocolType.Tcp);
        }
        RemoveClient(clientID, false);
    }
    public void CloseMatch()
    {
        while (_clients.Count > 0)
        {
            KickClient(_clients.Keys.First(), "Match Closed");
        }
        MatchMaker.DeleteMatch(_type, MatchCode);
    }
    #endregion
    #region Distributers
    // Sends data to the Host
    public void SendToHost(Packet packet, ProtocolType protocolType)
    {
        _hostClient.SendData(packet, protocolType);
    }
    // Distributes data to all clients
    public void SendToAll(Packet packet, ProtocolType protocolType)
    {
        _hostClient.SendData(packet, protocolType);
        foreach (ClientDataStore client in _clients.Values)
        {
            client.SendData(packet, protocolType);
        }
    }
    // Distributes data to all clients except the host
    public void SendToAllClients(Packet packet, ProtocolType protocolType)
    {
        foreach (ClientDataStore client in _clients.Values)
        {
            client.SendData(packet, protocolType);
        }
    }
    // Distributes data one client
    public void SentToClient(string reference, Packet packet, ProtocolType protocolType)
    {
        _clients[reference].SendData(packet, protocolType);
    }
    // Distributes data to all but one client and the host
    public void SendToAllClientsIgnored(string Ignored, Packet packet, ProtocolType protocolType)
    {
        foreach (string client in _clients.Keys)
        {
            if (client != Ignored)
            {
                _clients[client].SendData(packet, protocolType);
            }
        }
    }
    #endregion
}