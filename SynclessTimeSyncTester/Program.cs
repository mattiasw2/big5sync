using System;
using System.Diagnostics;

namespace SynclessTimeSyncTester
{
    public class Program
    {
        public static void Main()
        {
            Process timeSync = new Process();
            timeSync.StartInfo.FileName = "SynclessTimeSync.exe";
            timeSync.Start();
            timeSync.WaitForExit();
            Console.WriteLine(timeSync.ExitCode);
            Console.ReadKey();
        }
    }
}
