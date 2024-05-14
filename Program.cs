using Network;
using Network.Automation;
internal class Program
{
    private static void Main(string[] args)
    {
        Console.Clear();


        // Creates a Network with the name MSGapp automatically
        Controller.CreateNetType<Star/*Type of network*/>("MSGapp"/*Name of network*/);

        // Reffer to the RPSMatch and RPSClient to see maunal inplamentation

        // Opens all the networks listeners
        Server.StartServer();
    }
}