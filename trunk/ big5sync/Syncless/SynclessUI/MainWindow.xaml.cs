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
        private string _appPath;
        private bool _closenormally = true;
        private bool _manualSyncEnabled = true;

        private NotificationWatcher _notificationWatcher;
        private PriorityNotificationWatcher _priorityNotificationWatcher;

        private Dictionary<string, double> _syncProgressNotificationDictionary =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, string> _tagStatusNotificationDictionary =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IUIControllerInterface Gui;


        public MainWindow()
        {
            CancellingTags = new HashSet<string>();
            InitializeComponent();
            InitializeSyncless();
            InitializeKeyboardShortcuts();
        }

        public SyncProgressWatcher Watcher { get; set; }
        public SyncProgress CurrentProgress { get; set; }

        public string SelectedTag
        {
            get { return (string) Application.Current.Properties["SelectedTag"]; }
            set { Application.Current.Properties["SelectedTag"] = value; }
        }

        public HashSet<string> CancellingTags { get; set; }

        private string TagFilter
        {
            get { return TxtBoxFilterTag.Text.Trim(); }
            set { TxtBoxFilterTag.Text = value; }
        }

        private void DisplayLoadingAnimation()
        {
            if (Settings.Default.EnableAnimation)
            {
                var loading = (Storyboard) Resources["MainWindowOnLoaded"];
                loading.Begin();
            }
        }

        private void DisplayUnloadingAnimation()
        {
            if (Settings.Default.EnableAnimation)
            {
                var unloading = (Storyboard) Resources["MainWindowUnloaded"];
                unloading.Begin();

                DateTime dateTime = DateTime.Now;

                while (DateTime.Now < dateTime.AddMilliseconds(1000))
                {
                    Dispatcher.Invoke(DispatcherPriority.Background,
                                      (DispatcherOperationCallback) delegate(object unused) { return null; }, null);
                }
            }
        }

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

        /// <summary>
        ///     Starts up the system logic layer and initializes it
        /// </summary>
        private void InitializeSyncless()
        {
            try
            {
                CancellingTags = new HashSet<string>();
                DisplayLoadingAnimation();
                _appPath = Assembly.GetExecutingAssembly().Location;
                Application.Current.Properties["AppPath"] = _appPath;
                Gui = ServiceLocator.GUI;

                if (Gui.Initiate(this))
                {
                    if (Settings.Default.EnableShellIntegration)
                    {
                        RegistryHelper.CreateRegistry(_appPath);
                    }

                    InitializeTagInfoPanel();
                    InitializeTagList();
                    Show();
                    Topmost = true;
                    Topmost = false;

                    if (!Settings.Default.EnableAnimation)
                    {
                        DisplayWelcomeScreen(this, null);
                    }

                    _notificationWatcher = new NotificationWatcher(this);
                    _notificationWatcher.Start();
                    _priorityNotificationWatcher = new PriorityNotificationWatcher(this);
                    _priorityNotificationWatcher.Start();

                    if (Settings.Default.SynchronizeTime)
                    {
                        BackgroundWorker timeWorker = new BackgroundWorker();
                        timeWorker.DoWork += timeWorker_DoWork;
                        timeWorker.RunWorkerAsync();
                    }
                }
                else
                {
                    DialogHelper.ShowError(this, "Syncless Initialization Error",
                                           "Syncless has failed to initialize and will now exit.");
                    _closenormally = false;

                    Close();
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        private void timeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            InitiateTimeSyncHelper();
        }

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

        private void ListBoxTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxTag.SelectedItem == null) return;


            ViewTagInfo(ListBoxTag.SelectedItem.ToString());
        }

        public void ViewTagInfo(string tagname)
        {
            try
            {
                TagView tv = Gui.GetTag(tagname);

                if (tv == null)
                {
                    InitializeTagInfoPanel();
                    return;
                }
                SelectedTag = tagname;
                TagTitle.Text = tv.TagName;

                ListTaggedPath.ItemsSource = tv.PathStringList;

                TagIcon.Visibility = Visibility.Visible;
                TagStatusPanel.Visibility = Visibility.Visible;
                SyncPanel.Visibility = Visibility.Visible;
                BdrTaggedPath.Visibility = tv.PathStringList.Count == 0 ? Visibility.Hidden : Visibility.Visible;

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
        ///     Gets the list of tags and then populates the Tag List Box and keeps a count
        /// </summary>
        public void InitializeTagList()
        {
            try
            {
                TagFilter = string.Empty;
                List<string> taglist = Gui.GetAllTags();

                ListBoxTag.ItemsSource = taglist;
                LblTagCount.Content = "[" + taglist.Count + "/" + taglist.Count + "]";
                SelectedTag = (string) ListBoxTag.SelectedItem;

                if (taglist.Count != 0)
                {
                    SelectedTag = taglist[0];
                    SelectTag(SelectedTag);
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        public void SelectTag(string tagname)
        {
            try
            {
                if (tagname != null)
                {
                    List<string> taglist = Gui.GetAllTags();
                    int index = taglist.IndexOf(tagname);
                    ListBoxTag.SelectedIndex = index;
                    ViewTagInfo(tagname);
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        ///     If lists of tag is empty, reset the UI back to 0, else displayed the first Tag on the list.
        /// </summary>
        private void InitializeTagInfoPanel()
        {
            try
            {
                List<string> taglist = Gui.GetAllTags();

                if (taglist.Count == 0)
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
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        ///     Sets the behavior of the close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
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

        private void MinimizeWindow()
        {
            WindowState = WindowState.Minimized;
            if (Settings.Default.MinimizeToTray)
                ShowInTaskbar = false;
        }

        public void RestoreWindow()
        {
            ShowInTaskbar = true;
            WindowState = WindowState.Normal;
            Topmost = true;
            Topmost = false;
            Focus();
        }

        private void BtnSyncMode_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Hi");
            if (!_manualSyncEnabled)
            {
                bool success = Gui.CancelSwitch(SelectedTag);
                if (!success)
                {
                    DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                           "You cannot change the synchronization mode while it is synchronizing.");
                }
                return;
            }

            try
            {
                if (!Gui.GetTag(SelectedTag).IsLocked && !(GetTagStatus(SelectedTag) == "Finalizing"))
                {
                    if (string.Compare((string) LblSyncMode.Content, "Manual") == 0)
                    {
                        if (Gui.SwitchMode(SelectedTag, TagMode.Seamless))
                        {
                            ProgressBarSync.Value = 0;
                            SetProgressBarColor(0);
                            const string message = "Please Wait";
                            LblStatusText.Content = message;
                            _tagStatusNotificationDictionary[SelectedTag] = message;

                            Console.WriteLine("User -> Switching");
                        }
                        else
                        {
                            DialogHelper.ShowError(this, "Change Synchronization Mode Error",
                                                   SelectedTag + " could not be switched to Seamless Mode.");
                        }
                    }
                    else if (string.Compare((string) LblSyncMode.Content, "Seamless") == 0)
                    {
                        if (Gui.SwitchMode(SelectedTag, TagMode.Manual))
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
                    bool success = Gui.CancelSwitch(SelectedTag);
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

        private void UpdateTagState()
        {
            TagView view = Gui.GetTag(SelectedTag);
            if (view == null)
            {
                return;
            }
            switch (view.TagState)
            {
                case TagState.Seamless:
                    SeamlessMode();
                    break;
                case TagState.Manual:
                    ManualMode();
                    break;
                case TagState.SeamlessToManual:
                case TagState.ManualToSeamless:
                    SwitchingMode();
                    break;
            }
        }

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
            Console.WriteLine("In Seamless Mode");
            LblStatusTitle.Visibility = Visibility.Hidden;
            LblStatusText.Visibility = Visibility.Hidden;
        }

        private void SwitchingMode()
        {
            LblSyncMode.Content = "Please Wait";
            BtnSyncMode.ToolTip = "Please Wait";
            BtnPreview.Visibility = Visibility.Hidden;
            BtnSyncNow.Visibility = Visibility.Hidden;
            BtnSyncMode.SetResourceReference(BackgroundProperty, "ToggleOffBrush");
            LblSyncMode.SetResourceReference(MarginProperty, "ToggleOffMargin");
            LblSyncMode.SetResourceReference(ForegroundProperty, "ToggleOffForeground");

            TagView tv = Gui.GetTag(SelectedTag);
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
            }
            else if (tv.TagState == TagState.SeamlessToManual)
            {
                ProgressBarSync.Visibility = Visibility.Visible;
                LblProgress.Visibility = Visibility.Visible;
            }

            Console.WriteLine("In Switching Mode");
            LblStatusTitle.Visibility = Visibility.Visible;
            LblStatusText.Visibility = Visibility.Visible;
        }

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

                if (_tagStatusNotificationDictionary.ContainsKey(SelectedTag))
                {
                    ProgressBarSync.Visibility = Visibility.Visible;
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

                Console.WriteLine("In Manual Mode");
                LblStatusTitle.Visibility = Visibility.Visible;
                LblStatusText.Visibility = Visibility.Visible;

                if (SelectedTag != null)
                {
                    TagView tv = Gui.GetTag(SelectedTag);

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
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        private void CancelButtonMode()
        {
            LblSyncNow.Content = "Cancel Sync";
            LblSyncNow.ToolTip = "Cancel Current Synchronization";
        }

        private void SyncButtonMode()
        {
            LblSyncNow.Content = "Sync Now";
            LblSyncNow.ToolTip = "Start Manual Synchronization";
        }

        private void BtnSyncNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PathChanged();

                if (LblSyncNow.Content.Equals("Sync Now"))
                {
                    BtnSyncNow.IsEnabled = false;

                    try
                    {
                        if (Gui.GetTag(SelectedTag).PathStringList.Count > 1)
                        {
                            if (Gui.StartManualSync(SelectedTag))
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
                            SyncButtonMode();
                        }
                    }
                    catch (UnhandledException)
                    {
                        DialogHelper.DisplayUnhandledExceptionMessage(this);
                    }

                    BtnSyncNow.IsEnabled = true;
                }
                else
                {
                    BtnSyncNow.IsEnabled = false;
                    bool success = Gui.CancelManualSync(SelectedTag);
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

        public bool CreateTag(string tagName)
        {
            try
            {
                try
                {
                    TagView tv = Gui.CreateTag(tagName);
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

        private void TxtBoxFilterTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (Gui != null)
                {
                    List<string> taglist = Gui.GetAllTags();
                    var filteredtaglist = new List<string>();

                    int initial = taglist.Count;

                    foreach (string x in taglist)
                    {
                        if (x.ToLower().Contains(TagFilter.ToLower()))
                            filteredtaglist.Add(x);
                    }

                    int after = filteredtaglist.Count;

                    LblTagCount.Content = "[" + after + "/" + initial + "]";

                    ListBoxTag.ItemsSource = null;
                    ListBoxTag.ItemsSource = filteredtaglist;
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

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

                    if (!Gui.GetTag(currentTag).IsLocked)
                    {
                        if (ListTaggedPath.SelectedIndex == -1)
                        {
                            DialogHelper.ShowError(this, "No Path Selected", "Please select a path to untag.");
                        }
                        else
                        {
                            TagView tv = Gui.GetTag(currentTag);

                            if (tv != null)
                            {
                                if (!tv.IsLocked)
                                {
                                    int count = Gui.Untag(currentTag,
                                                          new DirectoryInfo((string) ListTaggedPath.SelectedValue));

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

                                InitializeTagInfoPanel();

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

        public void ResetTagSyncStatus(string currentTag)
        {
            _syncProgressNotificationDictionary.Remove(currentTag);
            _tagStatusNotificationDictionary.Remove(currentTag);
            LblStatusText.Content = "";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (_closenormally)
                {
                    // Prepares the SLL for termination
                    if (Gui.PrepareForTermination())
                    {
                        bool result = DialogHelper.ShowWarning(this, "Exit", "Are you sure you want to exit Syncless?" +
                                                                             "\nExiting Syncless will disable seamless synchronization.");

                        if (result)
                        {
                            // Terminates the SLL and closes the UI
                            Gui.Terminate();
                            TerminateNow(true);
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        bool result = DialogHelper.ShowWarning(this, "Exit",
                                                               "Are you sure you want to exit Syncless?" +
                                                               "\nAll current synchronization operations will be completed and" +
                                                               "\nany unfinished synchronization operations will be removed.");

                        if (!result)
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            DialogWindow terminationWindow = DialogHelper.ShowIndeterminate(this,
                                                                                            "Termination in Progress",
                                                                                            "Please wait for the current synchronization to complete.");
                            TaskbarIcon.Visibility = Visibility.Hidden;
                            terminationWindow.Show();
                            var bgWorker = new BackgroundWorker();
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

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var terminationWindow = e.Argument as DialogWindow;
            Gui.Terminate();
            e.Result = terminationWindow;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var terminationWindow = e.Result as DialogWindow;
            terminationWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                                                                                              {
                                                                                                  terminationWindow.
                                                                                                      CannotBeClosed =
                                                                                                      false;
                                                                                                  terminationWindow.
                                                                                                      Close();
                                                                                              }));

            TerminateNow(false);
        }

        private void TerminateNow(bool showAnimation)
        {
            SaveApplicationSettings();
            if (Settings.Default.EnableShellIntegration == false)
            {
                RegistryHelper.RemoveRegistry();
            }
            _notificationWatcher.Stop();
            _priorityNotificationWatcher.Stop();

            if (showAnimation)
                DisplayUnloadingAnimation();
        }

        private void RemoveTagRightClick_Click(object sender, RoutedEventArgs e)
        {
            RemoveTag();
        }

        private void DetailsRightClick_Click(object sender, RoutedEventArgs e)
        {
            DisplayTagDetailsWindow();
        }

        private void TagRightClick_Click(object sender, RoutedEventArgs e)
        {
            DisplayTagWindow();
        }

        private void DisplayTagDetailsWindow()
        {
            if (SelectedTag != null)
            {
                if (!Gui.GetTag(SelectedTag).IsLocked)
                {
                    var tdw = new TagDetailsWindow(this, SelectedTag);
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

        private void DisplayLogWindow()
        {
            var lw = new LogWindow(this);
        }

        private void BtnOptions_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DisplayOptionsWindow();
        }

        private void DisplayOptionsWindow()
        {
            if (Application.Current.Properties["OptionsWindowIsOpened"] == null)
            {
                Application.Current.Properties["OptionsWindowIsOpened"] = false;
            }

            if (!(bool) Application.Current.Properties["OptionsWindowIsOpened"])
            {
                var ow = new OptionsWindow(this);
                ow.ShowDialog();
            }
        }

        private static void OpenSynclessWebpage()
        {
            Process.Start(new ProcessStartInfo("http://code.google.com/p/big5sync/"));
        }

        private void SynclessLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenSynclessWebpage();
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_manualSyncEnabled || Gui.GetTag(SelectedTag).IsLocked || GetTagStatus(SelectedTag) == "Finalizing")
            {
                DialogHelper.ShowError(this, SelectedTag + " is Synchronizing",
                                       "You cannot preview while it is synchronizing.");
                return;
            }

            if (Gui.GetTag(SelectedTag).PathStringList.Count > 1)
            {
                var psw = new PreviewSyncWindow(this, SelectedTag);
                psw.ShowDialog();
            }
            else
            {
                DialogHelper.ShowError(this, "Nothing to Preview",
                                       "You can only preview only when there are two or more folders.");
            }
        }

        private void driveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var source = (MenuItem) sender;
                var driveletter = (string) source.Header;
                var drive = new DriveInfo(driveletter);
                if (!Gui.AllowForRemoval(drive))
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

        private void SynclessLogoContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            LogoHighlight.Visibility = Visibility.Visible;
        }

        private void BtnShortcuts_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DisplayShortcutsWindow();
        }

        private void TagIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DisplayTagDetailsWindow();
        }

        private void LayoutRoot_Drop(object sender, DragEventArgs e)
        {
            try
            {
                HideDropIndicator();

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                    if (foldernames != null)
                        foreach (string i in foldernames)
                        {
                            string path = i;

                            // convert potential shortcuts into folders
                            string shortcutfolderpath = FileHelper.GetShortcutTargetFile(i);
                            if (shortcutfolderpath != null)
                            {
                                path = shortcutfolderpath;
                            }

                            // to detect folders
                            try
                            {
                                var folder = new DirectoryInfo(path);
                                if (folder.Exists && !FileHelper.IsFile(path))
                                {
                                    var tw = new TagWindow(this, path, SelectedTag, false);
                                }
                            }
                            catch
                            {
                            }
                        }
                }
                TxtBoxFilterTag.IsHitTestVisible = true;
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        private void LayoutRoot_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                TxtBoxFilterTag.IsHitTestVisible = false;
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                    if (foldernames != null)
                        foreach (string i in foldernames)
                        {
                            string path = i;

                            // convert potential shortcuts into folders
                            string shortcutfolderpath = FileHelper.GetShortcutTargetFile(path);
                            if (shortcutfolderpath != null)
                            {
                                path = shortcutfolderpath;
                            }

                            // to detect folders
                            try
                            {
                                var folder = new DirectoryInfo(path);
                                if (folder.Exists && !FileHelper.IsFile(path))
                                {
                                    ShowDropIndicator();
                                }
                            }
                            catch
                            {
                            }
                        }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        private void LayoutRoot_DragLeave(object sender, DragEventArgs e)
        {
            HideDropIndicator();
            TxtBoxFilterTag.IsHitTestVisible = true;
        }

        private void ShowDropIndicator()
        {
            DropIndicator.Visibility = Visibility.Visible;
            DropIndicator.Focusable = false;
        }

        private void HideDropIndicator()
        {
            DropIndicator.Visibility = Visibility.Hidden;
        }

        public void NotifyCancelComplete(string tagname)
        {
            if (SelectedTag == tagname)
            {
                SyncButtonMode();
                LblStatusText.Content = "";
                _tagStatusNotificationDictionary.Remove(SelectedTag);
                BtnSyncNow.IsEnabled = true;
                ProgressBarSync.IsIndeterminate = false;
                ProgressBarSync.Visibility = Visibility.Hidden;
                LblProgress.Visibility = Visibility.Hidden;
                _syncProgressNotificationDictionary.Remove(SelectedTag);
                CancellingTags.Remove(tagname);
            }
            else
            {
                CancellingTags.Remove(tagname);
            }
        }

        public void NotifyAutoSyncComplete(string path)
        {
            NotifyBalloon("Synchronization Completed", path + " is now synchronized.");
        }

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

            LblStatusText.Content = message;
            _tagStatusNotificationDictionary[tagname] = message;
        }

        public double GetSyncProgressPercentage(string tagname)
        {
            return CurrentProgress.TagName == tagname
                       ? CurrentProgress.PercentComplete
                       : _syncProgressNotificationDictionary[tagname];
        }

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

        public void SetSyncProgress(string tagname, SyncProgress progress)
        {
        }

        private void SetProgressBarColor(double percentageComplete)
        {
            byte rcolor = 0, gcolor = 0;
            const byte bcolor = 0;

            if (percentageComplete <= 50)
            {
                rcolor = 211;
                gcolor = (byte) (percentageComplete/50*211);
            }
            else
            {
                rcolor = (byte) ((100 - percentageComplete)/50*211);
                gcolor = 211;
            }

            ProgressBarSync.Foreground = new SolidColorBrush(Color.FromArgb(255, rcolor, gcolor, bcolor));
        }

        #region Progress Notification

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

                if (Gui.GetTag(SelectedTag).IsSeamless)
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
                if (Gui.GetTagState(SelectedTag) == TagState.ManualToSeamless ||
                    Gui.GetTagState(SelectedTag) == TagState.SeamlessToManual)
                {
                    SwitchingMode();
                }
            }

            _syncProgressNotificationDictionary[tagname] = percentageComplete;
            _tagStatusNotificationDictionary[tagname] = message;
        }

        #endregion

        #region Events Handlers for Taskbar Icon Context Menu Items

        private void TaskbarExitItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void TaskbarOptionsItem_Click(object sender, RoutedEventArgs e)
        {
            DisplayOptionsWindow();
        }

        private void TaskbarTagItem_Click(object sender, RoutedEventArgs e)
        {
            DisplayTagWindow();
        }

        private void TaskbarUnmonitorItem_Click(object sender, RoutedEventArgs e)
        {
            DisplayUnmonitorContextMenu();
        }

        private void TaskbarOpenItem_Click(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        #endregion

        #region Tag Info Panel Context Menu

        private void OpenInExplorerRightClick_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderInWindowsExplorer();
        }

        private void UntagRightClick_Click(object sender, RoutedEventArgs e)
        {
            Untag();
        }

        private void ListTaggedPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFolderInWindowsExplorer();
        }

        private void OpenFolderInWindowsExplorer()
        {
            var path = (string) ListTaggedPath.SelectedItem;
            if (path != "")
            {
                var runExplorer = new ProcessStartInfo();
                runExplorer.FileName = "explorer.exe";
                runExplorer.Arguments = path;
                Process.Start(runExplorer);
            }
        }

        #endregion

        #region Implements Methods & Supporting Methods in IUIInterface

        public string getAppPath()
        {
            return Path.GetDirectoryName(_appPath);
        }

        public void DriveChanged()
        {
            UpdateTagList_ThreadSafe();
        }

        public void TagsChanged()
        {
            UpdateTagList_ThreadSafe();
        }

        public void TagChanged(string tagName)
        {
            UpdateTagInfo_ThreadSafe(tagName);
        }

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

        private void UpdateTagList_ThreadSafe()
        {
            try
            {
                ListBoxTag.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                  (Action) (() =>
                                                                {
                                                                    List<string> taglist = Gui.GetAllTags();
                                                                    ListBoxTag.ItemsSource = taglist;
                                                                    LblTagCount.Content = "[" + taglist.Count + "/" +
                                                                                          taglist.Count + "]";
                                                                }));
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        public void PathChanged()
        {
            try
            {
                ListTaggedPath.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                      (Action) (() =>
                                                                    {
                                                                        if (SelectedTag == null)
                                                                        {
                                                                            return;
                                                                        }
                                                                        TagView tv = Gui.GetTag(SelectedTag);

                                                                        ListTaggedPath.ItemsSource = tv.PathStringList;
                                                                        BdrTaggedPath.Visibility =
                                                                            tv.PathStringList.Count == 0
                                                                                ? Visibility.Hidden
                                                                                : Visibility.Visible;
                                                                    }));
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        #endregion

        #region Commandline Interface: Tag/Untag

        public void ProcessCommandLine(string[] args)
        {
            if (args.Length != 0)
            {
                CommandLineHelper.ProcessCommandLine(args, this);
            }
        }

        public void CliTag(string clipath)
        {
            string tagname = "";

            if (FileHelper.IsFile(clipath))
            {
                DialogHelper.ShowError(this, "Tagging not Allowed", "You cannot tag a file.");
                return;
            }

            try
            {
                var di = new DirectoryInfo(clipath);
                tagname = di.Name;
            }
            catch (Exception)
            {
                DialogHelper.ShowError(this, "Invalid Folder", "You cannot tag this folder.");
                return;
            }

            var tw = new TagWindow(this, clipath, tagname, true);
        }

        public void CliUntag(string clipath)
        {
            if (FileHelper.IsFile(clipath))
            {
                DialogHelper.ShowError(this, "Untagging not Allowed", "You cannot tag a file.");
                return;
            }

            DirectoryInfo di = null;

            try
            {
                new DirectoryInfo(clipath);
            }
            catch (ArgumentException)
            {
                DialogHelper.ShowError(this, "Invalid Folder", "You cannot untag this folder.");
                return;
            }

            var tw = new UntagWindow(this, clipath, true);
        }

        public void CliClean(string clipath)
        {
            if (FileHelper.IsFile(clipath))
            {
                DialogHelper.ShowError(this, "Cleaning not Allowed", "You cannot clean a file.");
                return;
            }

            DirectoryInfo di = null;

            try
            {
                di = new DirectoryInfo(clipath);
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

        #region Keyboard Shortcuts

        private void InitializeKeyboardShortcuts()
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

        private void InitializeTimeSyncCommand()
        {
            var TimeSyncCommand = new RoutedCommand();

            var cb = new CommandBinding(TimeSyncCommand, TimeSyncCommandExecute, TimeSyncCommandCanExecute);
            CommandBindings.Add(cb);

            BtnTimeSync.Command = TimeSyncCommand;

            var kg = new KeyGesture(Key.Y, ModifierKeys.Control);
            var ib = new InputBinding(TimeSyncCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeExitCommand()
        {
            var ExitCommand = new RoutedCommand();

            var cb = new CommandBinding(ExitCommand, ExitCommandExecute, ExitCommandCanExecute);
            CommandBindings.Add(cb);

            var kg = new KeyGesture(Key.W, ModifierKeys.Control);
            var ib = new InputBinding(ExitCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeLogCommand()
        {
            var LogCommand = new RoutedCommand();

            var cb = new CommandBinding(LogCommand, LogCommandExecute, LogCommandCanExecute);
            CommandBindings.Add(cb);

            BtnLog.Command = LogCommand;

            var kg = new KeyGesture(Key.L, ModifierKeys.Control);
            var ib = new InputBinding(LogCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeShortcutsCommand()
        {
            var ShortcutsCommand = new RoutedCommand();

            var cb = new CommandBinding(ShortcutsCommand, ShortcutsCommandExecute, ShortcutsCommandCanExecute);
            CommandBindings.Add(cb);

            var kg = new KeyGesture(Key.S, ModifierKeys.Control);
            var ib = new InputBinding(ShortcutsCommand, kg);
            var kg1 = new KeyGesture(Key.OemQuestion, ModifierKeys.Shift);
            var ib1 = new InputBinding(ShortcutsCommand, kg1);
            InputBindings.Add(ib);
            InputBindings.Add(ib1);
        }

        private void InitializeMinimizeCommand()
        {
            var MinimizeCommand = new RoutedCommand();

            var cb = new CommandBinding(MinimizeCommand, MinimizeCommandExecute, MinimizeCommandCanExecute);
            CommandBindings.Add(cb);

            var kg = new KeyGesture(Key.M, ModifierKeys.Control);
            var ib = new InputBinding(MinimizeCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeOptionsCommand()
        {
            var OptionsCommand = new RoutedCommand();

            var cb = new CommandBinding(OptionsCommand, OptionsCommandExecute, OptionsCommandCanExecute);
            CommandBindings.Add(cb);

            var kg = new KeyGesture(Key.O, ModifierKeys.Control);
            var ib = new InputBinding(OptionsCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeUnmonitorCommand()
        {
            var UnmonitorCommand = new RoutedCommand();

            var cb = new CommandBinding(UnmonitorCommand, UnmonitorCommandExecute, UnmonitorCommandCanExecute);
            CommandBindings.Add(cb);

            BtnUnmonitor.Command = UnmonitorCommand;

            var kg = new KeyGesture(Key.I, ModifierKeys.Control);
            var ib = new InputBinding(UnmonitorCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeTagDetailsCommand()
        {
            var DetailsCommand = new RoutedCommand();

            var cb = new CommandBinding(DetailsCommand, DetailsCommandExecute, DetailsCommandCanExecute);
            CommandBindings.Add(cb);

            BtnDetails.Command = DetailsCommand;

            var kg = new KeyGesture(Key.D, ModifierKeys.Control);
            var ib = new InputBinding(DetailsCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeUntagCommand()
        {
            var UntagCommand = new RoutedCommand();

            var cb = new CommandBinding(UntagCommand, UntagCommandExecute, UntagCommandCanExecute);
            CommandBindings.Add(cb);

            btnUntag.Command = UntagCommand;

            var kg = new KeyGesture(Key.U, ModifierKeys.Control);
            var ib = new InputBinding(UntagCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeTagCommand()
        {
            var TagCommand = new RoutedCommand();

            var cb = new CommandBinding(TagCommand, TagCommandExecute, TagCommandCanExecute);
            CommandBindings.Add(cb);

            btnTag.Command = TagCommand;

            var kg = new KeyGesture(Key.T, ModifierKeys.Control);
            var ib = new InputBinding(TagCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeRemoveTagCommand()
        {
            var RemoveTagCommand = new RoutedCommand();

            var cb = new CommandBinding(RemoveTagCommand, RemoveTagCommandExecute, RemoveTagCommandCanExecute);
            CommandBindings.Add(cb);

            btnRemove.Command = RemoveTagCommand;

            var kg = new KeyGesture(Key.R, ModifierKeys.Control);
            var ib = new InputBinding(RemoveTagCommand, kg);
            InputBindings.Add(ib);
        }

        private void InitializeCreateTagCommand()
        {
            var CreateTagCommand = new RoutedCommand();

            var cb = new CommandBinding(CreateTagCommand, CreateTagCommandExecute, CreateTagCommandCanExecute);
            CommandBindings.Add(cb);

            BtnCreate.Command = CreateTagCommand;

            var kg = new KeyGesture(Key.N, ModifierKeys.Control);
            var ib = new InputBinding(CreateTagCommand, kg);
            InputBindings.Add(ib);
        }

        private void CreateTagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            var ctw = new CreateTagWindow(this);
            ctw.ShowDialog();
        }

        private void CreateTagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void RemoveTagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            RemoveTag();
        }

        private void RemoveTag()
        {
            try
            {
                if (SelectedTag != null)
                {
                    if (!Gui.GetTag(SelectedTag).IsLocked)
                    {
                        bool result = DialogHelper.ShowWarning(this, "Remove Tag",
                                                               "Are you sure you want to remove " + SelectedTag +
                                                               "?");

                        if (result)
                        {
                            if (!Gui.GetTag(SelectedTag).IsLocked)
                            {
                                bool success = Gui.DeleteTag(SelectedTag);
                                if (success)
                                {
                                    _syncProgressNotificationDictionary.Remove(SelectedTag);
                                    _tagStatusNotificationDictionary.Remove(SelectedTag);

                                    InitializeTagList();
                                    InitializeTagInfoPanel();
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


        private void RemoveTagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void TagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayTagWindow();
        }

        private void DisplayTagWindow()
        {
            var tw = new TagWindow(this, "", SelectedTag, false);
        }

        private void TagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void UntagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Untag();
        }

        private void UntagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void DetailsCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayTagDetailsWindow();
        }

        private void DetailsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void LogCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayLogWindow();
        }

        private void LogCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ExitCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void ExitCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void UnmonitorCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayUnmonitorContextMenu();
        }

        private void DisplayUnmonitorContextMenu()
        {
            var driveMenu = new ContextMenu();

            List<string> removableDrives = DriveHelper.GetUSBDriveLetters();
            if (removableDrives.Count == 0)
            {
                var driveMenuItem = new MenuItem();
                driveMenuItem.Header = "No Removable Drives Found";
                driveMenu.Items.Add(driveMenuItem);
            }
            else
            {
                foreach (string letter in removableDrives)
                {
                    var driveMenuItem = new MenuItem();
                    driveMenuItem.Header = letter;
                    driveMenuItem.Click += driveMenuItem_Click;
                    driveMenu.Items.Add(driveMenuItem);
                }
            }

            driveMenu.PlacementTarget = this;
            driveMenu.IsOpen = true;
        }

        private void UnmonitorCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OptionsCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayOptionsWindow();
        }

        private void OptionsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void MinimizeCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            MinimizeWindow();
        }

        private void MinimizeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ShortcutsCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DisplayShortcutsWindow();
        }

        private void ShortcutsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void TimeSyncCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            InitiateTimeSync();
        }

        private void TimeSyncCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

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

        private void DisplayShortcutsWindow()
        {
            var sw = new ShortcutsWindow(this);

            sw.ShowDialog();
        }

        #endregion

        #region Application Settings

        private void SaveApplicationSettings()
        {
            Settings.Default.Save();
        }

        private void ListBoxTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && ListBoxTag.Items.Count > 0)
            {
                RemoveTag();
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #endregion
    }
}