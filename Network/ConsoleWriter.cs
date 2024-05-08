using System;
using System.Net;
using Microsoft.VisualBasic;

using Network.threads;
namespace Network
{
    public static class ConsoleWriter
    {
        public static bool MainLogFile = true;
        public static bool OverrideLogFiles = true;
        // Initializes Files For system loging
        public static void InitializeLogFiles()
        {
            // Creates Log Files and addresses
            WriteLine("Creating Log Files...");
            // Main Log File
            int logFilesCreated = 0;
            int dynamicLogFileCount = 0;
            if (MainLogFile)
            {

                logFilesCreated++;
            }
            MatchMaker.CreateLogFiles(ref logFilesCreated, ref dynamicLogFileCount);
            WriteLine($"Created {logFilesCreated} Log Files : {dynamicLogFileCount} Log Folders Created");
        }
        // An easier method to add color and writes output to a log file
        public static void WriteLine(string msg, ConsoleColor color = ConsoleColor.White, bool? writeToLog = null)
        {
            if (writeToLog == null)
            {
                writeToLog = MainLogFile;
            }
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
            if (writeToLog == true)
            {
                if (MainLogFile)
                {

                }
                else
                {
                    WriteLine($"Cannot write to log, MainLogFile is set to false", ConsoleColor.Red, false);
                }
            }
        }
        // Gives the capability to write to a separate log File Per Match
        public static void WriteLine(string msg, bool writeToLog, string logFile, bool outputToConsole = false)
        {
            if (outputToConsole)
            {
                WriteLine(msg, ConsoleColor.Gray, false);
            }
            if (writeToLog)
            {

            }
        }
        public static void ConsoleCommand(string? command)
        {
            string[] entrys = getWords(command);
            switch(entrys[0])
            {
                case "stop":
                    ThreadManager.StopThreads();
                    Server.StopServer();
                    WriteLine("Server Closed", ConsoleColor.Green);
                    break;
                case "test":
                    if (int.TryParse(entrys[1], out int partialClient))
                    {
                        Server.DebugPacket(partialClient);
                        WriteLine($"Send Debug Packet To Partial Client {entrys[1]}", ConsoleColor.Green);
                    }
                    else
                    {
                        Server.DebugPacket(IPEndPoint.Parse(entrys[1]));
                        WriteLine($"Send Debug Packet To Client at {entrys[1]}", ConsoleColor.Green);
                    }
                    break;
                default:
                    WriteLine("Unknown Command", ConsoleColor.Red);
                    break;
            }
        }
        // Split the input into separate words
        private static string[] getWords(string? line, char separater = ' ')
        {
            List<string> words = new List<string>();
            if (string.IsNullOrEmpty(line))
            {
                return words.ToArray();
            }
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
}