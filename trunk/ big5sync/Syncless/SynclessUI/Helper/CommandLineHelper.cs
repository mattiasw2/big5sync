using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SynclessUI.Helper
{
    public class CommandLineHelper
    {
        #region ProcessCommandLine

        public static void ProcessCommandLine(List<string> commands, MainWindow main)
        {
            string flag = commands[1];
            string path = commands[2];
            if (flag.Equals("-TFile"))
            {
                //Shell Context Menu clicked for Files ( Tag )
                main.CLI_CreateTag(path);
            }
            else if (flag.Equals("-TFolder"))
            {
                //Shell Context Menu clicked for Folders ( Tag )
                main.CLI_CreateTag(path);
            }
            else if (flag.Equals("-UTFolder"))
            {
                //Shell Context Menu clicked for Folders ( Untag )
                // main.CLI_Untag("file", path);
            }
            else if (flag.Equals("-UTFile"))
            {
                //Shell Context Menu clicked for Files ( Untag )
                // main.CLI_Untag("folder", path);
            }
            else
            {
                throw new Exception("Unknown Command");
            }
        }

        #endregion
    }
}
