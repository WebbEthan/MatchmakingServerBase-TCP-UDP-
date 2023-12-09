This package allows anyone to create a server of any network typology
-----------------------------------------------------------------------
Settup
    Create a Matchtype --- (use the MSGMatch as an example)
        - create class the inhearites from Match
        - this class must contain a TryClient method this is where you decide if tha client joining the match joins
        - call AddClient to finish adding the client to the match
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
    Use the corolating package in your client usage is explained there.
-------------------------------------------------------------------------
Extra Info
    Client<>
        - Classes that inhearite from Client are where the incoming data needs to be prossesed.
        - Methods in the MatchType passed in can be accessed here.
    Match
        - Classes that inhearite from Match store a reference all the client in that match.
        - Contains methods for distributing data throughout match.
        - Classes that inhearite from Match can be used as a scriptable zone.
---------------------------------------------------------------------------
Console Commands
    Stop
        - Disconnects all clients, Stops the listeners and closes all sockets, Stops the server