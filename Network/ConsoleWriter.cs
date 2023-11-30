using System;
public static class ConsoleWriter
{
    // An easier method to add color
    public static void WriteLine(string msg, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(msg);
        Console.ForegroundColor = ConsoleColor.White;
    }
    public static void ConsoleCommand(string command)
    {
        string[] entrys = getWords(command);
        switch(entrys[0])
        {
            case "stop":
                ThreadManager.StopThreads();
                Server.StopServer();
                WriteLine("Server Closed", ConsoleColor.Green);
                break;
        }
    }
    // Split the input into separate words
    private static string[] getWords(string line, char separater = ' ')
    {
        List<string> words = new List<string>();
        while (line.Length > 0)
        {
            if (line.Contains(separater))
            {
                words.Add(line.Substring(0, line.IndexOf(separater)));
                line = line.Remove(0, line.IndexOf(separater)+1);
            }
            else
            {
                words.Add(line);
                break;
            }
        }
        return words.ToArray();
    }
}