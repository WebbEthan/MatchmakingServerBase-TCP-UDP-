public class MSGMatch : Match
{
    public MSGMatch(ClientDataStore hostClient, string matchCode):base(hostClient, matchCode)
    {
        
    }
    public override bool TryClient(ClientDataStore client)
    {
        AddClient(client);
        return true;
    }
}