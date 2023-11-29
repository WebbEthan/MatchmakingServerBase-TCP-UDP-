using System.Reflection;
public static class MatchMaker
{
    // Initializes the dictionary for each type of match
    public static void InitializeData()
    {
        foreach (Type type in Assembly.GetAssembly(typeof(Match)).GetTypes()
            .Where(Match => Match.IsClass && !Match.IsAbstract && Match.IsSubclassOf(typeof(Match))))
        {
            _matches.Add(type, new Dictionary<string, Match>());
        }
    }
    private static Dictionary<Type, Dictionary<string, Match>> _matches = new Dictionary<Type, Dictionary<string, Match>>();
    // Handles the creation and joining of matches "-1" to create a match "0" for a random match
    public static bool RequestMatch(string match, Type type, Client client, ref Match matchData, out bool isHost)
    {
        isHost = false;
        switch (match)
        {
            case "0":
                for (int i = 0; i < _matches[type].Count; i++)
                {
                    if (_matches[type].ElementAt(i).Value.TryClient(client))
                    {
                        matchData = _matches[type].ElementAt(i).Value;
                        return true;
                    }
                }
                matchData = _createMatch(type, client);
                isHost = true;
                return true;
            case "-1":
                matchData = _createMatch(type, client);
                isHost = true;
                return true;
            default:
                if (_matches[type].ContainsKey(match))
                {
                    if (_matches[type][match].TryClient(client))
                    {
                        matchData = _matches[type][match];
                        return true;
                    }
                }
                break;
        }
        return false;
    }
    private const string _usableCodeCharaters = "abcdefghijklmnopqrstuvwxyz0123456789";
    private static Random _random = new Random();
    private static Match _createMatch(Type type, Client client)
    {
        // Generates new match code
        string matchCode = null;
        while (matchCode == null || _matches[type].ContainsKey(matchCode))
        {
            matchCode = new string(Enumerable.Repeat(_usableCodeCharaters, 5)
        .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
        // creates match
        Match newMatch = (Match)Activator.CreateInstance(type, new object[] { client, matchCode });
        _matches[type].Add(matchCode, newMatch);
        Console.WriteLine($"Created {type.Name} match with code : {matchCode}");
        return newMatch;
    }
}