using System.Net.Sockets;

public class RPSMatch : Match
{
    public RPSMatch(MatchInitializer initializer):base(initializer)
    {
        MaxClients = 2;
    }
    public override bool TryClient(ClientDataStore client)
    {
        if (ClientCount < 2)
        {
            AddClient(client);
            _startGame();
            return true;
        }
        return false;
    }
    private void _startGame()
    {
        using (Packet packet = new Packet(1))
        {
            SendToAll(packet, ProtocolType.Tcp);
        }
    }
    private enum option { rock, paper, scissors }
    private string? _guesser;
    private option? _savedGuess;
    public void MakeGuess(Packet packet, ProtocolType type)
    {
        string guesser = packet.ReadString();
        option guess = (option)packet.ReadByte();
        if (_savedGuess == null)
        {
            _guesser = guesser;
            _savedGuess = guess;
        }
        else
        {
            using (Packet packet1 = new Packet(2))
            {
                if (_savedGuess == guess)
                {
                    // Draw
                    packet1.Write((byte)0);
                }
                else if ((byte)_savedGuess + 1 == (byte)guess || ((byte)_savedGuess + 1 == 4 && (byte)guess == 0))
                {
                    // Lose
                    packet1.Write((byte)1);
                    packet.Write(guesser);
                }
                else
                {
                    // Win
                    packet1.Write((byte)1);   
                    packet.Write(_guesser);
                }
                SendToAll(packet1, ProtocolType.Tcp);
            }
            _savedGuess = null;
            _guesser = null;
        }
    }
}