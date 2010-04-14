/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

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
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete

        private SyncConfig _currentSyncConfig; // Current Synchronization Configuration object

        /// <summary>
        /// Initializes the WelcomeScreenWindow
        /// </summary>
        /// <param name="main">Reference to the Main Window</param>
        public OptionsWindow(MainWindow main)
        {
            _main = main;

            // Get Current Sync Config
            _currentSyncConfig = _main.LogicLayer.GetSyncConfig();

            InitializeComponent();
            InitializeSyncConfigComponents();
            InitializeComponentsFromSettings();

            // Sets up general window properties
            Owner = _main;
            ShowInTaskbar = false;
        }

        #region Initialization

        /// <summary>
        /// Initializes all components to the Current Sync Config and add events handlers to components
        /// </summary>
        private void InitializeSyncConfigComponents()
        {
            
            ChkBoxSendToRecycleBin.IsChecked = _currentSyncConfig.Recycle;
            
            // Sets up the archiving components
            LblChanges.Content = "" + _currentSyncConfig.ArchiveLimit;
            if (_currentSyncConfig.ArchiveLimit == 0)
            {
                ChkBoxMoveToSynclessArchive.IsChecked = false;
                SliderChanges.Visibility = Visibility.Hidden;
            }
            else
            {
                ChkBoxMoveToSynclessArchive.IsChecked = true;
                SliderChanges.Visibility = Visibility.Visible;
                SliderChanges.Value = _currentSyncConfig.ArchiveLimit;
            }

            SliderChanges.ValueChanged += SliderChanges_ValueChanged;
            ChkBoxMoveToSynclessArchive.Checked += ChkBoxMoveToSynclessArchive_Checked;
            ChkBoxMoveToSynclessArchive.Unchecked += ChkBoxMoveToSynclessArchive_Unchecked;
        }

        /// <summary>
        /// Initializes the Components From Application Settings
        /// </summary>
        private void InitializeComponentsFromSettings()
        {
            ChkBoxRegistryIntegration.IsChecked = Settings.Default.EnableShellIntegration;
            ChkBoxMinimizeToTray.IsChecked = Settings.Default.MinimizeToTray;
            ChkBoxEnableAnimation.IsChecked = Settings.Default.EnableAnimation;
            ChkBoxEnableTrayNotification.IsChecked = Settings.Default.EnableTrayNotification;
            ChkBoxEnableNotificationSounds.IsChecked = Settings.Default.EnableNotificationSounds;
            ChkBoxDisplayWelcomeScreen.IsChecked = Settings.Default.DisplayWelcomeScreen;
            ChkBoxSynchronizeTime.IsChecked = Settings.Default.SynchronizeTime;
        }

        #endregion

        #region General Window Components & Related Events

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

        #endregion

        #region Command Panel
        /// <summary>
        /// Event handler for BtnOk_Click event. Does all saving of application options to settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;

            // Updates the Sync Config
            _main.LogicLayer.UpdateSyncConfig(_currentSyncConfig);

            ReinitializeShellIntegration();

            SaveOptionsToSettings();

            Close();

        }

        /// <summary>
        /// Event handler for BtnCancel_Click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            Close();
        }

        /// <summary>
        /// Modifies registry based on shell integration choice
        /// </summary>
        private void ReinitializeShellIntegration()
        {
            bool shellIntegrationChoice = ChkBoxRegistryIntegration.IsChecked.Value;

            if (shellIntegrationChoice)
            {
                string appPath = (string)Application.Current.Properties["AppPath"];
                RegistryHelper.CreateRegistry(appPath);
            }
            else
            {
                RegistryHelper.RemoveRegistry();
            }
        }

        /// <summary>
        /// Saves all application options
        /// </summary>
        private void SaveOptionsToSettings()
        {
            Settings.Default.EnableShellIntegration = (bool) ChkBoxRegistryIntegration.IsChecked;
            Settings.Default.MinimizeToTray = (bool)ChkBoxMinimizeToTray.IsChecked;
            Settings.Default.EnableAnimation = (bool)ChkBoxEnableAnimation.IsChecked;
            Settings.Default.EnableTrayNotification = (bool)ChkBoxEnableTrayNotification.IsChecked;
            Settings.Default.EnableNotificationSounds = (bool)ChkBoxEnableNotificationSounds.IsChecked;
            Settings.Default.DisplayWelcomeScreen = (bool)ChkBoxDisplayWelcomeScreen.IsChecked;
            Settings.Default.SynchronizeTime = (bool)ChkBoxSynchronizeTime.IsChecked;
            Settings.Default.Save();
        }

        #endregion

        #region Single Instance
        /// <summary>
        /// Event handler for Window_Loaded. Ensures only one instance of options window is opened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties["OptionsWindowIsOpened"] = true;
        }

        /// <summary>
        /// Event handler for Window_Unloaded. Ensures only one instance of options window is opened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties["OptionsWindowIsOpened"] = false;
        }
        #endregion

        #region SyncConfig Components Event Handlers

        /// <summary>
        /// Event handler for ChkBoxMoveToSynclessArchive_Checked. Allows users to adjust SyncArchive Setting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBoxMoveToSynclessArchive_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            SliderChanges.Visibility = Visibility.Visible;
            LblChanges.Content = (int) SliderChanges.Value;
            _currentSyncConfig.ArchiveLimit = (int) SliderChanges.Value;
        }

        /// <summary>
        /// Event handler for ChkBoxMoveToSynclessArchive_Checked. Disallows users to adjust SyncArchive Setting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBoxMoveToSynclessArchive_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            SliderChanges.Visibility = Visibility.Hidden;
            _currentSyncConfig.ArchiveLimit = 0;
            LblChanges.Content = "0";
        }

        /// <summary>
        /// Event handler for SliderChanges_ValueChanged. If SliderChanges's value changes, adjust _currentSyncConfig.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderChanges_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (_currentSyncConfig != null)
            {
                LblChanges.Content = "" + SliderChanges.Value;
                _currentSyncConfig.ArchiveLimit = (int) SliderChanges.Value;
            }
        }

        /// <summary>
        /// Event handler for ChkBoxSendToRecycleBin_Click. Saves Recycle Bin setting to _currentSyncConfig.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBoxSendToRecycleBin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _currentSyncConfig.Recycle = (bool) ChkBoxSendToRecycleBin.IsChecked;
        }

        #endregion

        #region Tab Menu

        private void TabItemGeneral_PreviewMouseLeftButtonDown(object sender,
                                                               System.Windows.Input.MouseButtonEventArgs e)
        {
            TabItemDescription.Content = "General and User Interface Settings";
        }

        private void TabItemArchiving_PreviewMouseLeftButtonDown(object sender,
                                                                 System.Windows.Input.MouseButtonEventArgs e)
        {
            TabItemDescription.Content = "Archiving Settings";
        }

        #endregion
    }
}