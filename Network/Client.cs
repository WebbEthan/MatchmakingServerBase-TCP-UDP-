using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
/*
1. TCPSocket is passed into the TCP class
2. Packet is sent (WelcomeMSG, parialClient)
3. Client then needs to respond via UDP with a packet containing (partialClient)
4. Endpoint is passed into the UDP class and the client is connected
*/

public abstract class Client
{
    protected Match CurrentMatch;
    public string MatchRefrenceForClient = "";
    public bool IsHost = false;
    // They Type of match this client can go into
    protected abstract Type MatchType { get; set; }
    private int _programID;
    // Mesage sent durring authentication
    protected string WelcomeMSG = "Welcome to the server";
    public Client(Socket socket, int programID, int partialClient)
    {
        _programID = programID;
        _tcpProtocal = new _tcp(socket, this, partialClient);
    }
    // Secondary connect call for athenticating data and setting up UDP
    public void SetupUDP(IPEndPoint endPoint)
    {
        _udpProtocal = new _udp(endPoint, this);
        // Authentication callback
        using (Packet packet = new Packet(253))
        {
            SendData(packet, ProtocolType.Tcp);
        }
        Console.WriteLine($"Client successfully connected");
    }
    public delegate void _packetScripts(Packet packet, ProtocolType protocolType);
    // The store of methods defined by each client type
    public Dictionary<int, _packetScripts> Handles;
    public void SendData(Packet packet, ProtocolType protocolType)
    {
        if (!packet.Packaged)
        {
            packet.PrepForSending();
        }
        if (protocolType == ProtocolType.Tcp)
        {
            _tcpProtocal.SendData(packet);
        }
        else
        {
            _udpProtocal.SendData(packet);
        }
    }
    #region  Sockets
    public void Kick()
    {
        _tcpProtocal.SendData(new Packet(0));
        Disconnect();
    }
    public void Disconnect()
    {
        _tcpProtocal.Disconnect();
        _udpProtocal.Disconnect();
        Console.WriteLine("Client disconnected");
    }
    private _tcp _tcpProtocal;
    private _udp _udpProtocal;
    private class _tcp
    {
        // Prevents data being recieved during Close_Wait
        private bool _active = true;
        private Client _refrence;
        private Socket _socket;
        private byte[] _buffer = new byte[4096];
        public _tcp(Socket socket, Client reference, int partialClient)
        {
            _refrence = reference;
            _socket = socket;
            // Begins listening for data
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(_handleData), null);
            using(Packet packet = new Packet(0))
            {
                packet.Write(_refrence.WelcomeMSG);
                packet.Write(partialClient);
                packet.PrepForSending();
                SendData(packet);
            }
        }
        
        public void SendData(Packet packet)
        {
            _socket.BeginSend(packet.data.ToArray(), 0, packet.data.Count, SocketFlags.None, new AsyncCallback(_endSend), null);
        }
        private void _endSend(IAsyncResult result) { _socket.EndSend(result); }
        private void _handleData(IAsyncResult result)
        {
            if (_active)
            {
                // Checks that the socket is still connected
                if (!_socket.Connected)
                {
                    _refrence.Disconnect();
                    return;
                }
                // Accepts incoming data
                int recievedLength = _socket.EndReceive(result);
                // ensures data is within the set byte limit
                if (recievedLength > _buffer.Length)
                {
                    _refrence.Disconnect();
                }
                if (recievedLength > 0)
                {
                    // Obtains data from the buffer
                    byte[] data = new byte[recievedLength];
                    Array.Copy(_buffer, data, recievedLength);
                    // Executes handle code
                    ThreadManager.ExecuteOnMainThread = new List<Action>() {() => 
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
                                        if (MatchMaker.RequestMatch(matchCode, _refrence.MatchType, _refrence, ref _refrence.CurrentMatch, out _refrence.IsHost))
                                        {
                                            // Returns data for match
                                            using (Packet packet1 = new Packet(255))
                                            {
                                                packet1.Write(_refrence.CurrentMatch.MatchCode);
                                                packet1.Write(_refrence.IsHost);
                                                string[] matchData = _refrence.CurrentMatch.GetClientIDs;
                                                packet1.Write(matchData.Length);
                                                foreach (string id in matchData)
                                                {
                                                    packet1.Write(id);
                                                }
                                                packet1.PrepForSending();
                                                SendData(packet1);
                                            }
                                        }
                                    }
                                    else if (packet.PacketType == 252)
                                    {
                                        _refrence.CurrentMatch.RemoveClient(_refrence.MatchRefrenceForClient);
                                    }
                                    else
                                    {
                                        // Runs Handle
                                        _refrence.Handles[packet.PacketType](packet, ProtocolType.Tcp);
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
                    }};
                    // Begins listening for more data
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(_handleData), null);
                }
                else
                {
                    _refrence.Disconnect();
                }
            }
        }
        public void Disconnect()
        {
            _active = false;
            _socket.Close();
        }
    }
    private class _udp
    {
        private Client _reference;
        private IPEndPoint _udpEndpoint;
        public _udp(IPEndPoint endPoint, Client reference)
        {
            _reference = reference;
            _udpEndpoint = endPoint;
        }
        public void SendData(Packet packet)
        {
            Server.SendUDPData(packet, _reference._programID, _udpEndpoint);
        }
        public void Disconnect()
        {
            Server.DisconnectClient = _udpEndpoint;
        }
    }
    #endregion
}