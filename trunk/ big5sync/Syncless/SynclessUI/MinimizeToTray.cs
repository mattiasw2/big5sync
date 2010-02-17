using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;

namespace SynclessUI
{
/// <summary>
/// Class implementing support for "minimize to tray" functionality.
/// </summary>
public static class MinimizeToTray
{
    /// <summary>
    /// Enables "minimize to tray" behavior for the specified Window.
    /// </summary>
    /// <param name="window">Window to enable the behavior for.</param>
    public static void Enable(Window window)
    {
        // No need to track this instance; its event handlers will keep it alive
        new MinimizeToTrayInstance(window);
    }

    /// <summary>
    /// Class implementing "minimize to tray" functionality for a Window instance.
    /// </summary>
    private class MinimizeToTrayInstance
    {
        private Window _window;
        private NotifyIcon _notifyIcon;
        private bool _balloonShown;

        /// <summary>
        /// Initializes a new instance of the MinimizeToTrayInstance class.
        /// </summary>
        /// <param name="window">Window instance to attach to.</param>
        public MinimizeToTrayInstance(Window window)
        {
            Debug.Assert(window != null, "window parameter is null.");
            _window = window;
            _window.StateChanged += new EventHandler(HandleStateChanged);
        }

        /// <summary>
        /// Handles the Window's StateChanged event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleStateChanged(object sender, EventArgs e)
        {
            if (_notifyIcon == null)
            {
                // Initialize NotifyIcon instance "on demand"
                _notifyIcon = new NotifyIcon();
                _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                _notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(HandleNotifyIconOrBalloonClicked);
                _notifyIcon.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
            }
            // Update copy of Window Title in case it has changed
            _notifyIcon.Text = _window.Title;

            // Show/hide Window and NotifyIcon
            var minimized = (_window.WindowState == WindowState.Minimized);
            _window.ShowInTaskbar = !minimized;
            _notifyIcon.Visible = minimized;
            if (minimized && !_balloonShown)
            {
                // If this is the first time minimizing to the tray, show the user what happened
                _notifyIcon.ShowBalloonTip(1000, null, _window.Title + " has been minimized.", ToolTipIcon.None);
                _balloonShown = true;
            }
        }

        /// <summary>
        /// Handles a click on the notify icon or its balloon.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
        {
            // Restore the Window
            _window.WindowState = WindowState.Normal;
        }
    }
}
}