/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System.Windows;

namespace SynclessUI.Helper
{
    public static class DialogHelper
    {
        public static void ShowError(Window window, string caption, string message)
        {
            DialogWindow dw = new DialogWindow(window, caption, message, DialogType.Error);
            dw.ShowDialog();
        }

        public static void ShowInformation(Window window, string caption, string message)
        {
            DialogWindow dw = new DialogWindow(window, caption, message, DialogType.Information);
            dw.ShowDialog();
        }

        public static bool ShowWarning(Window window, string caption, string message)
        {
            DialogWindow dw = new DialogWindow(window, caption, message, DialogType.Warning);
            dw.ShowDialog();

            if(Application.Current == null)
                return false;

            return (bool)Application.Current.Properties["DialogWindowChoice"];
            
        }

        public static DialogWindow ShowIndeterminate(Window window, string caption, string message)
        {
            DialogWindow dw = new DialogWindow(window, caption, message, DialogType.Indeterminate);
            return dw;
        }

        public static void DisplayUnhandledExceptionMessage(Window window)
        {
            ShowError(window, "Unexpected Error",
                      "An unexpected error has occured. \n\nPlease help us by - \n 1. Submitting the debug.log in your Syncless Application Folder\\log to big5.syncless@gmail.com \n 2. Raise it as an issue on our GCPH @ http://code.google.com/p/big5sync/issues/list\n\n Please restart Syncless.");
        }
    }
}