using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace SynclessUI.Helper
{
    internal class CommandLineHelper
    {
        /*
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetLongPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder path,
            [MarshalAs(UnmanagedType.LPTStr)]
        StringBuilder longPath,
            int longPathLength
        );
        */

        // Credits of http://www.mail-archive.com/dotnet@discuss.develop.com/msg04537.html

        private static string GetLongPathName(string path)
        {
            string spath = path;

            string[] elm = spath.Split(
                new char[]
                    {
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar
                    });
            string lpath = elm[0] + Path.DirectorySeparatorChar;
            for (int p = 1; p < elm.Length; p++)
            {
                string[] npath = Directory.GetFileSystemEntries(lpath, elm[p]);
                if(npath.Count() != 0)
                    lpath = npath[0];
            }

            return lpath;
        }

        #region ProcessCommandLine

        public static void ProcessCommandLine(string[] commands, MainWindow main)
        {
            string flag = commands[0];
			
			// Get full path from array
            string path = "";
			
			for(int i = 1; i < commands.Length; i++) {
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
                main.CliClean(longPath.ToString());
            }
            else
            {
                throw new Exception("Unknown Command");
            }
        }

        #endregion
    }
}
