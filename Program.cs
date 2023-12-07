Console.Clear();
// Starts the threads
ThreadManager.StartThreads();
// Initializes Data
Console.WriteLine("Initializing please wait...");
Server.InitializeData();
MatchMaker.InitializeData();
Console.WriteLine($"Data Initialized");
// Opens the server
Server.StartListeners();
