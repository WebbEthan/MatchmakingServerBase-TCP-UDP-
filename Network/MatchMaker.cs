using System.Reflection;
public static class MatchMaker
{
    #region Data
    // Initializes the dictionary for each type of match
    public static void InitializeData()
    {
        foreach (Type type in Assembly.GetAssembly(typeof(Match)).GetTypes()
            .Where(Match => Match.IsClass && !Match.IsAbstract && Match.IsSubclassOf(typeof(Match))))
        {
            _matches.Add(type, new Dictionary<string, Match>());
            FillerMatcher.Add(type, (Match)Activator.CreateInstance(type));
        }
    }
    public static Dictionary<Type, Match> FillerMatcher = new Dictionary<Type, Match>();
    private static Dictionary<Type, Dictionary<string, Match>> _matches = new Dictionary<Type, Dictionary<string, Match>>();
    #endregion
    // Handles the creation and joining of matches "-1" to create a match "0" for a random match
    public static bool RequestMatch<matchType>(string match, Type type, ClientDataStore client, ref matchType matchData, out bool isHost) where matchType : Match
    {
        isHost = false;
        switch (match)
        {
            case "0":
                // Random Match
                for (int i = 0; i < _matches[type].Count; i++)
                {
                    if (_matches[type].ElementAt(i).Value.TryClient(client))
                    {
                        matchData = (matchType)_matches[type].ElementAt(i).Value;
                        return true;
                    }
                }
                matchData = _createMatch<matchType>(type, client);
                isHost = true;
                return true;
            case "-1":
                // New Match
                matchData = _createMatch<matchType>(type, client);
                isHost = true;
                return true;
            default:
                // Specific Match
                if (_matches[type].ContainsKey(match))
                {
                    if (_matches[type][match].TryClient(client))
                    {
                        matchData = (matchType)_matches[type][match];
                        return true;
                    }
                }
                break;
        }
        return false;
    }
    #region MatchCreation
    private const string _usableCodeCharaters = "abcdefghijklmnopqrstuvwxyz0123456789";
    private static Random _random = new Random();
    // crates a new match
    private static matchType _createMatch<matchType>(Type type, ClientDataStore client) where matchType : Match
    {
        // Generates new match code
        string matchCode = null;
        while (matchCode == null || _matches[type].ContainsKey(matchCode))
        {
            matchCode = new string(Enumerable.Repeat(_usableCodeCharaters, 5)
        .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
        // creates match
        matchType newMatch = (matchType)Activator.CreateInstance(type, new object[] { new MatchInitializer{ MatchType = type, HostClient = client, MatchCode = matchCode }});
        _matches[type].Add(matchCode, newMatch);
        Console.WriteLine($"Created {type.Name} match with code : {matchCode}");
        return newMatch;
    }
    #endregion
    public static void DeleteMatch(Type type, string matchCode)
    {
        _matches[type].Remove(matchCode);
        Console.WriteLine($"{type.Name} with code {matchCode} was closed.");
    }
}