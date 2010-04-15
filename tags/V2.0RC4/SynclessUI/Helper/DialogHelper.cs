/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System.Windows;

namespace SynclessUI.Helper
{
    /// <summary>
    /// Helper class to generate Various Types of Customized Dialog Windows with Various Settings
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Generates a customized Error Dialog Box
        /// </summary>
        /// <param name="window">Parent Window</param>
        /// <param name="caption">Caption of the window</param>
        /// <param name="message">Message of the window</param>
        public static void ShowError(Window window, string caption, string message)
        {
            DialogWindow dw = new DialogWindow(window, caption, message, DialogType.Error);
            dw.ShowDialog();
        }

        /// <summary>
        /// Generates a customized Information Dialog Box
        /// </summary>
        /// <param name="window">Parent Window</param>
        /// <param name="caption">Caption of the window</param>
        /// <param name="message">Message of the window</param>
        public static void ShowInformation(Window window, string caption, string message)
        {
            DialogWindow dw = new DialogWindow(window, caption, message, DialogType.Information);
            dw.ShowDialog();
        }

        /// <summary>
        /// Generates a customized Warning Dialog Box
        /// </summary>
        /// <param name="window">Parent Window</param>
        /// <param name="caption">Caption of the window</param>
        /// <param name="message">Message of the window</param>
        /// <returns>The choice of the user, which is recorded through the Application Current Properties</returns>
        public static bool ShowWarning(Window window, string caption, string message)
        {
            DialogWindow dw = new DialogWindow(window, caption, message, DialogType.Warning);
            dw.ShowDialog();

            if(Application.Current == null)
                return false;

            return (bool)Application.Current.Properties["DialogWindowChoice"];
            
        }

        /// <summary>
        /// Generates an unclosable DialogBox with an indeterminate progress bar. Eg. TerminationWindow
        /// </summary>
        /// <param name="window">Parent Window</param>
        /// <param name="caption">Caption of the window</param>
        /// <param name="message">Message of the window</param>
        /// <returns>The dialog window itself</returns>
        public static DialogWindow ShowIndeterminate(Window window, string caption, string message)
        {
            DialogWindow dw = new DialogWindow(window, caption, message, DialogType.Indeterminate);
            return dw;
        }

        /// <summary>
        /// Generates a general UnhandledExceptionMessage if called upon
        /// </summary>
        /// <param name="window">Parent Window</param>
        public static void DisplayUnhandledExceptionMessage(Window window)
        {
            ShowError(window, "Unexpected Error",
                      "An unexpected error has occured. \n\nPlease help us by - \n 1. Submitting the debug.log in your Syncless Application Folder\\log to big5.syncless@gmail.com \n 2. Raise it as an issue on our GCPH @ http://code.google.com/p/big5sync/issues/list\n\n Please restart Syncless.");
        }
    }
}