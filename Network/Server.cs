using System.Net;
using System.Net.Sockets;
using System.Reflection;

using Network.threads;
using Network.data;

namespace Network
{
    public static class Server
    {
        private static int _startingPort = 25578;
        #region Data
        // This is a the reference to all of the clients on the server
        private static Dictionary<IPEndPoint, ClientDataStore> _clients = new Dictionary<IPEndPoint, ClientDataStore>();
        private static int _clientsConnected = 0;
        private static Dictionary<int, ClientDataStore> _partialClients = new Dictionary<int, ClientDataStore>();
        // Removes a client reference
        public static IPEndPoint DisconnectClient { set{ _clients.Remove(value); } }

        // A list of all possible client types
        private static List<Type> _programTypes = new List<Type>();
        // Obtains all the possible types of clients
        private static void _initializeData()
        {
            ConsoleWriter.WriteLine("Loading Scripts...");
            foreach (Type type in Assembly.GetAssembly(typeof(ClientDataStore)).GetTypes()
                .Where(Client => Client.IsClass && !Client.IsAbstract && Client.IsSubclassOf(typeof(ClientDataStore))))
            {
                _programTypes.Add(type);
            }
        }
        private static void _loadConfig()
        {
            try
            {
                string[] configData = File.ReadAllLines("Config.txt");
                
                ConsoleWriter.MainLogFile = bool.Parse(configData[0].Substring(0, configData[0].IndexOf(":") -1));
                _startingPort = int.Parse(configData[1].Substring(0, configData[1].IndexOf(":") -1));
                ConsoleWriter.OverrideLogFiles = bool.Parse(configData[4].Substring(0, configData[4].IndexOf(":") -1));
            }
            catch
            {
                ConsoleWriter.WriteLine("Unable To Load Config.", ConsoleColor.Red, false);
            }
        }
        #endregion
        #region Debug
        public static void DebugPacket(int partialClient)
        {
            using (Packet packet = new Packet(0))
            {
                packet.Write("This is a test packet.");
                _partialClients[partialClient].SendData(packet, ProtocolType.Tcp);
            }
        }
        public static void DebugPacket(IPEndPoint client)
        {
            using (Packet packet = new Packet(0))
            {
                packet.Write("This is a test packet.");
                _clients[client].SendData(packet, ProtocolType.Tcp);
            }
        }
        #endregion
        #region Methods
        // The main method that starts the server
        public static void StartServer()
        {
            // Starts the threads
            ThreadManager.StartThreads();
            // Loads the Config Settings
            Console.WriteLine("Loading Config");
            _loadConfig();
            // Initializes Data
            Console.WriteLine("Initializing please wait...");
            _initializeData();
            MatchMaker.InitializeData();

            ConsoleWriter.InitializeLogFiles();
            ConsoleWriter.WriteLine($"Data Initialized", ConsoleColor.DarkMagenta);
            // Opens the server
            _startListeners();
            ConsoleWriter.WriteLine("Server Succesfully Started.", ConsoleColor.DarkMagenta);
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
            foreach (ClientDataStore client in _clients.Values)
            {
                client.Disconnect();
            }
        }
        #endregion
        #region  Listeners
            // Starts the listeners
        private static void _startListeners()
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
                    _partialClients.Add(partialClient, Activator.CreateInstance(_programTypes[program], new object[] { new SocketData{ Socket = newTCPClient, ProgramID = program, PartialClient = partialClient}} ) as ClientDataStore);
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
            _serverUDPSockets[programID].BeginSend(packet.Data.ToArray(), packet.Data.Count, endPoint, null, null);
        }
    }
}
