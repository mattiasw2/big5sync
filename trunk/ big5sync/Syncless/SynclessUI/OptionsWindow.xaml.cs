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
        private bool _closingAnimationNotCompleted = true;

        public OptionsWindow()
        {
            InitializeComponent();
            InitializeOptions();
        }

        private void InitializeOptions()
        {
            ChkBoxRegistryIntegration.IsChecked = Settings.Default.EnableShellIntegration;
            ChkBoxMinimizeToTray.IsChecked = Settings.Default.MinimizeToTray;
            ChkBoxEnableAnimation.IsChecked = Settings.Default.EnableAnimation;
            ChkBoxEnableTrayNotification.IsChecked = Settings.Default.EnableTrayNotification;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            bool choice = ChkBoxRegistryIntegration.IsChecked.Value;

            if (choice)
            {
                var appPath = (string) Application.Current.Properties["AppPath"];

                RegistryHelper.CreateRegistry(appPath);
            }
            else
            {
                RegistryHelper.RemoveRegistry();
            }

            Close();
            Settings.Default.EnableShellIntegration = choice;
            Settings.Default.MinimizeToTray = (bool) ChkBoxMinimizeToTray.IsChecked;
            Settings.Default.EnableAnimation = (bool) ChkBoxEnableAnimation.IsChecked;
            Settings.Default.EnableTrayNotification = (bool) ChkBoxEnableTrayNotification.IsChecked;
            Settings.Default.Save();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
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