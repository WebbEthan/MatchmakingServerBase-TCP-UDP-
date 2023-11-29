using System.Threading;
using System;
using System.Reflection.Metadata;
public static class ThreadManager
{
    private const int _tickrate = 20;
    public static bool ProgramActive;
    public static void StartThreads()
    {
        ProgramActive = true;
        _mainThreadReference.Start();
        ConsoleWriter.WriteLine($"Main Thread Started");
        _consoleThreadReference.Start();
    }
    public static void StopThreads()
    {
        ProgramActive = false;
    }
    // Everything from the network is moved to the Main Thread to prevent UDP packets from skipping
    #region  Main Thread
    private static Thread _mainThreadReference = new Thread(new ThreadStart(_mainThread));
    private static List<Action> _toExecuteOnMainThread = new List<Action>();
    public static List<Action> ExecuteOnMainThread { set { lock(_toExecuteOnMainThread) {_toExecuteOnMainThread.AddRange(value);} } }
    private static void _mainThread()
    {
        while (ProgramActive)
        {
            lock(_toExecuteOnMainThread)
            {
                while (_toExecuteOnMainThread.Count > 0)
                {
                    _toExecuteOnMainThread[0].Invoke();
                    _toExecuteOnMainThread.RemoveAt(0);
                }
            }
            Thread.Sleep(10);
        }
    }
    #endregion
    // Reasponsible for running console commands
    #region  Console Thread
    private static Thread _consoleThreadReference = new Thread(new ThreadStart(_consoleThread));
    private static void _consoleThread()
    {
        while (ProgramActive)
        {
            ConsoleWriter.ConsoleCommand(Console.ReadLine());
        }
    }
    #endregion
}