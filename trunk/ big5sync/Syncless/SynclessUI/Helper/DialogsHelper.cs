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
    }
}
