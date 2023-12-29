public class MSGMatch : Match
{
    // pass the hostclient and the matchcode into the base
    public MSGMatch(MatchInitializer initializer):base(initializer)
    {
        
    }
    // create a tryclient method for handling how client can or can't join the match
    public override bool TryClient(ClientDataStore client)
    {
        return AddClient(client);
    }
}