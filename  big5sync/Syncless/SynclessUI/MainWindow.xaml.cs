/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Syncless.Core;
using Syncless.Core.Exceptions;
using Syncless.Core.View;
using Syncless.Helper;
using Syncless.Notification;
using Syncless.Tagging.Exceptions;
using SynclessUI.Helper;
using SynclessUI.Notification;
using SynclessUI.Properties;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUIInterface
    {
        /// <summary>
        /// Path of the Application
        /// </summary>
        private string _appPath; 
        /// <summary>
        /// Status if Syncless has closed normally. Set to false when Syncless fails to initialize on startup
        /// </summary>
        private bool _closednormally = true;
        /// <summary>
        /// Status to check if Manual Sync Operations is enabled
        /// </summary>
        private bool _manualSyncEnabled = true;

        /// <summary>
        /// Notification Watcher
        /// </summary>
        private NotificationWatcher _notificationWatcher;
        
        /// <summary>
        /// Priority Notification Watcher
        /// </summary>
        private PriorityNotificationWatcher _priorityNotificationWatcher; 

        /// <summary>
        /// Dictionary to store the sync progress percentage of every tag in use
        /// </summary>
        private Dictionary<string, double> _syncProgressNotificationDictionary =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Dictionary to store the tag status of every tag in use
        /// </summary>
        private Dictionary<string, string> _tagStatusNotificationDictionary =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Reference to the SystemLogicLayer whose subset of functionality is exposed to the User Interace.
        /// </summary>
        public IUIControllerInterface LogicLayer;

        /// <summary>
        /// Current SyncProgress for keeping track of synchronizations
        /// </summary>
        public SyncProgress CurrentProgress { get; set; }

        /// <summary>
        /// Gets/Sets the Current Tag on the TagList Panel. This is stored in the current application properties
        /// </summary>
        public string SelectedTag
        {
            get { return (string)Application.Current.Properties["SelectedTag"]; }
            private set { Application.Current.Properties["SelectedTag"] = value; }
        }

        /// <summary>
        /// Set of all tags that are in cancelling mode
        /// </summary>
        private HashSet<string> CancellingTags { get; set; }

        /// <summary>
        /// Gets/Sets the Current Tag Filter.
        /// </summary>
        private string TagFilter
        {
            get { return TxtBoxFilterTag.Text.Trim(); }
            set { TxtBoxFilterTag.Text = value; }
        }

        /// <summary>
        /// Initializes the MainWindow
        /// </summary>
        public MainWindow()
        {
            CancellingTags = new HashSet<string>();
            InitializeComponent();
            InitializeSyncless();
            InitializeAllCommands();

            if (Settings.Default.MinimizeOnStartup)
            {
                MinimizeWindow();
            }

        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #region Initializing Syncless

        /// <summary>
        /// Starts up the system logic layer and initializes it
        /// </summary>
        private void InitializeSyncless()
        {
            try
            {
                CancellingTags = new HashSet<string>();
                DisplayLoadingAnimation();
                _appPath = Assembly.GetExecutingAssembly().Location;
                Application.Current.Properties["AppPath"] = _appPath;
                LogicLayer = ServiceLocator.GUI;

                // Initialize LogicLayer
                if (LogicLayer.Initiate(this))
                {
                    // Initialize Shell Integration
                    if (Settings.Default.EnableShellIntegration)
                    {
                        RegistryHelper.CreateRegistry(_appPath);
                    }

                    // Initialize tag panels
                    InitializeTagInfoPanel();
                    InitializeTagList();

                    Show();
                    Topmost = true;
                    Topmost = false;

                    // Display Welcome Screen
                    if (!Settings.Default.EnableAnimation)
                    {
                        DisplayWelcomeScreen(this, null);
                    }

                    // Initialize Notification Watchers
                    _notificationWatcher = new NotificationWatcher(this);
                    _notificationWatcher.Start();
                    _priorityNotificationWatcher = new PriorityNotificationWatcher(this);
                    _priorityNotificationWatcher.Start();

                    InitializeTimeSyncOnStartup(); // Start Time Sync
                }
                else
                {
                    DialogHelper.ShowError(this, "Syncless Initialization Error",
                                           "Syncless has failed to initialize and will now exit.\nPlease refer to http://code.google.com/p/big5sync/wiki/FAQs for more info");
                    _closednormally = false;

                    try
                    {
                        Close();
                    } catch(InvalidOperationException) {}
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Displays the Grand Loading Animation
        /// </summary>
        private void DisplayLoadingAnimation()
        {
            if (Settings.Default.EnableAnimation)
            {
                Storyboard loading = (Storyboard)Resources["MainWindowOnLoaded"];
                loading.Begin();
            }
        }

        /// <summary>
        /// Displays the Grand Unloading Animation
        /// </summary>
        private void DisplayUnloadingAnimation()
        {
            if (Settings.Default.EnableAnimation)
            {
                Storyboard unloading = (Storyboard)Resources["MainWindowUnloaded"];
                unloading.Begin();

                DateTime dateTime = DateTime.Now;

                while (DateTime.Now < dateTime.AddMilliseconds(1000))
                {
                    Dispatcher.Invoke(DispatcherPriority.Background,
                                      (DispatcherOperationCallback)delegate(object unused) { return null; }, null);
                }
            }
        }


        /// <summary>
        /// Displays the Welcome Screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayWelcomeScreen(object sender, EventArgs e)
        {
            if (Settings.Default.DisplayWelcomeScreen)
            {
                WelcomeScreenWindow wsw = new WelcomeScreenWindow(this);
                wsw.ShowDialog();
                Topmost = true;
                Topmost = false;
            }
        }

        #endregion

        #region De-Initializing Syncless

        /// <summary>
        /// Helper Method to do all methods to terminate Syncless
        /// </summary>
        /// <param name="showAnimation"></param>
        private void TerminateNow(bool showAnimation)
        {
            Settings.Default.Save(); // Save all settings
            if (Settings.Default.EnableShellIntegration == false) // Disable Shell Integration if necessary
            {
                RegistryHelper.RemoveRegistry();
            }
            // Stop notification watchers
            _notificationWatcher.Stop();
            _priorityNotificationWatcher.Stop();

            // Shows unloading animation
            if (showAnimation)
                DisplayUnloadingAnimation();
        }

        /// <summary>
        /// Carrys out all necessary steps to properly terminate Syncless when the user wants to close Syncless
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (_closednormally)
                {
                    // Prepares the SLL for termination
                    if (LogicLayer.PrepareForTermination())
                    {
                        bool result = DialogHelper.ShowWarning(this, "Exit", "Are you sure you want to exit Syncless?" +
                                                                             "\nExiting Syncless will disable seamless synchronization.");

                        if (result)
                        {
                            // Terminates the SLL and closes the UI
                            LogicLayer.Terminate();
                            TerminateNow(true);
                        }
                        else
                        {
                            // Cancels the closing event
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        // If Syncless not prepared for termination
                        bool result = DialogHelper.ShowWarning(this, "Exit",
                                                               "Are you sure you want to exit Syncless?" +
                                                               "\nAll current synchronization operations will be completed and" +
                                                               "\nany unfinished synchronization operations will be removed.");

                        if (!result)
                        {
                            // if user decides not to
                            e.Cancel = true;
                        }
                        else
                        {
                            // Shows the termination window via a background worker
                            DialogWindow terminationWindow = DialogHelper.ShowIndeterminate(this,
                                                                                            "Termination in Progress",
                                                                                            "Please wait for the current synchronization to complete.");
                            TaskbarIcon.Visibility = Visibility.Hidden;
                            terminationWindow.Show();
                            BackgroundWorker bgWorker = new BackgroundWorker();
                            bgWorker.DoWork += bw_DoWork;
                            bgWorker.RunWorkerCompleted += bw_RunWorkerCompleted;
                            bgWorker.RunWorkerAsync(terminationWindow);
                        }
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Termination Window Background Worker Work to Do
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            DialogWindow terminationWindow = e.Argument as DialogWindow;
            LogicLayer.Terminate();
            e.Result = terminationWindow;
        }

        /// <summary>
        /// When the background worker for the termination window has completed, sets the dialog window to be closable
        /// and closes Syncless
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogWindow terminationWindow = e.Result as DialogWindow;
            terminationWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                terminationWindow.
                    CannotBeClosed =
                    false; // Syncless can be finally closed
                terminationWindow.
                    Close();
            }));

            TerminateNow(false);
        }

        #endregion

        #region Tag List Panel

        /// <summary>
        /// Get the Lists of Tag from the LogicLayer and populates the TagList, after which displays the first tag
        /// if the list is not empty.
        /// </summary>
        public void InitializeTagList()
        {
            try
            {
                TagFilter = string.Empty;

                List<string> taglist = LogicLayer.GetAllTags();

                if(taglist != null)
                {
                    ListBoxTag.ItemsSource = taglist;
                    LblTagCount.Content = "[" + taglist.Count + "/" + taglist.Count + "]";
                    SelectedTag = (string)ListBoxTag.SelectedItem;

                    if (taglist.Count != 0)
                    {
                        SelectedTag = taglist[0];
                        SelectTag(SelectedTag);
                    }
                }

            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Any Selection change on the list of tags result in it being viewed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxTag.SelectedItem == null) return;


            ViewTagInfo(ListBoxTag.SelectedItem.ToString());
        }

        /// <summary>
        /// If the user press on the delete key on an unempty tag list, calls the remove tag process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && ListBoxTag.Items.Count > 0)
            {
                RemoveTag();
            }
        }

        /// <summary>
        /// For every text change in the tag filter, get all tags and filter the tag list based on the filter as 
        /// long as it contains the charactes in the tag filter and refreshes the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBoxFilterTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (LogicLayer != null)
                {
                    List<string> taglist = LogicLayer.GetAllTags();

                    if(taglist != null)
                    {
                        List<string> filteredtaglist = new List<string>();

                        int initial = taglist.Count;

                        // Looks through the tag list and takes those tags which contains the character
                        foreach (string x in taglist)
                        {
                            if (x.ToLower().Contains(TagFilter.ToLower()))
                                filteredtaglist.Add(x);
                        }

                        int after = filteredtaglist.Count;

                        // Displays Tag Count
                        LblTagCount.Content = "[" + after + "/" + initial + "]";

                        ListBoxTag.ItemsSource = null;
                        ListBoxTag.ItemsSource = filteredtaglist;
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Sets the Selected Tag on the TagList and then view the Tag on the TagInfoPanel
        /// </summary>
        /// <param name="tagname">Tag to Select</param>
        public void SelectTag(string tagname)
        {
            try
            {
                if (tagname != null)
                {
                    List<string> taglist = LogicLayer.GetAllTags();
                    
                    if(taglist != null)
                    {
                        int index = taglist.IndexOf(tagname);
                        ListBoxTag.SelectedIndex = index;
                        ViewTagInfo(tagname);
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        #endregion

        #region Time Sync

        /// <summary>
        /// Helper Method to Initiate the Time Sync Program After the Time Sync icon is clicked
        /// </summary>
        /// <returns>True if the Time Sync was successful, False otherwise.</returns>
        private bool InitiateTimeSyncHelper()
        {
            Process timeSync = new Process();
            timeSync.StartInfo.FileName = "SynclessTimeSync.exe";

            try
            {
                timeSync.Start();
                timeSync.WaitForExit();
            }
            catch (Win32Exception)
            {
                return false;
            }

            return timeSync.ExitCode == 0 ? true : false;
        }

        /// <summary>
        /// Calls the Time Sync Helper and displays the result
        /// </summary>
        private void InitiateTimeSync()
        {
            bool result = InitiateTimeSyncHelper();

            if (result)
            {
                DialogHelper.ShowInformation(this, "Time Synchronized Successfully",
                                             "Your computer clock has been synchronized with an Internet Time Server.");
            }
            else
            {
                DialogHelper.ShowError(this, "Time Synchronized Unsuccessfully",
                                       "Your computer clock could not be synchronized with an Internet Time Server.");
            }
        }

        /// <summary>
        /// Starts the TimeSync on Application Startup if the user enables the auto time sync on startup.
        /// Runs the TimeSync program on a background worker.
        /// </summary>
        private void InitializeTimeSyncOnStartup()
        {
            if (Settings.Default.SynchronizeTime)
            {
                BackgroundWorker timeWorker = new BackgroundWorker();
                timeWorker.DoWork += timeWorker_DoWork;
                timeWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Background worker to do TimeSync on Startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            InitiateTimeSyncHelper();
        }

        #endregion

        #region Header Panel

        /// <summary>
        /// Opens the Syncless Webpage after the Syncless Logo has been clicked
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        private void SynclessLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://code.google.com/p/big5sync/"));
        }

        /// <summary>
        /// Upon entering the Syncless Logo, the white highlight at the bottom will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SynclessLogoContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            LogoHighlight.Visibility = Visibility.Visible;
        }

        #endregion

        #region Application Command Panel: Exit/Minimize/Restore/Shortcuts

        /// <summary>
        ///     Sets the behavior of the close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Close();
            } catch(InvalidOperationException) {}
        }

        /// <summary>
        ///     Sets the behavior of the minimize button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MinimizeWindow();
        }

        /// <summary>
        /// Sets the Window in a minimized state and totally minimizes to the taskbar
        /// if the user has set the MinimizeToTray setting
        /// </summary>
        private void MinimizeWindow()
        {
            WindowState = WindowState.Minimized;
            if (Settings.Default.MinimizeToTray)
                ShowInTaskbar = false;
        }

        /// <summary>
        /// Restores the Main Window
        /// </summary>
        public void RestoreWindow()
        {
            ShowInTaskbar = true;
            WindowState = WindowState.Normal;
            Topmost = true;
            Topmost = false;
            Focus();
        }

        /// <summary>
        /// Displays the Option Window after clicking on the options icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOptions_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DisplayOptionsWindow();
        }

        /// <summary>
        /// Displays the shortcuts Window after clicking on the shortcuts icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShortcuts_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DisplayShortcutsWindow();
        }

        /// <summary>
        /// Displays the shortcut window
        /// </summary>
        private void DisplayShortcutsWindow()
        {
            ShortcutsWindow sw = new ShortcutsWindow(this);

            sw.ShowDialog();
        }

        #endregion

        #region TagInfo Panel

        /// <summary>
        ///     If lists of tag is empty, reset the UI back to 0, else displayed the first Tag on the list.
        /// </summary>
        private void InitializeTagInfoPanel()
        {
            try
            {
                List<string> taglist = LogicLayer.GetAllTags();

                if (taglist != null & taglist.Count == 0)
                {
                    ResetTagInfoPanel();
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Resets the tag info panel to the original state
        /// </summary>
        private void ResetTagInfoPanel()
        {
            TagTitle.Text = "Select a Tag";
            TagIcon.Visibility = Visibility.Hidden;
            TagStatusPanel.Visibility = Visibility.Hidden;
            SyncPanel.Visibility = Visibility.Hidden;
            BdrTaggedPath.Visibility = Visibility.Hidden;
            ProgressBarSync.Visibility = Visibility.Hidden;
            LblProgress.Visibility = Visibility.Hidden;
            SelectedTag = null;
            ListTaggedPath.ItemsSource = null;
        }

        /// <summary>
        /// Display the tag details window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DisplayTagDetailsWindow();
        }

        /// <summary>
        /// View the tag in the tag info panel
        /// </summary>
        /// <param name="tagname">Tag to be viewed</param>
        private void ViewTagInfo(string tagname)
        {
            try
            {
                TagView tv = LogicLayer.GetTag(tagname);

                // if tag not found, reset the panel and initialize the tag list
                if (tv == null)
                {
                    ResetTagInfoPanel();
                    InitializeTagList();
                    return;
                }
                SelectedTag = tagname;
                TagTitle.Text = tv.TagName;

                // Show Path List
                ListTaggedPath.ItemsSource = tv.PathStringList;

                // Sets visibility of the various components in the tag info panel
                TagIcon.Visibility = Visibility.Visible;
                TagStatusPanel.Visibility = Visibility.Visible;
                SyncPanel.Visibility = Visibility.Visible;
                BdrTaggedPath.Visibility = tv.PathStringList.Count == 0 ? Visibility.Hidden : Visibility.Visible;

                // If there is stored current progress percentage
                if (_syncProgressNotificationDictionary.ContainsKey(tagname))
                {
                    double percentageComplete = GetSyncProgressPercentage(tagname);

                    string status = GetTagStatus(tagname);
                    SetProgressBarColor(percentageComplete);
                    ProgressBarSync.Value = percentageComplete;
                    LblStatusText.Content = status;
                }
                else
                {
                    ProgressBarSync.Value = 0;
                    SetProgressBarColor(0);
                }

                // if there is stored tag status
                if (_tagStatusNotificationDictionary.ContainsKey(tagname))
                {
                    string tagStatus = GetTagStatus(tagname);

                    LblStatusText.Content = tagStatus;

                    if (tagStatus != "")
                    {
                        ProgressBarSync.Visibility = Visibility.Visible;
                        LblProgress.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ProgressBarSync.Visibility = Visibility.Hidden;
                        LblProgress.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    LblStatusText.Content = "";
                    ProgressBarSync.Visibility = Visibility.Hidden;
                    LblProgress.Visibility = Visibility.Hidden;
                }

                // Refreshes the tag info panel according to the tag info according current mode of the tag.
                switch (tv.TagState)
                {
                    case TagState.SeamlessToManual:
                    case TagState.ManualToSeamless:
                        Console.WriteLine("Viewing: Switching");
                        SwitchingMode();
                        break;
                    case TagState.Seamless:
                        Console.WriteLine("Viewing: Seamless");
                        SeamlessMode();
                        break;
                    case TagState.Manual:
                        _manualSyncEnabled = !tv.IsLocked;
                        Console.WriteLine("Viewing: Manual");
                        ManualMode();
                        break;
                }
                
                // if tag is in a state of cancellation
                if (CancellingTags.Contains(SelectedTag))
                {
                    BtnSyncNow.IsEnabled = false;
                    BtnPreview.Visibility = Visibility.Hidden;
                    LblSyncNow.Content = "Cancelling..";
                }
                else
                {
                    BtnSyncNow.IsEnabled = true;
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Event handler for BtnSyncMode Click and does the approriate events depending on what mode it is in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSyncMode_Click(object sender, RoutedEventArgs e)
        {
            if (!_manualSyncEnabled)
            {
                // If in Manual Sync, Cancel Synchronization
                bool success = LogicLayer.CancelSwitch(SelectedTag);
                if (!success)
                {
                    DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                           "You cannot change the synchronization mode while it is synchronizing.");
                }
                return;
            }

            try
            {
                // if tag is not locked
                if (!LogicLayer.GetTag(SelectedTag).IsLocked && !(GetTagStatus(SelectedTag) == "Finalizing"))
                {
                    // Button is in Manual mode now, change to seamless
                    if (string.Compare((string)LblSyncMode.Content, "Manual") == 0)
                    {
                        if (LogicLayer.SwitchMode(SelectedTag, TagMode.Seamless))
                        {
                            ProgressBarSync.Value = 0;
                            SetProgressBarColor(0);
                            const string message = "Please Wait";
                            LblStatusText.Content = message;
                            _tagStatusNotificationDictionary[SelectedTag] = message;
                        }
                        else
                        {
                            DialogHelper.ShowError(this, "Change Synchronization Mode Error",
                                                   SelectedTag + " could not be switched to Seamless Mode.");
                        }
                    } // if button is in seamless mode, switch to manual mode
                    else if (string.Compare((string)LblSyncMode.Content, "Seamless") == 0)
                    {
                        if (LogicLayer.SwitchMode(SelectedTag, TagMode.Manual))
                        {
                            _syncProgressNotificationDictionary.Remove(SelectedTag);
                            _tagStatusNotificationDictionary.Remove(SelectedTag);
                            LblStatusText.Content = "";
                            ProgressBarSync.Visibility = Visibility.Hidden;
                            LblProgress.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            DialogHelper.ShowError(this, "Change Synchronization Mode Error",
                                                   SelectedTag + " could not be switched to Manual Mode.");
                        }
                    }
                }
                else
                {
                    // if the tag is not locked, allow the user to cancel the switching
                    bool success = LogicLayer.CancelSwitch(SelectedTag);
                    if (!success)
                    {
                        DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                               "You cannot change the synchronization mode while it is synchronizing.");
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Handles the BtnSyncNow click and does the approriate events depending on what mode it is in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSyncNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Calls a path changed just in case some path has changed
                PathChanged();

                // if Button is in sync now mode
                if (LblSyncNow.Content.Equals("Sync Now"))
                {
                    BtnSyncNow.IsEnabled = false; // disable spamming

                    try
                    {
                        // if more than one path to sync
                        if (LogicLayer.GetTag(SelectedTag).PathStringList.Count > 1)
                        {
                            // start manual sync
                            if (LogicLayer.StartManualSync(SelectedTag))
                            {
                                ProgressBarSync.Visibility = Visibility.Visible;
                                LblProgress.Visibility = Visibility.Visible;
                                BtnPreview.Visibility = Visibility.Hidden;
                                CancelButtonMode();
                                const string message = "Synchronization Request Has Been Queued";
                                LblStatusText.Content = message;
                                _tagStatusNotificationDictionary[SelectedTag] = message;
                                _syncProgressNotificationDictionary[SelectedTag] = 0;
                                ProgressBarSync.Value = 0;
                                SetProgressBarColor(0);
                            }
                            else
                            {
                                DialogHelper.ShowError(this, "Synchronization Error",
                                                       SelectedTag + " could not be synchronized.");
                                SyncButtonMode();
                            }
                        }
                        else
                        {
                            DialogHelper.ShowError(this, "Nothing to Synchronize",
                                                   "You can only synchronize when there are two or more folders.");
                            SyncButtonMode(); // set back to sync button mode
                        }
                    }
                    catch (UnhandledException)
                    {
                        DialogHelper.DisplayUnhandledExceptionMessage(this);
                    }

                    BtnSyncNow.IsEnabled = true;
                }
                else // button is in syncing mode currently
                {
                    BtnSyncNow.IsEnabled = false;
                    bool success = LogicLayer.CancelManualSync(SelectedTag);
                    
                    // set to cancelling mode
                    if (success)
                    {
                        CancellingTags.Add(SelectedTag);
                        LblSyncNow.Content = "Cancelling..";
                    }
                    else
                    {
                        DialogHelper.ShowError(this, "Unable to Cancel",
                                               "Please wait until synchronization is complete.");
                        BtnSyncNow.IsEnabled = true;
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Event handler for BtnPreview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            // if in the various modes below, preview cannot proceed
            if (!_manualSyncEnabled || LogicLayer.GetTag(SelectedTag).IsLocked || GetTagStatus(SelectedTag) == "Finalizing")
            {
                DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                       "You cannot preview while it is synchronizing.");
                return;
            }

            // Preview enabled only if more than 1 path
            if (LogicLayer.GetTag(SelectedTag).PathStringList.Count > 1)
            {
                PreviewSyncWindow psw = new PreviewSyncWindow(this, SelectedTag);
                psw.ShowDialog();
            }
            else
            {
                DialogHelper.ShowError(this, "Nothing to Preview",
                                       "You can only preview only when there are two or more folders.");
            }
        }

        /// <summary>
        /// Sets The Tag Info Panel to Seamless Mode
        /// </summary>
        private void SeamlessMode()
        {
            LblSyncMode.Content = "Seamless";
            BtnSyncMode.ToolTip = "Switch to Manual Synchronization Mode";
            BtnPreview.Visibility = Visibility.Hidden;
            BtnSyncNow.Visibility = Visibility.Hidden;
            BtnSyncMode.SetResourceReference(BackgroundProperty, "ToggleOnBrush");
            LblSyncMode.SetResourceReference(MarginProperty, "ToggleOnMargin");
            LblSyncMode.SetResourceReference(ForegroundProperty, "ToggleOnForeground");
            ProgressBarSync.Visibility = Visibility.Hidden;
            LblProgress.Visibility = Visibility.Hidden;
            LblStatusTitle.Visibility = Visibility.Hidden;
            LblStatusText.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Sets The Tag Info Panel to Switching Mode
        /// </summary>
        private void SwitchingMode()
        {
            LblSyncMode.Content = "Please Wait";
            BtnSyncMode.ToolTip = "Please Wait";
            BtnPreview.Visibility = Visibility.Hidden;
            BtnSyncNow.Visibility = Visibility.Hidden;
            BtnSyncMode.SetResourceReference(BackgroundProperty, "ToggleOffBrush");
            LblSyncMode.SetResourceReference(MarginProperty, "ToggleOffMargin");
            LblSyncMode.SetResourceReference(ForegroundProperty, "ToggleOffForeground");

            TagView tv = LogicLayer.GetTag(SelectedTag);
            if(tv != null)
            {
                // if state is manual switching to seamless
                if (tv.TagState == TagState.ManualToSeamless)
                {
                    if (CurrentProgress != null && CurrentProgress.TagName == SelectedTag)
                    {
                        switch (CurrentProgress.State)
                        {
                            case SyncState.Analyzing:
                                ProgressBarSync.Visibility = Visibility.Visible;
                                LblProgress.Visibility = Visibility.Hidden;
                                ProgressBarSync.IsIndeterminate = true;
                                break;
                            case SyncState.Finalizing:
                            case SyncState.Synchronizing:
                                ProgressBarSync.IsIndeterminate = false;
                                ProgressBarSync.Visibility = Visibility.Visible;
                                LblProgress.Visibility = Visibility.Visible;
                                break;
                        }
                    }
                    else
                    {
                        ProgressBarSync.Visibility = Visibility.Hidden;
                        LblProgress.Visibility = Visibility.Hidden;
                    }
                } // else if stats is seamless to manual
                else if (tv.TagState == TagState.SeamlessToManual)
                {
                    ProgressBarSync.Visibility = Visibility.Visible;
                    LblProgress.Visibility = Visibility.Visible;
                }
            }

            LblStatusTitle.Visibility = Visibility.Visible;
            LblStatusText.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Sets The Tag Info Panel to Manual Mode
        /// </summary>
        private void ManualMode()
        {
            try
            {
                LblSyncMode.Content = "Manual";
                BtnSyncMode.ToolTip = "Switch to Seamless Synchronization Mode";
                BtnPreview.Visibility = Visibility.Visible;
                BtnSyncMode.SetResourceReference(BackgroundProperty, "ToggleOffBrush");
                LblSyncMode.SetResourceReference(MarginProperty, "ToggleOffMargin");
                LblSyncMode.SetResourceReference(ForegroundProperty, "ToggleOffForeground");

                // Check the current tag status of the selected tag
                if (_tagStatusNotificationDictionary.ContainsKey(SelectedTag))
                {
                    ProgressBarSync.Visibility = Visibility.Visible;
                    
                    // check if the tag is in analyzing mode
                    if (CurrentProgress.TagName == SelectedTag && CurrentProgress.State == SyncState.Analyzing)
                    {
                        LblProgress.Visibility = Visibility.Hidden;
                        ProgressBarSync.IsIndeterminate = true;
                    }
                    else
                    {
                        ProgressBarSync.IsIndeterminate = false;
                    }
                }

                LblStatusTitle.Visibility = Visibility.Visible;
                LblStatusText.Visibility = Visibility.Visible;

                // Set the visibility of BtnSyncNow and BtnPreview depending on whether what synchronization mode it is in
                if (SelectedTag != null)
                {
                    TagView tv = LogicLayer.GetTag(SelectedTag);

                    if(tv != null)
                    {
                        if (tv.IsLocked)
                        {
                            if (CurrentProgress != null && CurrentProgress.TagName == SelectedTag &&
                                (CurrentProgress.State == SyncState.Analyzing || CurrentProgress.State == SyncState.Queued ||
                                 CurrentProgress.State == SyncState.Started))
                            {
                                BtnSyncNow.Visibility = Visibility.Visible;
                                BtnPreview.Visibility = Visibility.Hidden;
                                CancelButtonMode();
                            }
                            else
                            {
                                BtnSyncNow.Visibility = Visibility.Hidden;
                                BtnPreview.Visibility = Visibility.Hidden;
                            }

                            if (tv.IsQueued)
                            {
                                BtnSyncNow.Visibility = Visibility.Visible;
                                CancelButtonMode();
                            }
                        }
                        else
                        {
                            SyncButtonMode();
                            BtnSyncNow.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Sets the BtnSyncNow to the cancel sync mode
        /// </summary>
        private void CancelButtonMode()
        {
            LblSyncNow.Content = "Cancel Sync";
            LblSyncNow.ToolTip = "Cancel Current Synchronization";
        }

        /// <summary>
        /// Sets the BtnSyncNow to the initial mode
        /// </summary>
        private void SyncButtonMode()
        {
            LblSyncNow.Content = "Sync Now";
            LblSyncNow.ToolTip = "Start Manual Synchronization";
        }

        #endregion

        #region Drag & Drop Functionality

        /// <summary>
        /// Mechanism which will handle any item that has been dragged into Syncless and dropped onto it.
        /// After dropping and if item is a valid folder, the tag window will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayoutRoot_Drop(object sender, DragEventArgs e)
        {
            try
            {
                HideDropIndicator();

                // If it is of file/drop type
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get all items that have been dropped and loops through them
                    string[] foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                    if (foldernames != null)
                        foreach (string i in foldernames)
                        {
                            string path = i;

                            // convert potential shortcuts into folders
                            string shortcutfolderpath = FileFolderHelper.GetShortcutTargetFile(i);
                            if (shortcutfolderpath != null)
                            {
                                path = shortcutfolderpath;
                            }

                            // Check for the existence of folders
                            try
                            {
                                DirectoryInfo folder = new DirectoryInfo(path);
                                if (folder.Exists && !FileFolderHelper.IsFile(path))
                                {
                                    // displays the tag window for each folder
                                    TagWindow tw = new TagWindow(this, path, SelectedTag, false);
                                }
                            } catch {}  // do nothing, and this is the desired behavior. move on with the tagging process
                                        // if an exception was encountered.
                        }
                }
                TxtBoxFilterTag.IsHitTestVisible = true;
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Event handler after any item has been dragged into the Main Window.
        /// The items will be checked to see if they are folders. If one of them is, the tag to drop image will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayoutRoot_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                TxtBoxFilterTag.IsHitTestVisible = false;
                // If it is of file/drop type
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get all items that have been dropped and loops through them
                    string[] foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                    if (foldernames != null)
                        foreach (string i in foldernames)
                        {
                            string path = i;

                            // convert potential shortcuts into folders
                            string shortcutfolderpath = FileFolderHelper.GetShortcutTargetFile(path);
                            if (shortcutfolderpath != null)
                            {
                                path = shortcutfolderpath;
                            }

                            // Check for the existence of folders
                            try
                            {
                                DirectoryInfo folder = new DirectoryInfo(path);
                                if (folder.Exists && !FileFolderHelper.IsFile(path))
                                {
                                    ShowDropIndicator(); // Shows drop image
                                    break;  // breaks out of the loop as the image will be displayed as long as one of the
                                            // item dragged in is a folder
                                }
                            }
                            catch {}   // do nothing, and this is the desired behavior. move on with the tagging process
                                       // if an exception was encountered.
                        }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Behavior when the mouse is in drag and drop mode and has left the MainWindow
        /// Hides the Drop to Tag Image restores back the hit test mode of the Main Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayoutRoot_DragLeave(object sender, DragEventArgs e)
        {
            HideDropIndicator();
            TxtBoxFilterTag.IsHitTestVisible = true;
        }

        /// <summary>
        /// Shows Drop To Tag Image on the Main Window
        /// </summary>
        private void ShowDropIndicator()
        {
            DropIndicator.Visibility = Visibility.Visible;
            DropIndicator.Focusable = false;
        }

        /// <summary>
        /// Hides the Tag to Drop Image on the Main Window
        /// </summary>
        private void HideDropIndicator()
        {
            DropIndicator.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Sync Progress Status

        /// <summary>
        /// Lookup the current Synchronization Progress Percentage if the selected tag is the tag currently
        /// in synchronization or refers to the dictionary if it is not.
        /// </summary>
        /// <param name="tagname">Tag to lookup for progress percentage</param>
        /// <returns></returns>
        public double GetSyncProgressPercentage(string tagname)
        {
            return CurrentProgress.TagName == tagname
                       ? CurrentProgress.PercentComplete
                       : _syncProgressNotificationDictionary[tagname];
        }

        /// <summary>
        /// Get the current tag status from the Dictionary
        /// </summary>
        /// <param name="tagname">Tag to lookup for status</param>
        /// <returns></returns>
        public string GetTagStatus(string tagname)
        {
            try
            {
                string status = _tagStatusNotificationDictionary[tagname];

                return status;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Sets the colour of the progress bar depending on the percentage. Change in colour of the progress bar
        /// is based on the percentage of the progress which is passed in
        /// </summary>
        /// <param name="percentageComplete">Progress Bar Percentage</param>
        private void SetProgressBarColor(double percentageComplete)
        {
            byte rcolor = 0, gcolor = 0;
            const byte bcolor = 0;

            if (percentageComplete <= 50)
            {
                rcolor = 211;
                gcolor = (byte)(percentageComplete / 50 * 211);
            }
            else
            {
                rcolor = (byte)((100 - percentageComplete) / 50 * 211);
                gcolor = 211;
            }

            ProgressBarSync.Foreground = new SolidColorBrush(Color.FromArgb(255, rcolor, gcolor, bcolor));
        }

        /// <summary>
        /// Removes all Tag + Synchronization Status from a particular tag
        /// </summary>
        /// <param name="tag">Tag to reset</param>
        public void ResetTagSyncStatus(string tag)
        {
            _syncProgressNotificationDictionary.Remove(tag);
            _tagStatusNotificationDictionary.Remove(tag);
            LblStatusText.Content = "";
        }

        #endregion

        #region Core Syncless Functionality

        /// <summary>
        /// Creates a new tag
        /// </summary>
        /// <param name="tagName">Tag to create</param>
        /// <returns>True if it can be created. False if the tag already exists.</returns>
        public bool CreateTag(string tagName)
        {
            try
            {
                try
                {
                    TagView tv = LogicLayer.CreateTag(tagName);
                    if (tv != null)
                    {
                        InitializeTagList();
                        SelectTag(tagName);
                    }
                    else
                    {
                        DialogHelper.ShowError(this, "Tag Creation Error", "Tag could not be created.");
                    }
                }
                catch (TagAlreadyExistsException)
                {
                    return false;
                }
            }
            catch (UnhandledException ue)
            {
                throw ue;
            }

            return true;
        }

        /// <summary>
        /// Remove the currently selected tag if it is not locked
        /// </summary>
        private void RemoveTag()
        {
            try
            {
                if (SelectedTag != null)
                {
                    if (!LogicLayer.GetTag(SelectedTag).IsLocked)
                    {
                        bool result = DialogHelper.ShowWarning(this, "Remove Tag",
                                                               "Are you sure you want to remove " + SelectedTag + "?");

                        // if users really want to remove
                        if (result)
                        {
                            if (!LogicLayer.GetTag(SelectedTag).IsLocked)
                            {
                                bool success = LogicLayer.DeleteTag(SelectedTag);
                                if (success)
                                {
                                    // Wipe all trace of the tag from the stored statuses and reset the taginfo panel and the initialize tag list.

                                    _syncProgressNotificationDictionary.Remove(SelectedTag);
                                    _tagStatusNotificationDictionary.Remove(SelectedTag);

                                    ResetTagInfoPanel();
                                    InitializeTagList();
                                }
                                else
                                {
                                    DialogHelper.ShowError(this, "Remove Tag Error",
                                                           "" + SelectedTag + " could not be removed.");
                                }
                            }
                            else
                            {
                                DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                                       "You cannot remove a tag while the tag is synchronizing.");
                            }
                        }
                    }
                    else
                    {
                        DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                               "You cannot remove a tag while the tag is synchronizing.");
                    }
                }
                else
                {
                    DialogHelper.ShowError(this, "No Tag Selected", "Please select a tag to remove.");
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Untags the currently selected path from the Tag, but only if the tag is not locked
        /// </summary>
        private void Untag()
        {
            try
            {
                if (SelectedTag == null || !ListTaggedPath.HasItems)
                {
                    DialogHelper.ShowError(this, "Nothing to Untag", "There is nothing to untag.");
                }
                else
                {
                    string currentTag = SelectedTag;

                    // if tag is not locked
                    if (!LogicLayer.GetTag(currentTag).IsLocked)
                    {
                        // if no path was selected
                        if (ListTaggedPath.SelectedIndex == -1)
                        {
                            DialogHelper.ShowError(this, "No Path Selected", "Please select a path to untag.");
                        }
                        else
                        {
                            TagView tv = LogicLayer.GetTag(currentTag);

                            if (tv != null)
                            {
                                if (!tv.IsLocked)
                                {
                                    int count = LogicLayer.Untag(currentTag,
                                                          new DirectoryInfo((string)ListTaggedPath.SelectedValue));

                                    if (count == 1)
                                    {
                                        //success
                                        ResetTagSyncStatus(currentTag);
                                    }
                                    else
                                    {
                                        // fail
                                        DialogHelper.ShowError(this, "Error Untagging",
                                                               "An error while untagging the folder");
                                    }

                                    SelectTag(currentTag);
                                }
                                else
                                {
                                    DialogHelper.ShowError(this, tv.TagName + " is Synchronizing",
                                                           "You cannot untag while a tag is synchronizing.");
                                }
                            }
                            else
                            {
                                DialogHelper.ShowError(this, "Tag Does Not Exist",
                                                       "The tag which you tried to untag does not exist.");

                                ResetTagInfoPanel();
                                InitializeTagList();

                                return;
                            }
                        }
                    }
                    else
                    {
                        DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                               "You cannot untag a folder while the tag is synchronizing.");
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Displays the Tag Details Window, but only if the tag is not synchronizing.
        /// </summary>
        private void DisplayTagDetailsWindow()
        {
            if (SelectedTag != null)
            {
                if (!LogicLayer.GetTag(SelectedTag).IsLocked)
                {
                    TagDetailsWindow tdw = new TagDetailsWindow(this, SelectedTag);
                    tdw.ShowDialog();
                }
                else
                {
                    DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                           "You cannot view tag details while the tag is synchronizing.");
                }
            }
            else
            {
                DialogHelper.ShowError(this, "No Tag Selected", "Please select a tag to show details.");
            }
        }

        /// <summary>
        /// Displays the Log Window
        /// </summary>
        private void DisplayLogWindow()
        {
            LogWindow lw = new LogWindow(this);
        }

        /// <summary>
        /// Displays the option window, and make it single instance as multiple options can be called via the 
        /// context tray if it is not.
        /// </summary>
        private void DisplayOptionsWindow()
        {
            // Single Instance handling
            if (Application.Current.Properties["OptionsWindowIsOpened"] == null)
            {
                Application.Current.Properties["OptionsWindowIsOpened"] = false;
            }

            if (!(bool)Application.Current.Properties["OptionsWindowIsOpened"])
            {
                OptionsWindow ow = new OptionsWindow(this);
                ow.ShowDialog();
            }
        }

        /// <summary>
        /// Displays the Unmonitor Context Menu, and populates the drive with all USB Drive letters
        /// </summary>
        private void DisplayUnmonitorContextMenu()
        {
            ContextMenu driveMenu = new ContextMenu();

            List<string> removableDrives = DriveHelper.GetUSBDriveLetters();
            if (removableDrives.Count == 0)
            {
                MenuItem driveMenuItem = new MenuItem();
                driveMenuItem.Header = "No Removable Drives Found";
                driveMenu.Items.Add(driveMenuItem);
            }
            else
            {
                foreach (string letter in removableDrives)
                {
                    MenuItem driveMenuItem = new MenuItem();
                    driveMenuItem.Header = letter;
                    driveMenuItem.Click += driveMenuItem_Click;
                    driveMenu.Items.Add(driveMenuItem);
                }
            }

            driveMenu.PlacementTarget = this;
            driveMenu.IsOpen = true;
        }

        /// <summary>
        /// Displays the Default Tag Window
        /// </summary>
        private void DisplayTagWindow()
        {
            TagWindow tw = new TagWindow(this, "", SelectedTag, false);
        }

        /// <summary>
        /// Unmonitors the drive which is clicked under the Unmonitor Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void driveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem source = (MenuItem)sender;
                string driveletter = (string)source.Header;
                DriveInfo drive = new DriveInfo(driveletter);
                if (!LogicLayer.AllowForRemoval(drive))
                {
                    DialogHelper.ShowError(this, "Unmonitor Error",
                                           "Syncless could not unmonitor " + driveletter + " from seamless mode.");
                }
                else
                {
                    DialogHelper.ShowInformation(this, "Monitoring Stopped for " + driveletter,
                                                 "Syncless has stopped all seamless monitoring for " + driveletter +
                                                 "\nTagging any folders in this drive will re-activate seamless monitoring.");
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        #endregion

        #region Tag List Panel Context Menu

        /// <summary>
        /// Calls the Remove Tag via Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveTagRightClick_Click(object sender, RoutedEventArgs e)
        {
            RemoveTag();
        }

        /// <summary>
        /// Displays Tag Details Window via Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailsRightClick_Click(object sender, RoutedEventArgs e)
        {
            DisplayTagDetailsWindow();
        }

        /// <summary>
        /// Displays Tag Window via Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagRightClick_Click(object sender, RoutedEventArgs e)
        {
            DisplayTagWindow();
        }

        #endregion

        #region Notification

        /// <summary>
        /// Displays a notification via a balloon in the Taskbar if tray notification is enabled.
        /// </summary>
        /// <param name="title">Title of the Balloon</param>
        /// <param name="text">Text of the Balloon</param>
        public void NotifyBalloon(string title, string text)
        {
            if (Settings.Default.EnableTrayNotification)
            {
                TaskbarIcon.ShowBalloonTip(title, text, BalloonIcon.Info);
            }

            if (Settings.Default.EnableNotificationSounds)
            {
                SystemSounds.Exclamation.Play();
            }
        }

        /// <summary>
        /// Notify the UI that the cancellation is complete and refreshes the UI according to the Cancel State
        /// if the currently selected tag is the tag which the cancellation has completed for.
        /// </summary>
        /// <param name="tagname">Tagname to cancel</param>
        public void NotifyCancelComplete(string tagname)
        {
            if (SelectedTag == tagname)
            {
                SyncButtonMode();
                LblStatusText.Content = "";
                _tagStatusNotificationDictionary.Remove(SelectedTag);
                BtnSyncNow.IsEnabled = true;
                BtnPreview.Visibility = Visibility.Visible;
                ProgressBarSync.IsIndeterminate = false;
                ProgressBarSync.Visibility = Visibility.Hidden;
                LblProgress.Visibility = Visibility.Hidden;
                _syncProgressNotificationDictionary.Remove(SelectedTag);
            }
            CancellingTags.Remove(tagname);
        }

        /// <summary>
        /// Notify the UI that the auto sychronization is completed via tray notification.
        /// </summary>
        /// <param name="path">Path that has been synchronized</param>
        public void NotifyAutoSyncComplete(string path)
        {
            NotifyBalloon("Synchronization Completed", path + " is now synchronized.");
        }

        /// <summary>
        /// Notify the UI that there is nothing to synchronize and refreshes the UI according to the 
        /// nothing to synchronize state if the currently selected tag is the tag which the notification is for
        /// </summary>
        /// <param name="tagname">Tagname to notify</param>
        public void NotifyNothingToSync(string tagname)
        {
            string message = "Nothing to Synchronize";

            if (SelectedTag == tagname)
            {
                LblStatusText.Content = message;
                BtnSyncNow.IsEnabled = true;
                BtnPreview.IsEnabled = true;
                BtnSyncMode.IsEnabled = true;
                _manualSyncEnabled = true;
            }

            _tagStatusNotificationDictionary[tagname] = message;
        }

        #endregion

        #region Progress Notification

        /// <summary>
        /// Notify that a synchronization has started and refreshes the UI if the tag of the progress is the currently selected tag
        /// </summary>
        /// <param name="progress">Progress to notify</param>
        public void ProgressNotifySyncStart(SyncProgress progress)
        {
            const string message = "Synchronization Started";

            if (SelectedTag == progress.TagName)
            {
                LblStatusText.Content = message;
            }

            _tagStatusNotificationDictionary[progress.TagName] = message;
            NotifyBalloon("Synchronization Started", progress.TagName + " is being synchronized.");
        }

        /// <summary>
        /// Notify that analyzing has started and refreshes the UI if the tag of the progress is the currently selected tag
        /// </summary>
        /// <param name="progress">Progress to notify</param>
        public void ProgressNotifyAnalyzing(SyncProgress progress)
        {
            const string message = "Analyzing Folders";
            const double percentageComplete = 0;
            string tagname = progress.TagName;

            if (SelectedTag == tagname)
            {
                BtnPreview.Visibility = Visibility.Hidden;
                LblStatusText.Content = message;
                ProgressBarSync.Value = percentageComplete;
                SetProgressBarColor(percentageComplete);
                ProgressBarSync.IsIndeterminate = true;
                LblProgress.Visibility = Visibility.Hidden;
            }

            _syncProgressNotificationDictionary[tagname] = percentageComplete;
            _tagStatusNotificationDictionary[tagname] = message;
        }

        /// <summary>
        /// Notify that synchronizing has started and refreshes the UI if the tag of the progress is the currently selected tag
        /// </summary>
        /// <param name="progress">Progress to notify</param>
        public void ProgressNotifySynchronizing(SyncProgress progress)
        {
            if (SelectedTag == progress.TagName)
            {
                BtnPreview.Visibility = Visibility.Hidden;
                BtnSyncNow.Visibility = Visibility.Hidden;
                ProgressBarSync.IsIndeterminate = false;
                LblProgress.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Notify that finalizing has started and refreshes the UI if the tag of the progress is the currently selected tag
        /// </summary>
        /// <param name="progress">Progress to notify</param>
        public void ProgressNotifyFinalizing(SyncProgress progress)
        {
            if (SelectedTag == progress.TagName)
            {
                BtnPreview.Visibility = Visibility.Hidden;
                BtnSyncNow.Visibility = Visibility.Hidden;
                ProgressBarSync.IsIndeterminate = false;
                LblProgress.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Notify that synchronization has completed and refreshes the UI if the tag of the progress is the currently selected tag
        /// </summary>
        /// <param name="progress">Progress to notify</param>
        public void ProgressNotifySyncComplete(SyncProgress progress)
        {
            string message = "Synchronization Completed at " + DateTime.Now;
            double percentageComplete = progress.PercentComplete;
            _tagStatusNotificationDictionary[progress.TagName] = message;
            _syncProgressNotificationDictionary[progress.TagName] = percentageComplete;
            NotifyBalloon("Synchronization Completed", progress.TagName + " is now synchronized.");

            if (SelectedTag == progress.TagName)
            {
                ProgressBarSync.IsIndeterminate = false;
                LblStatusText.Content = message;
                ProgressBarSync.Value = percentageComplete;
                SetProgressBarColor(percentageComplete);
                BtnSyncNow.IsEnabled = true;
                BtnPreview.IsEnabled = true;
                BtnSyncMode.IsEnabled = true;
                _manualSyncEnabled = true;

                if (LogicLayer.GetTag(SelectedTag).IsSeamless)
                {
                    BtnSyncNow.Visibility = Visibility.Hidden;
                    BtnPreview.Visibility = Visibility.Hidden;
                }
                else
                {
                    SyncButtonMode();
                    BtnSyncNow.Visibility = Visibility.Visible;
                    BtnPreview.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Notify that that a state changed has occured and refreshes the UI if the tag of the progress is the currently selected tag
        /// </summary>
        /// <param name="progress">Progress to notify</param>
        public void ProgressNotifyChange(SyncProgress progress)
        {
            string message = string.Empty;
            string breakString = progress.Message ?? string.Empty;

            if (breakString.Length >= 45)
            {
                breakString = breakString.Substring(0, 10) + " ... " +
                              breakString.Substring(breakString.Length - 35, 35);
            }

            switch (progress.State)
            {
                case SyncState.Analyzing:
                    message = "Analyzing " + breakString;
                    break;
                case SyncState.Synchronizing:
                    message = "Synchronizing " + breakString;
                    break;
                case SyncState.Finalizing:
                    message = "Finalizing " + breakString;
                    break;
                default:
                    return;
            }

            double percentageComplete = progress.PercentComplete;
            string tagname = progress.TagName;

            if (SelectedTag == tagname)
            {
                LblStatusText.Content = message;
                ProgressBarSync.Value = percentageComplete;
                SetProgressBarColor(percentageComplete);
                if (LogicLayer.GetTagState(SelectedTag) == TagState.ManualToSeamless ||
                    LogicLayer.GetTagState(SelectedTag) == TagState.SeamlessToManual)
                {
                    SwitchingMode();
                }
            }

            _syncProgressNotificationDictionary[tagname] = percentageComplete;
            _tagStatusNotificationDictionary[tagname] = message;
        }

        #endregion

        #region Events Handlers for Taskbar Icon Context Menu Items

        /// <summary>
        /// Exits Syncless from the Taskbar Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskbarExitItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (InvalidOperationException) {}
        }

        /// <summary>
        /// Displays the Option Window from the Taskbar Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskbarOptionsItem_Click(object sender, RoutedEventArgs e)
        {
            DisplayOptionsWindow();
        }

        /// <summary>
        /// Displays the Tag Window from the Taskbar Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskbarTagItem_Click(object sender, RoutedEventArgs e)
        {
            DisplayTagWindow();
        }

        /// <summary>
        /// Displays the list of removable drives to unmonitor from the Taskbar Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskbarUnmonitorItem_Click(object sender, RoutedEventArgs e)
        {
            DisplayUnmonitorContextMenu();
        }

        /// <summary>
        /// Restores the Main Window from the Taskbar Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskbarOpenItem_Click(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        /// <summary>
        /// Restoress the Main Window from by click on the Taskbar Icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        #endregion

        #region Events Handlers for Tag Info Panel Context Menu

        /// <summary>
        /// Calls the Window Explorer Helper from Tag Info Panel Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenInExplorerRightClick_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderInWindowsExplorerHelper();
        }

        /// <summary>
        /// Calls the Untag from Tag Info Panel Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UntagRightClick_Click(object sender, RoutedEventArgs e)
        {
            Untag();
        }

        /// <summary>
        /// Opens Windows Explorer upon path double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListTaggedPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFolderInWindowsExplorerHelper();
        }

        /// <summary>
        /// Helper method to open the folder in Windows Explorer
        /// </summary>
        private void OpenFolderInWindowsExplorerHelper()
        {
            string path = (string)ListTaggedPath.SelectedItem;
            if (path != "")
            {
                ProcessStartInfo runExplorer = new ProcessStartInfo();
                runExplorer.FileName = "explorer.exe";
                runExplorer.Arguments = path;
                Process.Start(runExplorer);
            }
        }

        #endregion

        #region Methods & Helper Methods Implementation in IUIInterface

        /// <summary>
        /// Get the Directory Name for the Syncless Application
        /// </summary>
        /// <returns>The directory name where Syncless is opened from</returns>
        public string getAppPath()
        {
            return Path.GetDirectoryName(_appPath);
        }

        /// <summary>
        /// Called by the LogicLayer to refresh the Tag Info Panel if a drive changed event detected, and that tag is currently the selected tag
        /// </summary>
        public void DriveChanged()
        {
            UpdateTagList_ThreadSafe();
        }

        /// <summary>
        /// Called by the LogicLayer to refresh the Tag List when tags have been changed.
        /// </summary>
        public void TagsChanged()
        {
            UpdateTagList_ThreadSafe();
        }

        /// <summary>
        /// Called by the LogicLayer to refresh the Tag Info Panel if a Tag has been changed, and that tag is currently the selected tag
        /// </summary>
        /// <param name="tagName">Tag name to refresh</param>
        public void TagChanged(string tagName)
        {
            UpdateTagInfo_ThreadSafe(tagName);
        }

        /// <summary>
        /// Called by the LogicLayer to refresh the Path List if the path has been changed
        /// </summary>
        public void PathChanged()
        {
            try
            {
                ListTaggedPath.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                      (Action)(() =>
                                                      {
                                                          if (SelectedTag == null)
                                                          {
                                                              return;
                                                          }
                                                          TagView tv = LogicLayer.GetTag(SelectedTag);

                                                          if(tv != null)
                                                          {
                                                              ListTaggedPath.ItemsSource = tv.PathStringList;
                                                              BdrTaggedPath.Visibility =
                                                                  tv.PathStringList.Count == 0
                                                                      ? Visibility.Hidden
                                                                      : Visibility.Visible;
                                                          }

                                                      }));
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Refreshes the currently selected tag
        /// </summary>
        /// <param name="tagName"></param>
        private void UpdateTagInfo_ThreadSafe(string tagName)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                if (SelectedTag == tagName)
                {
                    ViewTagInfo(tagName);
                }
            }));
        }

        /// <summary>
        /// Refreshes the whole tag list
        /// </summary>
        private void UpdateTagList_ThreadSafe()
        {
            try
            {
                ListBoxTag.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                  (Action) (() =>
                                                                {
                                                                    List<string> taglist = LogicLayer.GetAllTags();
                                                                    ListBoxTag.ItemsSource = taglist;
                                                                    if(taglist != null)
                                                                    {
                                                                        LblTagCount.Content = "[" + taglist.Count + "/" +
                                                                                              taglist.Count + "]";
                                                                    }
                                                                }));
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        #endregion

        #region Commandline Interface: Tag/Untag/Clean

        /// <summary>
        /// Method which passes any commandline arguments to CommandlineHelper for parsing
        /// </summary>
        /// <param name="args"></param>
        public void ProcessCommandLine(string[] args)
        {
            if (args.Length != 0)
            {
                CommandLineHelper.ProcessCommandLine(args, this);
            }
        }

        /// <summary>
        /// Displays the tag window for a given p.ath
        /// </summary>
        /// <param name="clipath">Path from Commandline</param>
        public void CliTag(string clipath)
        {
            string tagname = "";

            if (FileFolderHelper.IsFile(clipath))
            {
                DialogHelper.ShowError(this, "Tagging not Allowed", "You cannot tag a file.");
                return;
            }

            try
            {
                DirectoryInfo di = new DirectoryInfo(clipath);
                tagname = di.Name;
            }
            catch (Exception)
            {
                DialogHelper.ShowError(this, "Invalid Folder", "You cannot tag this folder.");
                return;
            }

            TagWindow tw = new TagWindow(this, clipath, tagname, true);
        }

        /// <summary>
        /// Displays the Untag by folder Window for a given path
        /// </summary>
        /// <param name="clipath">Path from Commandline</param>
        public void CliUntag(string clipath)
        {
            if (FileFolderHelper.IsFile(clipath))
            {
                DialogHelper.ShowError(this, "Untagging not Allowed", "You cannot tag a file.");
                return;
            }

            try
            {
                new DirectoryInfo(clipath);
            }
            catch (ArgumentException)
            {
                DialogHelper.ShowError(this, "Invalid Folder", "You cannot untag this folder.");
                return;
            }

            UntagWindow tw = new UntagWindow(this, clipath, true);
        }

        /// <summary>
        /// Cleans a path of all meta-data files.
        /// </summary>
        /// <param name="clipath">Path from Commandline</param>
        public void CliClean(string clipath)
        {
            if (FileFolderHelper.IsFile(clipath))
            {
                DialogHelper.ShowError(this, "Cleaning not Allowed", "You cannot clean a file.");
                return;
            }

            try
            {
                new DirectoryInfo(clipath);
            }
            catch (ArgumentException)
            {
                DialogHelper.ShowError(this, "Invalid Folder", "You cannot clean this folder.");
                return;
            }

            ServiceLocator.GUI.Clean(clipath);
            DialogHelper.ShowInformation(this, "Folder Cleaned", "The folder has been cleared of all meta-data files.");
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialize All KeyboardCommands
        /// </summary>
        private void InitializeAllCommands()
        {
            InitializeCreateTagCommand();
            InitializeRemoveTagCommand();
            InitializeTagCommand();
            InitializeUntagCommand();
            InitializeTagDetailsCommand();
            InitializeUnmonitorCommand();
            InitializeOptionsCommand();
            InitializeMinimizeCommand();
            InitializeShortcutsCommand();
            InitializeLogCommand();
            InitializeExitCommand();
            InitializeTimeSyncCommand();
        }

        /// <summary>
        /// Initialize TimeSyncCommand
        /// </summary>
        private void InitializeTimeSyncCommand()
        {
            RoutedCommand TimeSyncCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(TimeSyncCommand, TimeSyncCommandExecute, TimeSyncCommandCanExecute);
            CommandBindings.Add(cb);

            BtnTimeSync.Command = TimeSyncCommand;

            KeyGesture kg = new KeyGesture(Key.Y, ModifierKeys.Control);
            InputBinding ib = new InputBinding(TimeSyncCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize ExitCommand
        /// </summary>
        private void InitializeExitCommand()
        {
            RoutedCommand ExitCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(ExitCommand, ExitCommandExecute, ExitCommandCanExecute);
            CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.W, ModifierKeys.Control);
            InputBinding ib = new InputBinding(ExitCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize LogCommand
        /// </summary>
        private void InitializeLogCommand()
        {
            RoutedCommand LogCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(LogCommand, LogCommandExecute, LogCommandCanExecute);
            CommandBindings.Add(cb);

            BtnLog.Command = LogCommand;

            KeyGesture kg = new KeyGesture(Key.L, ModifierKeys.Control);
            InputBinding ib = new InputBinding(LogCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize ShortcutsCommand
        /// </summary>
        private void InitializeShortcutsCommand()
        {
            RoutedCommand ShortcutsCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(ShortcutsCommand, ShortcutsCommandExecute, ShortcutsCommandCanExecute);
            CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.S, ModifierKeys.Control);
            InputBinding ib = new InputBinding(ShortcutsCommand, kg);
            KeyGesture kg1 = new KeyGesture(Key.OemQuestion, ModifierKeys.Shift);
            InputBinding ib1 = new InputBinding(ShortcutsCommand, kg1);
            InputBindings.Add(ib);
            InputBindings.Add(ib1);
        }

        /// <summary>
        /// Initialize MinimizeCommand
        /// </summary>
        private void InitializeMinimizeCommand()
        {
            RoutedCommand MinimizeCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(MinimizeCommand, MinimizeCommandExecute, MinimizeCommandCanExecute);
            CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.M, ModifierKeys.Control);
            InputBinding ib = new InputBinding(MinimizeCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize OptionsCommand
        /// </summary>
        private void InitializeOptionsCommand()
        {
            RoutedCommand OptionsCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(OptionsCommand, OptionsCommandExecute, OptionsCommandCanExecute);
            CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.O, ModifierKeys.Control);
            InputBinding ib = new InputBinding(OptionsCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize UnmonitorCommand
        /// </summary>
        private void InitializeUnmonitorCommand()
        {
            RoutedCommand UnmonitorCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(UnmonitorCommand, UnmonitorCommandExecute, UnmonitorCommandCanExecute);
            CommandBindings.Add(cb);

            BtnUnmonitor.Command = UnmonitorCommand;

            KeyGesture kg = new KeyGesture(Key.I, ModifierKeys.Control);
            InputBinding ib = new InputBinding(UnmonitorCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize TagDetailsCommand
        /// </summary>
        private void InitializeTagDetailsCommand()
        {
            RoutedCommand DetailsCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(DetailsCommand, DetailsCommandExecute, DetailsCommandCanExecute);
            CommandBindings.Add(cb);

            BtnDetails.Command = DetailsCommand;

            KeyGesture kg = new KeyGesture(Key.D, ModifierKeys.Control);
            InputBinding ib = new InputBinding(DetailsCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize UntagCommand
        /// </summary>
        private void InitializeUntagCommand()
        {
            RoutedCommand UntagCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(UntagCommand, UntagCommandExecute, UntagCommandCanExecute);
            CommandBindings.Add(cb);

            btnUntag.Command = UntagCommand;

            KeyGesture kg = new KeyGesture(Key.U, ModifierKeys.Control);
            InputBinding ib = new InputBinding(UntagCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize TagCommand
        /// </summary>
        private void InitializeTagCommand()
        {
            RoutedCommand TagCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(TagCommand, TagCommandExecute, TagCommandCanExecute);
            CommandBindings.Add(cb);

            btnTag.Command = TagCommand;

            KeyGesture kg = new KeyGesture(Key.T, ModifierKeys.Control);
            InputBinding ib = new InputBinding(TagCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize RemoveTagCommand
        /// </summary>
        private void InitializeRemoveTagCommand()
        {
            RoutedCommand RemoveTagCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(RemoveTagCommand, RemoveTagCommandExecute, RemoveTagCommandCanExecute);
            CommandBindings.Add(cb);

            btnRemove.Command = RemoveTagCommand;

            KeyGesture kg = new KeyGesture(Key.R, ModifierKeys.Control);
            InputBinding ib = new InputBinding(RemoveTagCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Initialize CreateTagCommand
        /// </summary>
        private void InitializeCreateTagCommand()
        {
            RoutedCommand CreateTagCommand = new RoutedCommand();

            CommandBinding cb = new CommandBinding(CreateTagCommand, CreateTagCommandExecute, CreateTagCommandCanExecute);
            CommandBindings.Add(cb);

            BtnCreate.Command = CreateTagCommand;

            KeyGesture kg = new KeyGesture(Key.N, ModifierKeys.Control);
            InputBinding ib = new InputBinding(CreateTagCommand, kg);
            InputBindings.Add(ib);
        }

        /// <summary>
        /// Code Exected When Create Tag Command is called. Displays Create Tag Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            CreateTagWindow ctw = new CreateTagWindow(this);
            ctw.ShowDialog();
        }

        /// <summary>
        /// EventHandler to determined if Create Tag Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When Remove Tag Command is called. Calls the RemoveTag();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveTagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            RemoveTag();
        }

        /// <summary>
        /// EventHandler to determined if RemoveTag Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveTagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        
        /// <summary>
        /// Code Exected When Tag Command is called. Display Tag Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayTagWindow();
        }

        /// <summary>
        /// EventHandler to determined if Tag Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// EventHandler to determined if Untag Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UntagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Untag();
        }

        /// <summary>
        /// EventHandler to determined if Untag Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UntagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When Details Command is called. Display Tag Details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailsCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayTagDetailsWindow();
        }

        /// <summary>
        /// EventHandler to determined if Details Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When Log Command is called. Displays Log Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayLogWindow();
        }

        /// <summary>
        /// EventHandler to determined if Log Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When Exit Command is called. Exits Syncless
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                Close();
            } catch {}
        }

        /// <summary>
        /// EventHandler to determined if Exit Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When Unmonitor Command is called. Displayed all removable drives
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnmonitorCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayUnmonitorContextMenu();
        }

        /// <summary>
        /// EventHandler to determined if Unmonitor Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnmonitorCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When OptionsCommand is called. Displays Option Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OptionsCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayOptionsWindow();
        }

        /// <summary>
        /// EventHandler to determined if Options Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OptionsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When Minimize Command is called. Minimizes Syncless
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            MinimizeWindow();
        }

        /// <summary>
        /// EventHandler to determined if Minimize Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When ShortcutsCommand is called. Display Shortcuts Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShortcutsCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayShortcutsWindow();
        }

        /// <summary>
        /// EventHandler to determined if Shortcuts Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShortcutsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Code Exected When TimeSyncCommand is called. Initiate Time Sync
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeSyncCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            InitiateTimeSync();
        }

        /// <summary>
        /// EventHandler to determined if TimeSync Command can be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeSyncCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        #endregion
    }
}