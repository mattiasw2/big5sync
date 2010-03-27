using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SynclessUI.Helper
{
    public static class DialogsHelper
    {
        public static void ShowError(string caption, string message)
        {
            DialogWindow dw = new DialogWindow(caption, message, MessageBoxImage.Error);
            dw.ShowDialog();
        }

        public static void ShowInformation(string caption, string message)
        {
            DialogWindow dw = new DialogWindow(caption, message, MessageBoxImage.Information);
            dw.ShowDialog();
        }

        public static bool ShowWarning(string caption, string message)
        {
            DialogWindow dw = new DialogWindow(caption, message, MessageBoxImage.Warning);
            dw.ShowDialog();

            return (bool) Application.Current.Properties["DialogWindowChoice"];
        }

        public static void DisplayUnhandledExceptionMessage()
        {
            DialogsHelper.ShowError("Unexpected Error",
                                    "An unexpected error has occured. \n\nPlease help us by - \n 1. Submitting the debug.log in your Syncless Application Folder\\log to big5.syncless@gmail.com \n 2. Raise it as an issue on our GCPH @ http://code.google.com/p/big5sync/issues/list\n\n Please restart Syncless.");
        }
    }
}
