using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynclessUI.Helper
{
    public static class DialogsHelper
    {
        public static void ShowError(string caption, string message)
        {
            ErrorWindow er = new ErrorWindow(caption, message);
            er.Show();
        }
    }
}
