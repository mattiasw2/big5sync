using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Syncless.Core;
using Syncless.Core.Exceptions;
using Syncless.Core.View;
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
        private const string BI_DIRECTIONAL = "Bi-Dir..";
        private const string UNI_DIRECTIONAL = "Uni-Dir..";
        private string _appPath;
        private bool _closenormally = true;
        private bool _firstopen = true;
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
            InitializeComponent();
            InitializeSyncless();
            InitializeKeyboardShortcuts();
        }

        public string SelectedTag
        {
            get { return (string) Application.Current.Properties["SelectedTag"]; }
            set { Application.Current.Properties["SelectedTag"] = value; }
        }

        private string TagFilter
        {
            get { return TxtBoxFilterTag.Text.Trim(); }
        }

        /// <summary>
        ///     Starts up the system logic layer and initializes it
        /// </summary>
        private void InitializeSyncless()
        {
            try
            {
                _appPath = Assembly.GetExecutingAssembly().Location;
                Application.Current.Properties["AppPath"] = _appPath;
                Gui = ServiceLocator.GUI;

                if (Gui.Initiate(this))
                {
                    if (Settings.Default.EnableShellIntegration == true)
                    {
                        RegistryHelper.CreateRegistry(_appPath);
                    }

                    InitializeTagInfoPanel();
                    InitializeTagList();
                }
                else
                {
                    DialogsHelper.ShowError("Syncless Initialization Error",
                                            "Syncless has failed to initialize and will now exit.");
                    _closenormally = false;

                    Close();
                }

                _notificationWatcher = new NotificationWatcher(this);
                _notificationWatcher.Start();
                _priorityNotificationWatcher = new PriorityNotificationWatcher();
                _priorityNotificationWatcher.Start();
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
            }
        }

        public void NotifySyncAnalyzing(string tagname)
        {
            const string message = "Analyzing Folders";
            const double percentageComplete = 0;

            if (SelectedTag == tagname)
            {
                LblStatusText.Content = message;
                ProgressBarSync.Value = percentageComplete;
            }

            _syncProgressNotificationDictionary[tagname] = percentageComplete;
            _tagStatusNotificationDictionary[tagname] = message;
        }

        public void NotifySyncStart(string tagname)
        {
            const string message = "Synchronization Started";

            if (SelectedTag == tagname)
            {
                LblStatusText.Content = message;
            }

            _tagStatusNotificationDictionary[tagname] = message;
            NotifyBalloon("Synchronization Started", tagname + " is being synchronized.");
        }

        public void NotifySyncCompletion(string tagname)
        {
            string message = "Synchronization completed at " + DateTime.Now;

            if (SelectedTag == tagname)
            {
                LblStatusText.Content = message;
                BtnSyncNow.IsEnabled = true;
                BtnPreview.IsEnabled = true;
            }

            _tagStatusNotificationDictionary[tagname] = message;
            NotifyBalloon("Synchronization Completed", tagname + " is now synchronized.");
        }

        public double GetSyncProgressPercentage(string tagname)
        {
            return _syncProgressNotificationDictionary[tagname];
        }

        public string GetTagStatus(string tagname)
        {
            string status = _tagStatusNotificationDictionary[tagname];

            return status;
        }

        public void SetSyncProgress(string tagname, SyncProgress progress)
        {
            if (SelectedTag == tagname)
            {
                ProgressBarSync.SetValue(ProgressBar.ValueProperty, progress.PercentComplete);
                LblStatusText.Content = progress.Message;
                SetProgressBarColor(progress.PercentComplete);
            }

            _syncProgressNotificationDictionary[tagname] = progress.PercentComplete;
            _tagStatusNotificationDictionary[tagname] = progress.Message;
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

        private void NotifyBalloon(string title, string text)
        {
            TaskbarIcon.ShowBalloonTip(title, text, BalloonIcon.Info);
        }

        private void ListBoxTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxTag.SelectedItem == null) return;

            SelectedTag = ListBoxTag.SelectedItem.ToString();
            ViewTagInfo(SelectedTag);
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
                TagTitle.Text = tagname;
                // tag.direction not implemented yet

                if (tv.IsSeamless)
                {
                    SeamlessMode();
                }
                else
                {
                    _manualSyncEnabled = !tv.Locked;
                    ManualMode();
                }

                LblStatusText.Content = "";
                ListTaggedPath.ItemsSource = tv.PathStringList;

                TagIcon.Visibility = Visibility.Visible;
                TagStatusPanel.Visibility = Visibility.Visible;
                SyncPanel.Visibility = Visibility.Visible;
                BdrTaggedPath.Visibility = tv.PathStringList.Count == 0 ? Visibility.Hidden : Visibility.Visible;

                if (_syncProgressNotificationDictionary.ContainsKey(tagname))
                {
                    double percentageComplete = GetSyncProgressPercentage(tagname);
                    SetProgressBarColor(percentageComplete);
                    string status = GetTagStatus(tagname);

                    ProgressBarSync.Value = percentageComplete;
                    LblStatusText.Content = status;
                }
                else
                {
                    ProgressBarSync.Value = 0;
                    
                    if(_tagStatusNotificationDictionary.ContainsKey(tagname)) {
                        LblStatusText.Content = GetTagStatus(tagname);
                    } else
                    {
                        LblStatusText.Content = "";
                    }
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
            }
        }

        /// <summary>
        ///     Gets the list of tags and then populates the Tag List Box and keeps a count
        /// </summary>
        public void InitializeTagList()
        {
            try
            {
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
                DisplayUnhandledExceptionMessage();
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
                DisplayUnhandledExceptionMessage();
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
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
            }
        }

        /// <summary>
        ///     Makes the title bar draggable and movable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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
            ShowInTaskbar = false;
        }

        private void RestoreWindow()
        {
            ShowInTaskbar = true;
            WindowState = WindowState.Normal;
            Topmost = true;
            Topmost = false;
            Focus();
        }

        private void BtnDirection_Click(object sender, RoutedEventArgs e)
        {
            if (string.Compare((string) LblDirection.Content, UNI_DIRECTIONAL) == 0)
            {
                LblDirection.Content = BI_DIRECTIONAL;
            }
            else
            {
                LblDirection.Content = UNI_DIRECTIONAL;
            }
        }

        private void BtnSyncMode_Click(object sender, RoutedEventArgs e)
        {
            if (!_manualSyncEnabled)
            {
                return;
            }
            try
            {
                if (string.Compare((string) LblSyncMode.Content, "Manual") == 0)
                {
                    if (Gui.MonitorTag(SelectedTag, true))
                    {
                        SeamlessMode();
                    }
                    else
                    {
                        DialogsHelper.ShowError("Change Synchronization Mode Error",
                                                "' " + SelectedTag + " ' could not be set into Seamless Mode.");
                    }
                }
                else
                {
                    if (Gui.MonitorTag(SelectedTag, false))
                    {
                        ManualMode();
                    }
                    else
                    {
                        DialogsHelper.ShowError("Change Synchronization Mode Error",
                                                "' " + SelectedTag + " ' could not be set into Manual Mode.");
                    }
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
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
            //ProgressBarSync.Visibility = System.Windows.Visibility.Hidden;
            //LblProgress.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ManualMode()
        {
            LblSyncMode.Content = "Manual";
            BtnSyncMode.ToolTip = "Switch to Seamless Synchronization Mode";
            BtnPreview.Visibility = Visibility.Visible;
            BtnSyncNow.Visibility = Visibility.Visible;
            BtnSyncMode.SetResourceReference(BackgroundProperty, "ToggleOffBrush");
            LblSyncMode.SetResourceReference(MarginProperty, "ToggleOffMargin");
            LblSyncMode.SetResourceReference(ForegroundProperty, "ToggleOffForeground");
            ProgressBarSync.Visibility = Visibility.Visible;
            LblProgress.Visibility = Visibility.Visible;
			
            if(_manualSyncEnabled)
            {
                BtnSyncNow.IsEnabled = true;
                BtnPreview.IsEnabled = true;
            }
            else
            {
                BtnSyncNow.IsEnabled = false;
                BtnPreview.IsEnabled = false;
            }
        }

        private void BtnSyncNow_Click(object sender, RoutedEventArgs e)
        {
            if (!_manualSyncEnabled)
            {
                return;
            }
            try
            {
                if (Gui.GetTag(SelectedTag).PathStringList.Count > 1)
                {
                    if (Gui.StartManualSync(SelectedTag))
                    {
                        const string message = "Synchronization request has been queued.";
                        LblStatusText.Content = message;
                        _tagStatusNotificationDictionary[SelectedTag] = message;
                        BtnSyncNow.IsEnabled = false;
                        BtnPreview.IsEnabled = false;
                    }
                    else
                    {
                        DialogsHelper.ShowError("Synchronization Error", "'" + SelectedTag + "' could not be synchronized.");
                    }
                } 
                else
                {
                    DialogsHelper.ShowError("Nothing to Sync", "Please sync only when there are two or more folders to sync.");
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
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
                        DialogsHelper.ShowError("Tag Creation Error", "Tag could not be created.");
                    }
                }
                catch (TagAlreadyExistsException)
                {
                    return false;
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
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
                DisplayUnhandledExceptionMessage();
            }
        }

        private void Untag()
        {
            try
            {
                if (!ListTaggedPath.HasItems)
                {
                    /*
                    string messageBoxText = "There is nothing to untag.";
                    string caption = "Nothing to Untag";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                    */
                }
                else
                {
                    if (ListTaggedPath.SelectedIndex == -1)
                    {
                        DialogsHelper.ShowError("No Path Selected", "Please select a path to untag.");
                    }
                    else
                    {
                        TagView tv = Gui.GetTag((string) TagTitle.Text);

                        if (tv != null)
                        {
                            if(tv.IsSyncing)
                            {
                                Gui.Untag(tv.TagName, new DirectoryInfo((string)ListTaggedPath.SelectedValue));

                                SelectTag(tv.TagName);
                            } else
                            {
                                DialogsHelper.ShowError(tv.TagName + " is Synchronizing",
                                                        "You cannot untag while a tag is synchronizing.");
                            }
                        }
                        else
                        {
                            DialogsHelper.ShowError("Tag Does Not Exist",
                                                    "The tag which you tried to untag does not exist.");

                            InitializeTagInfoPanel();

                            return;
                        }
                    }
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
            }
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
                        bool result = DialogsHelper.ShowWarning("Exit", "Are you sure you want to exit Syncless?" +
                                                                        "\nExiting Syncless will disable seamless synchronization.");

                        if (result)
                        {
                            // Terminates the SLL and closes the UI
                            Gui.Terminate();
                            SaveApplicationSettings();
                            if (Settings.Default.EnableShellIntegration == false)
                            {
                                RegistryHelper.RemoveRegistry();
                            }
                            _notificationWatcher.Stop();
                            _priorityNotificationWatcher.Stop();
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        DialogsHelper.ShowError("Syncless Termination Error",
                                                "Syncless is not ready for termination. Please try again later.");

                        e.Cancel = true;
                    }
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
            }
        }

        private void RemoveTagRightClick_Click(object sender, RoutedEventArgs e)
        {
            RemoveTag();
        }

        private void ViewTagDetails()
        {
            if(SelectedTag != null)
            {
                var tdw = new TagDetailsWindow(SelectedTag, this);
                tdw.ShowDialog();
            }
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
                var ow = new OptionsWindow();
                ow.ShowDialog();
            }
        }

        private void OpenSynclessWebpage()
        {
            Process.Start(new ProcessStartInfo("http://code.google.com/p/big5sync/"));
        }

        private void SynclessLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenSynclessWebpage();
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_manualSyncEnabled)
            {
                return;
            }
            /*
            string messageBoxText = "This feature will come in a future version of Syncless.";
            string caption = "Feature Not Implemented Yet";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Exclamation;

            MessageBox.Show(messageBoxText, caption, button, icon);
			*/

            if (Gui.GetTag(SelectedTag).PathStringList.Count > 1)
            {
                var psw = new PreviewSyncWindow(this, SelectedTag);
                psw.ShowDialog();
            }
            else
            {
                DialogsHelper.ShowError("Nothing to Preview", "Please preview only when there are two or more folders to sync.");
            }
        }

        private List<DriveInfo> GetAllRemovableDrives()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var removableDrives = new List<DriveInfo>();

            foreach (DriveInfo di in allDrives)
            {
                if (di.DriveType == DriveType.Removable)
                {
                    removableDrives.Add(di);
                }
            }

            return removableDrives;
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
                    DialogsHelper.ShowError("Drive Removal Error",
                                            "Syncless could not prepare " + driveletter + " for removal.");
                }
                else
                {
                    DialogsHelper.ShowInformation("Monitoring Stopped for " + driveletter,
                                                  "Syncless has stopped all monitoring (seamless) operations on " +
                                                  driveletter + " " + "\nYou may proceed to remove it safely.");
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
            }
        }

        private void LayoutRoot_Drop(object sender, DragEventArgs e)
        {
            HideDropIndicator();

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (string i in foldernames)
                {
                    var folder = new DirectoryInfo(i);
                    if (folder.Exists)
                    {
                        var tw = new TagWindow(this, i, SelectedTag);
                    }
                }
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
            ViewTagDetails();
        }

        public void DisplayUnhandledExceptionMessage()
        {
            DialogsHelper.ShowError("Unexpected Error",
                                    "An unexpected error has occured. \n\nPlease help us by - \n 1. Submitting the debug.log in your Syncless Application Folder to big5.syncless@gmail.com \n 2. Raise it as an issue on our GCPH @ http://code.google.com/p/big5sync/issues/list\n\n Please restart Syncless.");
        }

        private void LayoutRoot_DragEnter(object sender, DragEventArgs e)
        {
            TxtBoxFilterTag.IsHitTestVisible = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (string i in foldernames)
                {
                    var folder = new DirectoryInfo(i);
                    if (folder.Exists)
                    {
                        ShowDropIndicator();
                    }
                }
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
            var tw = new TagWindow(this, "", SelectedTag);
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
            RepopulateTagList_ThreadSafe();
        }

        public void TagChanged()
        {
            RepopulateTagList();
        }

        private void RepopulateTagList()
        {
            string current = SelectedTag;

            InitializeTagList();

            SelectTag(current);
        }

        private void RepopulateTagList_ThreadSafe()
        {
            try
            {
                List<string> taglist = Gui.GetAllTags();

                ListBoxTag.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                  (Action) (() =>
                                                                {
                                                                    ListBoxTag.ItemsSource = taglist;
                                                                    LblTagCount.Content = "[" + taglist.Count + "/" +
                                                                                          taglist.Count + "]";
                                                                }));

                ListTaggedPath.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                      (Action) (() =>
                                                                    {
                                                                        if (SelectedTag == null)
                                                                        {
                                                                            return;
                                                                        }
                                                                        TagView tv = Gui.GetTag(SelectedTag);
                                                                        if (tv == null)
                                                                        {
                                                                            return;
                                                                        }
                                                                        if (tv.IsSeamless)
                                                                        {
                                                                            SeamlessMode();
                                                                        }
                                                                        else
                                                                        {
                                                                            ManualMode();
                                                                        }
                                                                        ListTaggedPath.ItemsSource = tv.PathStringList;
                                                                    }));
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
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

            try
            {
                var di = new DirectoryInfo(clipath);
                tagname = di.Name;
            }
            catch (Exception)
            {
            }

            var tw = new TagWindow(this, clipath, tagname);
            if (_firstopen == true)
            {
                MinimizeWindow();
                _firstopen = false;
            }
        }

        public void CliUntag(string clipath)
        {
            var tw = new UntagWindow(this, clipath);
            if (_firstopen == true)
            {
                MinimizeWindow();
                _firstopen = false;
            }
        }

        public void CliClean(string clipath)
        {
            //TODO 
            int count = ServiceLocator.GUI.Clean(clipath);
            MessageBox.Show(count + " metaclear-ed");
        }

        #endregion

        #region TagTitle Functionality: Renaming

        private bool RenameTag(String oldtagname, String newtagname)
        {
            /*
            if (Gui.RenameTag(oldtagname, newtagname))
            {
                InitializeTagList();
                SelectTag(newtagname);
                return true;
            }
            else
            {
                string messageBoxText = "Tag could not be renamed. There might be another tag with the same name.";
                string caption = "Rename Tag Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);

                return false;
            }
            */
            return true;
        }

        private void TagTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SelectedTag == TagTitle.Text) return;
            if (!RenameTag(SelectedTag, TagTitle.Text)) TagTitle.Text = SelectedTag;
        }

        private void TagTitleOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TagTitle.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        #endregion

        #region Keyboard Shortcuts

        private void InitializeKeyboardShortcuts()
        {
            // Create Tag Command
            var CreateTagCommand = new RoutedCommand();

            var cb = new CommandBinding(CreateTagCommand, CreateTagCommandExecute, CreateTagCommandCanExecute);
            CommandBindings.Add(cb);

            BtnCreate.Command = CreateTagCommand;

            var kg = new KeyGesture(Key.A, ModifierKeys.Control);
            var ib = new InputBinding(CreateTagCommand, kg);
            InputBindings.Add(ib);

            // Remove Tag Command
            var RemoveTagCommand = new RoutedCommand();

            var cb1 = new CommandBinding(RemoveTagCommand, RemoveTagCommandExecute, RemoveTagCommandCanExecute);
            CommandBindings.Add(cb1);

            btnRemove.Command = RemoveTagCommand;

            var kg1 = new KeyGesture(Key.R, ModifierKeys.Control);
            var ib1 = new InputBinding(RemoveTagCommand, kg1);
            InputBindings.Add(ib1);

            // Tag Command
            var TagCommand = new RoutedCommand();

            var cb2 = new CommandBinding(TagCommand, TagCommandExecute, TagCommandCanExecute);
            CommandBindings.Add(cb2);

            btnTag.Command = TagCommand;

            var kg2 = new KeyGesture(Key.T, ModifierKeys.Control);
            var ib2 = new InputBinding(TagCommand, kg2);
            InputBindings.Add(ib2);

            // Untag Command
            var UntagCommand = new RoutedCommand();

            var cb3 = new CommandBinding(UntagCommand, UntagCommandExecute, UntagCommandCanExecute);
            CommandBindings.Add(cb3);

            btnUntag.Command = UntagCommand;
            UntagRightClick.Command = UntagCommand;

            var kg3 = new KeyGesture(Key.U, ModifierKeys.Control);
            var ib3 = new InputBinding(UntagCommand, kg3);
            InputBindings.Add(ib3);

            // Details Command
            var DetailsCommand = new RoutedCommand();

            var cb4 = new CommandBinding(DetailsCommand, DetailsCommandExecute, DetailsCommandCanExecute);
            CommandBindings.Add(cb4);

            BtnDetails.Command = DetailsCommand;

            var kg4 = new KeyGesture(Key.D, ModifierKeys.Control);
            var ib4 = new InputBinding(DetailsCommand, kg4);
            InputBindings.Add(ib4);

            // Eject Command
            var EjectCommand = new RoutedCommand();

            var cb5 = new CommandBinding(EjectCommand, EjectCommandExecute, EjectCommandCanExecute);
            CommandBindings.Add(cb5);

            BtnEject.Command = EjectCommand;

            var kg5 = new KeyGesture(Key.E, ModifierKeys.Control);
            var ib5 = new InputBinding(EjectCommand, kg5);
            InputBindings.Add(ib5);

            // Options Command
            var OptionsCommand = new RoutedCommand();

            var cb6 = new CommandBinding(OptionsCommand, OptionsCommandExecute, OptionsCommandCanExecute);
            CommandBindings.Add(cb6);

            var kg6 = new KeyGesture(Key.O, ModifierKeys.Control);
            var ib6 = new InputBinding(OptionsCommand, kg6);
            InputBindings.Add(ib6);

            // Minimize Command
            var MinimizeCommand = new RoutedCommand();

            var cb7 = new CommandBinding(MinimizeCommand, MinimizeCommandExecute, MinimizeCommandCanExecute);
            CommandBindings.Add(cb7);

            var kg7 = new KeyGesture(Key.M, ModifierKeys.Control);
            var ib7 = new InputBinding(MinimizeCommand, kg7);
            InputBindings.Add(ib7);

            // Shortcuts Command
            var ShortcutsCommand = new RoutedCommand();

            var cb8 = new CommandBinding(ShortcutsCommand, ShortcutsCommandExecute, ShortcutsCommandCanExecute);
            CommandBindings.Add(cb8);

            var kg8 = new KeyGesture(Key.S, ModifierKeys.Control);
            var ib8 = new InputBinding(ShortcutsCommand, kg8);
            InputBindings.Add(ib8);
        }

        private void CreateTagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            //Actual Code
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
            //Actual Code

            RemoveTag();
        }

        private void RemoveTag()
        {
            try
            {
                if (SelectedTag != null)
                {
                    if(!Gui.GetTag(SelectedTag).IsSyncing)
                    {
                        bool result = DialogsHelper.ShowWarning("Remove Tag",
                                                                "Are you sure you want to remove the tag '" + SelectedTag +
                                                                "'?");

                        if (result)
                        {
                            bool success = Gui.DeleteTag(SelectedTag);
                            if (success)
                            {
                                InitializeTagList();
                                InitializeTagInfoPanel();
                            }
                            else
                            {
                                DialogsHelper.ShowError("Remove Tag Error", "' " + SelectedTag + " ' could not be removed.");
                            }
                        }
                    } else
                    {
                        DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                                                "You cannot delete a tag while it is synchronizing.");
                    }
                }
                else
                {
                    DialogsHelper.ShowError("No Tag Selected", "Please select a tag.");
                }
            }
            catch (UnhandledException)
            {
                DisplayUnhandledExceptionMessage();
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
            //Actual Code
            var tw = new TagWindow(this, "", SelectedTag);
        }

        private void TagCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void UntagCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            //Actual Code
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
            //Actual Code
            ViewTagDetails();
        }

        private void DetailsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void EjectCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            //Actual Code
            var driveMenu = new ContextMenu();

            List<DriveInfo> removableDrives = GetAllRemovableDrives();
            if (removableDrives.Count == 0)
            {
                var driveMenuItem = new MenuItem();
                driveMenuItem.Header = "No Removable Drives Found";
                driveMenu.Items.Add(driveMenuItem);
            }
            else
            {
                foreach (DriveInfo di in removableDrives)
                {
                    var driveMenuItem = new MenuItem();
                    driveMenuItem.Header = di.Name;
                    driveMenuItem.Click += new RoutedEventHandler(driveMenuItem_Click);
                    driveMenu.Items.Add(driveMenuItem);
                }
            }

            driveMenu.PlacementTarget = this;
            driveMenu.IsOpen = true;
        }

        private void EjectCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OptionsCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            //Actual Code
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
            //Actual Code
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
            //Actual Code
            DisplayShortcutsWindow();
        }

        private void ShortcutsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void DisplayShortcutsWindow()
        {
            var sw = new ShortcutsWindow();
            sw.ShowDialog();
        }

        #endregion

        #region Application Settings

        private void SaveApplicationSettings()
        {
            Settings.Default.Save();
        }

        #endregion
    }
}