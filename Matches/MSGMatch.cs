public class MSGMatch : Match
{
    public MSGMatch(Client<Match> hostClient, string matchCode):base(hostClient, matchCode)
    {
        
    }
    public override bool TryClient(Client<Match> client)
    {
        AddClient(client);
        return true;
    }
}