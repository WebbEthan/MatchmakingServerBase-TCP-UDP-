public class MSGMatch : Match
{
    public MSGMatch(Client hostClient, string matchCode):base(hostClient, matchCode)
    {
        
    }
    public override bool TryClient(Client client)
    {
        AddClient(client);
        return true;
    }
}