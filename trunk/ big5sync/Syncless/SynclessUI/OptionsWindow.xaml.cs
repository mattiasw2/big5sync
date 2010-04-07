using System;
using System.Windows;
using System.Windows.Input;
using SynclessUI.Helper;
using SynclessUI.Properties;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private MainWindow _main;
        private bool _closingAnimationNotCompleted = true;

        public OptionsWindow(MainWindow main)
        {
            _main = main;
            InitializeComponent();
            InitializeOptions();

            Owner = _main;
            ShowInTaskbar = false;
        }

        private void InitializeOptions()
        {
            ChkBoxRegistryIntegration.IsChecked = Settings.Default.EnableShellIntegration;
            ChkBoxMinimizeToTray.IsChecked = Settings.Default.MinimizeToTray;
            ChkBoxEnableAnimation.IsChecked = Settings.Default.EnableAnimation;
            ChkBoxEnableTrayNotification.IsChecked = Settings.Default.EnableTrayNotification;
            ChkBoxEnableNotificationSounds.IsChecked = Settings.Default.EnableNotificationSounds;
			ChkBoxDisplayWelcomeScreen.IsChecked = Settings.Default.DisplayWelcomeScreen;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            bool shellIntegrationChoice = ChkBoxRegistryIntegration.IsChecked.Value;

            if (shellIntegrationChoice)
            {
                var appPath = (string) Application.Current.Properties["AppPath"];

                RegistryHelper.CreateRegistry(appPath);
            }
            else
            {
                RegistryHelper.RemoveRegistry();
            }

            Close();
            Settings.Default.EnableShellIntegration = shellIntegrationChoice;
            Settings.Default.MinimizeToTray = (bool) ChkBoxMinimizeToTray.IsChecked;
            Settings.Default.EnableAnimation = (bool) ChkBoxEnableAnimation.IsChecked;
            Settings.Default.EnableTrayNotification = (bool) ChkBoxEnableTrayNotification.IsChecked;
            Settings.Default.EnableNotificationSounds = (bool)ChkBoxEnableNotificationSounds.IsChecked;
			Settings.Default.DisplayWelcomeScreen = (bool) ChkBoxDisplayWelcomeScreen.IsChecked;
            Settings.Default.Save();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties["OptionsWindowIsOpened"] = true;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties["OptionsWindowIsOpened"] = false;
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
                BtnCancel.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }
    }
}