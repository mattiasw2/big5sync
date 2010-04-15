/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System.IO;
using System.Linq;

namespace SynclessUI.Helper
{
    /// <summary>
    /// Helper methods for the Command Line
    /// </summary>
    internal class CommandLineHelper
    {

        /// <summary>
        /// Credits of http://www.mail-archive.com/dotnet@discuss.develop.com/msg04537.html
        /// with modification to catch an arrayindexoutofbounds exception instead of 
        /// D:\LONGDI~1\LONGSU~1\LONGFI~1.TXT to long form
        /// </summary>
        /// <param name="path">Path Name</param>
        /// <returns>Long Path Name</returns>
        private static string GetLongPathName(string path)
        {
            string spath = path;

            string[] elm = spath.Split(
                new[]
                    {
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar
                    });
            string lpath = elm[0] + Path.DirectorySeparatorChar;
            for (int p = 1; p < elm.Length; p++)
            {
                string[] npath = Directory.GetFileSystemEntries(lpath, elm[p]);
                if (npath.Count() != 0)
                    lpath = npath[0];
            }

            if (lpath == "\\")
            {
                lpath = "";
            }

            return lpath;
        }

        /// <summary>
        /// Processes the arguments from the command line to break it into the command to execute and the path of the folder
        /// </summary>
        /// <param name="commands">Arguments from command line</param>
        /// <param name="main">Reference to MainWindow Class</param>
        public static void ProcessCommandLine(string[] commands, MainWindow main)
        {
            string flag = commands[0];

            // Get full path from command line
            string path = "";

            for (int i = 1; i < commands.Length; i++)
            {
                path += commands[i] + " ";
            }

            string longPath = GetLongPathName(path);

            if (flag.Equals("-TFolder"))
            {
                //Shell Context Menu clicked for Folders ( Tag )
                main.CliTag(longPath);
            }
            else if (flag.Equals("-UTFolder"))
            {
                //Shell Context Menu clicked for Folders ( Untag )
                main.CliUntag(longPath);
            }
            else if (flag.Equals("-CleanMeta"))
            {
                // Shell Context Menu clicked for Folders (Clean)
                main.CliClean(longPath);
            }
        }
    }
}