using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Syncless.Core.Exceptions;
using SynclessUI.Helper;
using SynclessUI.Properties;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for WelcomeScreenWindow.xaml
    /// </summary>
    public partial class WelcomeScreenWindow : Window
    {
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete
        private MainWindow _main;

        /// <summary>
        /// Initializes the WelcomeScreenWindow
        /// </summary>
        /// <param name="main">Reference of the Main Window</param>
        public WelcomeScreenWindow(MainWindow main)
        {
            InitializeComponent();

            _main = main;

            // Sets up general window properties
            try
            {
                this.ShowInTaskbar = false;
                this.Owner = main;
            }
            catch (InvalidOperationException)
            {
            }

            // Initializes the Checkbox From Application Settings
            ChkBoxWelcomeOnStartup.IsChecked = Properties.Settings.Default.DisplayWelcomeScreen;
        }

        #region General Window Components & Related Events

        /// <summary>
        /// Event handler for BtnOk_Click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            Close();
        }

        /// <summary>
        /// Event handler for Canvas_MouseLeftButtonDown event. Allows user to drag the canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// On closing animation complete, set the boolean to false and closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            _closingAnimationNotCompleted = false;
            Close();
        }

        /// <summary>
        /// Event handler for Window_Closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // if animation is not completed, cancels the closing event and starts closing animation.
            if (_closingAnimationNotCompleted)
            {
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        /// <summary>
        /// Event handler for ChkBoxWelcomeOnStartup_Click. If user clicks, saves into Application Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBoxWelcomeOnStartup_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.DisplayWelcomeScreen = (bool) ChkBoxWelcomeOnStartup.IsChecked;
            Settings.Default.Save();
        }

        #endregion

        #region Web Page Links

        /// <summary>
        /// Event handler for LblAccurate_MouseLeftButtonDown. Opens the link in the user's browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblAccurate_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://code.google.com/p/big5sync/wiki/UserGuide#Time_Synchronization"));
        }


        /// <summary>
        /// Event handler for LblSeamless_MouseLeftButtonDown. Opens the link in the user's browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblSeamless_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(
                new ProcessStartInfo("http://code.google.com/p/big5sync/wiki/UserGuide#4._Switch_to_Seamless_Mode"));
        }

        /// <summary>
        /// Event handler for LblMultipleSync_MouseLeftButtonDown. Opens the link in the user's browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblMultipleSync_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://code.google.com/p/big5sync/wiki/UserGuide#N-way_Synchronization"));
        }

        /// <summary>
        /// Event handler for LblEasyTag_MouseLeftButtonDown. Opens the link in the user's browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblEasyTag_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://code.google.com/p/big5sync/wiki/UserGuide#1._Tag_Folders"));
        }

        #endregion
    }
}