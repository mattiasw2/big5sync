using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynclessUI
{
    public class CommandLineHelper
    {

        #region ProcessCommandLine

        public static void ProcessCommandLine(List<string> commands)
        {
            string flag = commands[1];
            string path = commands[2];
            if (flag.Equals("-TFile"))
            {
                //Shell Context Menu clicked for Files ( Tag ) 
            }
            else if (flag.Equals("-TFolder"))
            {
                //Shell Context Menu clicked for Folders ( Tag )
            }
            else if (flag.Equals("-UTFolder"))
            {
                //Shell Context Menu clicked for Folders ( Untag )

            }
            else if (flag.Equals("-UTFile"))
            {
                //Shell Context Menu clicked for Files ( Untag )
            }
            else
            {
                throw new Exception("Unknown Command");
            }
        }

        #endregion
    }
}
