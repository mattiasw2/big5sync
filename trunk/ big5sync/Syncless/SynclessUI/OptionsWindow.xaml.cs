using System;
using System.Windows;
using System.Windows.Input;
using Syncless.CompareAndSync;
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

        private SyncConfig _sc;

        public OptionsWindow(MainWindow main)
        {
            _main = main;


            InitializeComponent();
			// Get Sync Config
            _sc = _main.Gui.GetSyncConfig();
            
            InitializeOptions();
            ChkBoxSendToRecycleBin.IsChecked = _sc.Recycle;
            LblChanges.Content = "" + _sc.ArchiveLimit;

			if(_sc.ArchiveLimit == 0) {
				ChkBoxMoveToSynclessArchive.IsChecked = false;
        		SliderChanges.Visibility = Visibility.Hidden;
			} else {
				ChkBoxMoveToSynclessArchive.IsChecked = true;
        		SliderChanges.Visibility = Visibility.Visible;
                SliderChanges.Value = _sc.ArchiveLimit;
			}

            ChkBoxMoveToSynclessArchive.Checked += ChkBoxMoveToSynclessArchive_Checked;
            ChkBoxMoveToSynclessArchive.Unchecked += ChkBoxMoveToSynclessArchive_Unchecked;

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
            // Get Sync Config
            _main.Gui.UpdateSyncConfig(_sc);

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

        private void ChkBoxMoveToSynclessArchive_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
        	SliderChanges.Visibility = Visibility.Visible;
            LblChanges.Content = (int)SliderChanges.Value;
            _sc.ArchiveLimit = (int)SliderChanges.Value;
        }

        private void ChkBoxMoveToSynclessArchive_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
        	SliderChanges.Visibility = Visibility.Hidden;
            _sc.ArchiveLimit = 0;
            LblChanges.Content = "0";
        }

        private void SliderChanges_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if(_sc != null) {
        	    LblChanges.Content = "" + SliderChanges.Value;
                _sc.ArchiveLimit = (int) SliderChanges.Value;
            }
        }

        private void ChkBoxSendToRecycleBin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	_sc.Recycle = (bool) ChkBoxSendToRecycleBin.IsChecked;
        }
    }
}