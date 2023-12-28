public class MSGMatch : Match
{
    // pass the hostclient and the matchcode into the base
    public MSGMatch(ClientDataStore hostClient, string matchCode):base(typeof(MSGMatch), hostClient, matchCode)
    {
        
    }
    // create a tryclient method for handling how client can or can't join the match
    public override bool TryClient(ClientDataStore client)
    {
        AddClient(client);
        return true;
    }
}