using System.Net;
using System.Net.Sockets;
using System.Reflection;

public static class Server
{
    private const int _startingPort = 25578;
    // This is a the reference to all of the clients on the server
    private static Dictionary<IPEndPoint, Client> _clients = new Dictionary<IPEndPoint, Client>();
    private static int _clientsConnected = 0;
    private static Dictionary<int, Client> _partialClients = new Dictionary<int, Client>();
    // Removes a client reference
    public static IPEndPoint DisconnectClient { set{ _clients.Remove(value); } }

    // A list of all possible client types
    private static List<Type> _programTypes = new List<Type>();
    // Obtains all the possible types of clients
    public static void InitializeData()
    {
        foreach (Type type in Assembly.GetAssembly(typeof(Client)).GetTypes()
            .Where(Client => Client.IsClass && !Client.IsAbstract && Client.IsSubclassOf(typeof(Client))))
        {
            _programTypes.Add(type);
        }
    }

    // The main method that starts the server
    public static void StartListeners()
    {
        Console.WriteLine($"Starting listening on ports {_startingPort} through {_startingPort + _programTypes.Count - 1}");
        for (int i = 0; i < _programTypes.Count; i++)
        {
            _serverTCPListeners.Add(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
            _serverTCPListeners[i].Bind(new IPEndPoint(IPAddress.Any, _startingPort + i));
            _serverTCPListeners[i].Listen(5);
            _serverTCPListeners[i].BeginAccept(new AsyncCallback(_acceptTCPCallback), i);
            ConsoleWriter.WriteLine($"{_programTypes[i].Name} TCP started on port {_startingPort + i}", ConsoleColor.Cyan);
        }
        Console.WriteLine("All TCP started");
        for (int i = 0; i < _programTypes.Count; i++)
        {
            _serverUDPSockets.Add(new UdpClient(_startingPort + i));
            _serverUDPSockets[i].BeginReceive(_acceptUDPCallback, i);
            ConsoleWriter.WriteLine($"{_programTypes[i].Name} UDP started on port {_startingPort + i}", ConsoleColor.Cyan);
        }
        Console.WriteLine("All UDP started");
    }
    // Disconnects all clients and closes the server
    public static void StopServer()
    {
        // Stops Listeners
        foreach (Socket socket in _serverTCPListeners)
        {
            socket.Close();
        }
        foreach (UdpClient socket in _serverUDPSockets)
        {
            socket.Close();
        }
        // Dissconnects all clients
        foreach (Client client in _clients.Values)
        {
            client.Disconnect();
        }
    }
    #region  Listeners
    private static List<Socket> _serverTCPListeners = new List<Socket>();
    private static List<UdpClient> _serverUDPSockets = new List<UdpClient>();
    // Listens for new incoming TCP connections and creates to apropriate class for such
    private static void _acceptTCPCallback(IAsyncResult result)
    {
        if (ThreadManager.ProgramActive)
        {
            int program = (int)result.AsyncState;
            Socket newTCPClient = _serverTCPListeners[program].EndAccept(result);
            int partialClient = _clientsConnected++;
            Console.WriteLine($"{newTCPClient.RemoteEndPoint} Attempted to connect to the server as a/an {_programTypes[program].Name}...");
            if (newTCPClient.Connected)
            {
                _partialClients.Add(partialClient, (Client)Activator.CreateInstance(_programTypes[program], new object[] { newTCPClient, program, partialClient}) );
            }
            else
            {
                Console.WriteLine($"{newTCPClient.RemoteEndPoint} Socket was closed.");
            }
            _serverTCPListeners[program].BeginAccept(new AsyncCallback(_acceptTCPCallback), program);   
        }
    }
    // Handles all UDP data for the server
    private static void _acceptUDPCallback(IAsyncResult result)
    {
        if (ThreadManager.ProgramActive)
        {
            // Finds endpoint and program
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            int program = (int)result.AsyncState;
            // Gets data and starts listening for new data
            byte[] data = _serverUDPSockets[program].EndReceive(result, ref clientEndPoint);
            _serverUDPSockets[program].BeginReceive(_acceptUDPCallback, program);

            // Moves to main thread
            ThreadManager.ExecuteOnMainThread = new List<Action>() { () => 
            {
                try
                {
                    using (Packet packet = new Packet(data, false))
                    {
                        // Checks if client UDP is setup
                        if (_clients.ContainsKey(clientEndPoint))
                        {
                            // Runs client handle
                            _clients[clientEndPoint].Handles[packet.PacketType](packet, ProtocolType.Udp);
                        }
                        else
                        {
                            // Finishes setup
                            int partialClient = packet.ReadInt();
                            _partialClients[partialClient].SetupUDP(clientEndPoint);
                            _clients.Add(clientEndPoint, _partialClients[partialClient]);
                            _partialClients.Remove(partialClient);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWriter.WriteLine(ex.ToString(), ConsoleColor.Red);
                }
            }};
        }
    }
    #endregion
    public static void SendUDPData(Packet packet, int programID, IPEndPoint endPoint)
    {
        _serverUDPSockets[programID].BeginSend(packet.data.ToArray(), packet.data.Count, endPoint, null, null);
    }
}