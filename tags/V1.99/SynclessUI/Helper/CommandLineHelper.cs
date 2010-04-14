using System;
using System.IO;
using System.Linq;

namespace SynclessUI.Helper
{
    internal class CommandLineHelper
    {
        // Credits of http://www.mail-archive.com/dotnet@discuss.develop.com/msg04537.html

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

        #region ProcessCommandLine

        public static void ProcessCommandLine(string[] commands, MainWindow main)
        {
            string flag = commands[0];

            // Get full path from array
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
                main.CliClean(longPath);
            }
            else
            {
                throw new Exception("Unknown Command");
            }
        }

        #endregion
    }
}