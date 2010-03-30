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

            CloseWindow();
            Settings.Default.EnableShellIntegration = choice;
            Settings.Default.MinimizeToTray = (bool) ChkBoxMinimizeToTray.IsChecked;
            Settings.Default.EnableAnimation = (bool) ChkBoxEnableAnimation.IsChecked;
            Settings.Default.EnableTrayNotification = (bool) ChkBoxEnableTrayNotification.IsChecked;
            Settings.Default.Save();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
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

        private void CloseWindow()
        {
            FormFadeOut.Begin();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            Close();
        }
    }
}