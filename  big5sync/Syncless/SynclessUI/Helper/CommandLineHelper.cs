using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace SynclessUI.Helper
{
    internal class CommandLineHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetLongPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder path,
            [MarshalAs(UnmanagedType.LPTStr)]
        StringBuilder longPath,
            int longPathLength
        );

        #region ProcessCommandLine

        public static void ProcessCommandLine(string[] commands, MainWindow main)
        {
            string flag = commands[0];
			
			// Get full path from array
			StringBuilder path = new StringBuilder(255);
			
			for(int i = 1; i < commands.Length; i++) {
				 path.Append(commands[i] + " ");
			}
			
            if (flag.Equals("-TFolder"))
            {
                //Shell Context Menu clicked for Folders ( Tag )
                StringBuilder longPath = new StringBuilder(255);
                GetLongPathName(path, longPath, longPath.Capacity);
				
                if(FileHelper.IsZipFile(longPath.ToString()))
                    return;

                main.CliTag(longPath.ToString());
            }
            else if (flag.Equals("-UTFolder"))
            {
                //Shell Context Menu clicked for Folders ( Untag )
                StringBuilder longPath = new StringBuilder(255);
                GetLongPathName(path, longPath, longPath.Capacity);

                if (FileHelper.IsZipFile(longPath.ToString()))
                    return;

                main.CliUntag(longPath.ToString());
            }
            else if (flag.Equals("-CleanMeta"))
            {
                StringBuilder longPath = new StringBuilder(255);
                GetLongPathName(path, longPath, longPath.Capacity);

                if (FileHelper.IsZipFile(longPath.ToString()))
                    return;

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
