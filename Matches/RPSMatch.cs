public class RPSMatch : Match
{
    public RPSMatch(Client hostClient, string matchCode):base(hostClient, matchCode)
    {

    }
    public override bool TryClient(Client client)
    {
        if (ClientCount < 2)
        {
            AddClient(client);
            return true;
        }
        return false;
    }
    private enum option { rock, paper, scissors }
    private string? _guesser;
    private option? _savedGuess;
    public void MakeGuess(Packet packet, string guesser)
    {
        option guess = (option)packet.ReadByte();
        if (_savedGuess == null)
        {
            _guesser = guesser;
            _savedGuess = guess;
        }
        else
        {
            if (_savedGuess == guess)
            {
                // draw
            }
            if ((byte)_savedGuess + 1 == (byte)guess || ((byte)_savedGuess + 1 == 4 && (byte)guess == 0))
            {
                // lose
            }
            else
            {
                // win
            }
            _savedGuess = null;
            _guesser = null;
        }
    }
}