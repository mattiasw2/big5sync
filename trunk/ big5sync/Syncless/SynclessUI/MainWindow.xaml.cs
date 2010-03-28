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
using System.Media;

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
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
            string message = "Synchronization Completed at " + DateTime.Now;

            if (SelectedTag == tagname)
            {
                LblStatusText.Content = message;
                BtnSyncNow.IsEnabled = true;
                BtnPreview.IsEnabled = true;
                BtnSyncMode.IsEnabled = true;
                _manualSyncEnabled = true;
            }

            _tagStatusNotificationDictionary[tagname] = message;
            NotifyBalloon("Synchronization Completed", tagname + " is now synchronized.");
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
            return _syncProgressNotificationDictionary[tagname];
        }

        public string GetTagStatus(string tagname)
        {
            try {
                string status = _tagStatusNotificationDictionary[tagname];

                return status;
            } catch
            {
                return "";
            }
        }

        /*
         * notification.StartsWith("Synchronization Completed")
                    || notification == "Please Wait"
                    || notification == "Nothing to Synchronize"
                    || notification == "Analyzing Folders"
                    || notification == "Synchronization request has been queued"
         */

        public void SetSyncProgress(string tagname, SyncProgress progress)
        {
            string notification = _tagStatusNotificationDictionary[tagname];

            if (SelectedTag == tagname)
            {
                ProgressBarSync.SetValue(ProgressBar.ValueProperty, progress.PercentComplete);

                if (!(notification != null
                    && (notification.StartsWith("Synchronization Completed")
                    || notification == "Please Wait"
                    || notification == "Nothing to Synchronize"
                    || notification == "Analyzing Folders"
                    || notification == "Synchronization request has been queued"
                    || notification.StartsWith("Starting"))
                    && progress.Message == "Finalizing"))
                {
                    LblStatusText.Content = progress.Message;
                }

               SetProgressBarColor(progress.PercentComplete);
            }

            _syncProgressNotificationDictionary[tagname] = progress.PercentComplete;

            if (!(notification != null
                && notification.StartsWith("Synchronization Completed")
                && progress.Message == "Finalizing"))
            {
                _tagStatusNotificationDictionary[tagname] = progress.Message;
            }
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

        public void NotifyBalloon(string title, string text)
        {
            SystemSounds.Exclamation.Play();
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

                switch (tv.TagState)
                {
                    case TagState.Switching:
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
            if(Properties.Settings.Default.MinimizeToTray)
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
                DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                             "You cannot change the synchronization mode while it is synchronizing.");
                return;
            }

            try
            {
                if (!Gui.GetTag(SelectedTag).IsLocked  && !(GetTagStatus(SelectedTag) == "Finalizing"))
                {
                    if (string.Compare((string) LblSyncMode.Content, "Manual") == 0)
                    {
                        if (Gui.MonitorTag(SelectedTag, true))
                        {
                            ProgressBarSync.Value = 0;
                            const string message = "Please Wait";
                            LblStatusText.Content = message;
                            _tagStatusNotificationDictionary[SelectedTag] = message;
                            SwitchingMode();
                            Console.WriteLine("User -> Switching");
                        }
                        else
                        {
                            DialogsHelper.ShowError("Change Synchronization Mode Error",
                                                    "' " + SelectedTag + " ' could not be switched to Seamless Mode.");
                        }
                    }
                    else if (string.Compare((string)LblSyncMode.Content, "Seamless") == 0)
                    {
                        if (Gui.MonitorTag(SelectedTag, false))
                        {
                            ManualMode();
                        }
                        else
                        {
                            DialogsHelper.ShowError("Change Synchronization Mode Error",
                                                    "' " + SelectedTag + " ' could not be switched to Manual Mode.");
                        }
                    }
                } else
                {
                    DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                                            "You cannot change the synchronization mode while it is synchronizing.");
                }
            }
            catch (UnhandledException)
            {
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
            Console.WriteLine("In Seamless Mode");
        }

        private void SwitchingMode()
        {
            LblSyncMode.Content = "Switching";
            BtnSyncMode.ToolTip = "Please Wait";
            BtnPreview.Visibility = Visibility.Hidden;
            BtnSyncNow.Visibility = Visibility.Hidden;
            BtnSyncMode.SetResourceReference(BackgroundProperty, "ToggleOffBrush");
            LblSyncMode.SetResourceReference(MarginProperty, "ToggleOffMargin");
            LblSyncMode.SetResourceReference(ForegroundProperty, "ToggleOffForeground");
            ProgressBarSync.Visibility = Visibility.Visible;
            LblProgress.Visibility = Visibility.Visible;
            Console.WriteLine("In Switching Mode");
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
            Console.WriteLine("In Manual Mode");
        }

        private void BtnSyncNow_Click(object sender, RoutedEventArgs e)
        {
            if (!_manualSyncEnabled || Gui.GetTag(SelectedTag).IsLocked || GetTagStatus(SelectedTag) == "Finalizing")
            {
                DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                             "You cannot carry out another synchronization operation while it is synchronizing.");
                return;
            }

            try
            {
                if (Gui.GetTag(SelectedTag).PathStringList.Count > 1)
                {
                    if (Gui.StartManualSync(SelectedTag))
                    {
                        const string message = "Synchronization request has been queued";
                        LblStatusText.Content = message;
                        _tagStatusNotificationDictionary[SelectedTag] = message;
                        ProgressBarSync.Value = 0;
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
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
                    if (!Gui.GetTag(SelectedTag).IsLocked)
                    {
                        if (ListTaggedPath.SelectedIndex == -1)
                        {
                            DialogsHelper.ShowError("No Path Selected", "Please select a path to untag.");
                        }
                        else
                        {
                            TagView tv = Gui.GetTag((string)TagTitle.Text);

                            if (tv != null)
                            {
                                if (!tv.IsLocked)
                                {
                                    Gui.Untag(tv.TagName, new DirectoryInfo((string)ListTaggedPath.SelectedValue));

                                    SelectTag(tv.TagName);
                                }
                                else
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
                    else
                    {
                        DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                                                "You cannot untag a folder while the tag is synchronizing.");
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogsHelper.DisplayUnhandledExceptionMessage();
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

                            // Disposes the TaskBarIcon
                            TaskbarIcon.Dispose();
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
            }
        }

        private void RemoveTagRightClick_Click(object sender, RoutedEventArgs e)
        {
            RemoveTag();
        }
		
        private void DetailsRightClick_Click(object sender, RoutedEventArgs e)
        {
            ViewTagDetails();
        }

        private void TagRightClick_Click(object sender, RoutedEventArgs e)
        {
            DisplayTagWindow();
        }

        private void ViewTagDetails()
        {
            if(SelectedTag != null)
            {
                if (!Gui.GetTag(SelectedTag).IsLocked)
                {
                    var tdw = new TagDetailsWindow(SelectedTag, this);
                    tdw.ShowDialog();
                }
                else
                {
                    DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                                            "You cannot view tag details while the tag is synchronizing.");
                }
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
            if (!_manualSyncEnabled || Gui.GetTag(SelectedTag).IsLocked || GetTagStatus(SelectedTag) == "Finalizing")
            {
                DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                                            "You cannot preview while it is synchronizing.");
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
                    DialogsHelper.ShowError("Unmonitor Error",
                                            "Syncless could not unmonitor " + driveletter + " from seamless mode.");
                }
                else
                {
                    DialogsHelper.ShowInformation("Monitoring Stopped for " + driveletter,
                                                  "Syncless has stopped all seamless monitoring for " + driveletter + 
													"\nTagging any folders in this drive will re-activate seamless monitoring.");
                }
            }
            catch (UnhandledException)
            {
                DialogsHelper.DisplayUnhandledExceptionMessage();
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

        private void LayoutRoot_Drop(object sender, DragEventArgs e)
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
                        if(shortcutfolderpath != null)
                        {
                            path = shortcutfolderpath;
                        }

                        // to detect folders
                        try {
                            var folder = new DirectoryInfo(path);
                            if (folder.Exists && !FileHelper.IsFile(path))
                            {
                                var tw = new TagWindow(this, path, SelectedTag, false);
                            }
                        } catch
                        {
                        }
                    }
            }
            TxtBoxFilterTag.IsHitTestVisible = true;
        }

        private void LayoutRoot_DragEnter(object sender, DragEventArgs e)
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
            RepopulateTagList_ThreadSafe();
        }

        public void TagChanged()
        {
            RepopulateTagList_ThreadSafe();
        }

        /*
        private void RepopulateTagList()
        {
            try
            {
                string current = SelectedTag;

                InitializeTagList();

                SelectTag(current);
            } catch
            {
            }
        }
        */

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
                                                                        switch (tv.TagState)
                                                                        {
                                                                            case TagState.Switching:
                                                                                Console.WriteLine("Tag Changed: Switching");
                                                                                SwitchingMode();
                                                                                break;
                                                                            case TagState.Seamless:
                                                                                Console.WriteLine("Tag Changed: Seamless");
                                                                                SeamlessMode();
                                                                                break;
                                                                            case TagState.Manual:
                                                                                _manualSyncEnabled = !tv.IsLocked;
                                                                                Console.WriteLine("Tag Changed: Manual");
                                                                                ManualMode();
                                                                                break;
                                                                        }
                                                                        ListTaggedPath.ItemsSource = tv.PathStringList;
                                                                        BdrTaggedPath.Visibility = tv.PathStringList.Count == 0 ? Visibility.Hidden : Visibility.Visible;
                                                                    }));
            }
            catch (UnhandledException)
            {
                DialogsHelper.DisplayUnhandledExceptionMessage();
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

            if(FileHelper.IsFile(clipath))
            {
                DialogsHelper.ShowError("Tagging not Allowed", "You cannot tag a file.");
                return;
            }

            try
            {
                var di = new DirectoryInfo(clipath);
                tagname = di.Name;
            }
            catch (Exception)
            {
                DialogsHelper.ShowError("Invalid Folder", "You cannot tag this folder.");
                return;
            }

            var tw = new TagWindow(this, clipath, tagname, true);
        }

        public void CliUntag(string clipath)
        {
            System.Windows.WindowState currentWindowState = this.WindowState;
            if (FileHelper.IsFile(clipath))
            {
                DialogsHelper.ShowError("Untagging not Allowed", "You cannot tag a file.");
                return;
            }

            DirectoryInfo di = null;

            try
            {
                di = new DirectoryInfo(clipath);
            }
            catch (System.ArgumentException)
            {
                DialogsHelper.ShowError("Invalid Folder", "You cannot untag this folder.");
                return;
            }

            var tw = new UntagWindow(this, clipath, true);
        }

        public void CliClean(string clipath)
        {
            if (FileHelper.IsFile(clipath))
            {
                DialogsHelper.ShowError("Cleaning not Allowed", "You cannot clean a file.");
                return;
            }

            DirectoryInfo di = null;

            try
            {
                di = new DirectoryInfo(clipath);
            } catch(System.ArgumentException)
            {
                DialogsHelper.ShowError("Invalid Folder", "You cannot clean this folder.");
                return;
            }

            //TODO 
            int count = ServiceLocator.GUI.Clean(clipath);
            DialogsHelper.ShowInformation("Meta-data Cleaned", count + " metaclear-ed");
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

            var kg = new KeyGesture(Key.N, ModifierKeys.Control);
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

            // Unmonitor Command
            var UnmonitorCommand = new RoutedCommand();

            var cb5 = new CommandBinding(UnmonitorCommand, UnmonitorCommandExecute, UnmonitorCommandCanExecute);
            CommandBindings.Add(cb5);

            BtnUnmonitor.Command = UnmonitorCommand;

            var kg5 = new KeyGesture(Key.I, ModifierKeys.Control);
            var ib5 = new InputBinding(UnmonitorCommand, kg5);
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
                    if(!Gui.GetTag(SelectedTag).IsLocked)
                    {
                        bool result = DialogsHelper.ShowWarning("Remove Tag",
                                                                "Are you sure you want to remove the tag '" + SelectedTag +
                                                                "'?");

                        if (result)
                        {
                            if(!Gui.GetTag(SelectedTag).IsLocked)
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
                            else
                            {
                                DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                                                        "You cannot remove a tag while the tag is synchronizing.");
                            }
                        }
                    } else
                    {
                        DialogsHelper.ShowError(SelectedTag + " is Synchronizing",
                                                "You cannot remove a tag while the tag is synchronizing.");
                    }
                }
                else
                {
                    DialogsHelper.ShowError("No Tag Selected", "Please select a tag.");
                }
            }
            catch (UnhandledException)
            {
                DialogsHelper.DisplayUnhandledExceptionMessage();
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

        private void UnmonitorCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            //Actual Code
            DisplayUnmonitorContextMenu();
        }

        private void DisplayUnmonitorContextMenu()
        {
            var driveMenu = new ContextMenu();

            List<string> removableDrives = Syncless.Helper.DriveHelper.GetUSBDriveLetters();
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
                    driveMenuItem.Click += new RoutedEventHandler(driveMenuItem_Click);
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

        private void ListBoxTag_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete && ListBoxTag.Items.Count > 0)
            {
				RemoveTag();
			}
        }

        private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        #endregion
    }
}