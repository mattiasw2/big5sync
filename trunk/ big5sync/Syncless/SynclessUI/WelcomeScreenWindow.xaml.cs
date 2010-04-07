using System;
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
        private bool _closingAnimationNotCompleted = true;
        private MainWindow _main;

        public WelcomeScreenWindow(MainWindow main)
        {
            InitializeComponent();

            _main = main;

            try
            {
                this.ShowInTaskbar = false;
                this.Owner = main;
            }
            catch (InvalidOperationException)
            {
            }

            ChkBoxWelcomeOnStartup.IsChecked = Properties.Settings.Default.DisplayWelcomeScreen;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            Close();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            _closingAnimationNotCompleted = false;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closingAnimationNotCompleted)
            {
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        private void ChkBoxWelcomeOnStartup_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			Settings.Default.DisplayWelcomeScreen = (bool) this.ChkBoxWelcomeOnStartup.IsChecked;
            Settings.Default.Save();
        }
    }
}