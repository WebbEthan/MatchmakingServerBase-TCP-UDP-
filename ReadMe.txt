This package allows anyone to create a server of any network typology for as many programs as desired
-----------------------------------------------------------------------------------------------------
Settup
    Create a Matchtype --- (use the MSGMatch as an example)
        - create class the inhearites from Match passing a Match Initializer into the base
    Create a clienttype --- (use the MessagerClient as an example)
        - create a class that inhearites from Client<Matchtype>
        - within the constructor set the Handles dictionary
            * Methods in the Handle must have the paramiters(Packet, ProtocalType)
    Call Server.StartServer(); to start the server
-----------------------------------------------------------------------
Sending Data
    Creating Packets
        - Create a new instance of Packet with a byte passed in
            * initial byte corolates to what method set on the recieving end will be run.
        - use the Write method to pass values into the packet
    Distributing Packets;
        Client
            - using the reference to the client call SendData passing in the Packet and sent the ProtocalType to TCP or UDP.
        Matches
            - SendToHost
                * Sends Packet to client that initiated the match creation
            - SendToAll
                * Sends Packet to every client in the match
            - SendToAllClients
                * Sends Packet to every client in match ignoring the host client
            - SendToClient
                * Send Packet to specific client via the username
            - SentToAllClientsIgnored
                * Sends Packet to all clients ignoring the host client and the client with the username passed in
-------------------------------------------------------------------------
Conneting to the server
    Use the corolating package and your client usage is explained there.
-------------------------------------------------------------------------
Extra Info
    Client<delatgate> 
        - The delagate must be the associated matchtype.
        - Classes that inhearite from Client are where the incoming data needs to be prossesed.
        - Methods in the MatchType passed in can be accessed here.
        Methods
            - CurrentMatch, Is a reference to the match the client is in and you can use this variable to run functions in the specific match class you created.
            - WelcomeMSG, Is a message sent to the client while waiting for authentication.
            - Disconnect(), Disconnects the client and destroys the class containing the reference to them.
    Match
        - Classes that inhearite from Match store a reference all the client in that match.
        - Contains methods for distributing data throughout match.
        - Classes that inhearite from Match can be used as a scriptable zone.
        Methods
            - MaxClients, defualt is 99 for 100 player match.
            - TryClient(ClientDataStore), Can be used to create a custom authentication for the match, defualt retrun AddClient() method.
            - AddClient(ClientDataStore), adds client to match if possible.
            - KickClient(username, reason), Kicks a client from the match.
    Packet ID
        - Do Not Use Values [0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA] For your packet ID these are used by the sever for special functions.
---------------------------------------------------------------------------
Console Commands
    Stop
        - Disconnects all clients, Stops the listeners and closes all sockets, Stops the server


-------------------------
Planned Updates
    Ping and network timeout detection
    File logging
    Main config file
    Meathod for custom client OnConnectToServerAuthentication
