using System.Net;
using System.Net.Sockets;
// Used to get a consistant method for storing the class across the server with different datatypes in client<>
/*
1. TCPSocket is passed into the TCP class
2. Packet is sent (WelcomeMSG, parialClient)
3. Client then needs to respond via UDP with a packet containing (partialClient)
4. Endpoint is passed into the UDP class and the client is connected
*/
public abstract class ClientDataStore
{
    // A Reference to the UDP socket on the server
    private int _programID;
    // What the id of the clinet is in a match
    public string MatchRefrenceForClient = "";
    // Mesage sent durring authentication
    protected string WelcomeMSG = "Welcome to the server";
    public ClientDataStore(Socket socket, int programID, int partialClient)
    {
        _programID = programID;
        _tcpProtocal = new _tcp(socket, this, partialClient);
    }
    public delegate void _packetScripts(Packet packet, ProtocolType protocolType);
        // The store of methods defined by each client type
    public Dictionary<int, _packetScripts> Handles;
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
    protected abstract void HandleTCPData(byte[] data);
    private _tcp _tcpProtocal;
    private _udp _udpProtocal;
    private class _tcp
    {
        // Prevents data being recieved during Close_Wait
        private bool _active = true;
        private ClientDataStore _refrence;
        private Socket _socket;
        private byte[] _buffer = new byte[4096];
        public _tcp(Socket socket, ClientDataStore reference, int partialClient)
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
                        _refrence.HandleTCPData(data);
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
        private ClientDataStore _reference;
        private IPEndPoint _udpEndpoint;
        public _udp(IPEndPoint endPoint, ClientDataStore reference)
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