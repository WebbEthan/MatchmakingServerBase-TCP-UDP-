using System.Reflection;

using Network.data;
namespace Network
{
    public static class MatchMaker
    {
        #region Data
        // Initializes the dictionary for each type of match
        public static void InitializeData()
        {
            foreach (Type type in Assembly.GetAssembly(typeof(Match))!.GetTypes()
                .Where(Match => Match.IsClass && !Match.IsAbstract && Match.IsSubclassOf(typeof(Match))))
            {
                _matches.Add(type, new Dictionary<string, Match>());
                FillerMatcher.Add(type, (Match)Activator.CreateInstance(type, new MatchInitializer())!);
            }
        }
        public static void CreateLogFiles(ref int createdFiles, ref int createdAddresses)
        {

        }
        public static Dictionary<Type, Match> FillerMatcher = new Dictionary<Type, Match>();
        private static Dictionary<Type, Dictionary<string, Match>> _matches = new Dictionary<Type, Dictionary<string, Match>>();
        #endregion
        // Handles the creation and joining of matches "-1" to create a match "0" for a random match
        public static bool RequestMatch<matchType>(string match, ClientDataStore client, ref matchType matchData, out bool isHost) where matchType : Match
        {
            isHost = false;
            switch (match)
            {
                case "0":
                    // Random Match
                    for (int i = 0; i < _matches[typeof(matchType)].Count; i++)
                    {
                        if (_matches[typeof(matchType)].ElementAt(i).Value.TryClient(client))
                        {
                            matchData = (matchType)_matches[typeof(matchType)].ElementAt(i).Value;
                            return true;
                        }
                    }
                    matchData = _createMatch<matchType>(client);
                    isHost = true;
                    return true;
                case "-1":
                    // New Match
                    matchData = _createMatch<matchType>(client);
                    isHost = true;
                    return true;
                default:
                    // Specific Match
                    if (_matches[typeof(MatchType)].ContainsKey(match))
                    {
                        if (_matches[typeof(MatchType)][match].TryClient(client))
                        {
                            matchData = (matchType)_matches[typeof(MatchType)][match];
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
        private static matchType _createMatch<matchType>(ClientDataStore client) where matchType : Match
        {
            // Generates new match code
            string? matchCode = null;
            while (matchCode == null || _matches[typeof(matchType)].ContainsKey(matchCode))
            {
                matchCode = new string(Enumerable.Repeat(_usableCodeCharaters, 5)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
            }
            // creates match
            matchType newMatch = (matchType)Activator.CreateInstance(typeof(matchType), new MatchInitializer{ MatchType = typeof(matchType), HostClient = client, MatchCode = matchCode })!;
            _matches[typeof(matchType)].Add(matchCode, newMatch);
            Console.WriteLine($"Created match with code : {matchCode}");
            return newMatch;
        }
        #endregion
        public static void DeleteMatch(Type type, string matchCode)
        {
            _matches[type].Remove(matchCode);
            Console.WriteLine($"Match with code {matchCode} was closed.");
        }
    }
}
