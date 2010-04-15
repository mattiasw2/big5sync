/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Syncless.Core.Exceptions;
using Syncless.Core.View;
using Syncless.Tagging.Exceptions;
using SynclessUI.Helper;
using Button=System.Windows.Controls.Button;
using KeyEventArgs=System.Windows.Input.KeyEventArgs;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class TagWindow : Window
    {
        private readonly MainWindow _main;
        private bool _cancelstatus = false; // if user wishes to cancel the browser dialog
        private bool _notifyUser; // tray notification
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete

        private string _path; // initial path input into the tag window by shell/dragdrop
        private bool _popupclosed = true; // Keeps track whether the popup is open/closed
        private string _selectedTag; // Tag which is selected to be tagged
        private FolderBrowserDialog _oldFolderDialog = new FolderBrowserDialog();
        private VistaFolderBrowserDialog vistafolderDialog = new VistaFolderBrowserDialog();
        private bool _isTaggedNormally;
        private bool _isInvalidFolder; // Check if folder browsed is an invalid folder

        /// <summary>
        /// Initialize the Tag Window
        /// </summary>
        /// <param name="main">Reference to MainWindow</param>
        /// <param name="path">Path to initially use if user tags from the shell or drag a folder into Syncless</param>
        /// <param name="tagname">A pre-specified tagname to use</param>
        /// <param name="notifyUser">Whether or not to notify the user through tray notification</param>
        public TagWindow(MainWindow main, string path, string tagname, bool notifyUser)
        {
            InitializeComponent();

            // Set up Tag Window Properties
            _main = main;
            Owner = _main;
            ShowInTaskbar = false;

            // Trim the length of the Tag to 20 characters at most.
            if (tagname != null)
            {
                int maxlength = tagname.Length > 20 ? 20 : tagname.Length;
                tagname = tagname.Substring(0, maxlength);
            }

            _selectedTag = tagname;
            _notifyUser = notifyUser;

            // Initialize the two different folder dialogs used
            InitializeFolderDialogs();

            // Disable the AutoCompleteBox at the startup
            ACBName.IsEnabled = false;
            
            // if there is already a specified, the tagging operation has not been done through the Tag button on MainWindow
            _isTaggedNormally = path == "" ? true : false;
            _isInvalidFolder = false;

            // if no specified path onstartup, choose a path, if not use the specified paths
            _path = path == "" ? SelectPath() : path;

            // if user cancels the tagging operation, close the window
            if (_cancelstatus)
            {
                Close();
            }
            else
            {
                // Processes the path to see if the autocomplete box can be enabled
                ProcessPath(_path, _selectedTag);
                //if the path is an invalid folder and not tagged normally, it must have come from shell/drag and drop,
                // thus close the screen
                if (!_isTaggedNormally && _isInvalidFolder)
                {
                    Close();
                }
                else
                {
                    try
                    {
                        ShowDialog();
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Easy to use way to get/set the AutoCompleteBox; by using it the property to get set it
        /// </summary>
        private string TagToUse
        {
            get
            {
                if (ACBName.Text != null) return ACBName.Text.Trim();
                return "";
            }
            set
            {
                ACBName.Text = value;
            }
        }

        /// <summary>
        /// Initialize all folder dialogs, both the Vista/Old Folder Dialog
        /// </summary>
        private void InitializeFolderDialogs()
        {
            string DialogDescription = "Please select a folder to tag.";
            vistafolderDialog.Description = DialogDescription;
            vistafolderDialog.UseDescriptionForTitle = true;
            // This applies to the Vista style dialog only, not the old dialog.
            vistafolderDialog.ShowNewFolderButton = true;
            _oldFolderDialog.Description = DialogDescription;
            _oldFolderDialog.ShowNewFolderButton = true;
        }

        /// <summary>
        /// Check if the new vista folder browser dialog is support by vista and above, if not displays the old folder browser dialog
        /// </summary>
        /// <returns>Path Choosen</returns>
        private string SelectPath()
        {
            string path = "";

            if (VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            {
                if ((bool) vistafolderDialog.ShowDialog())
                {
                    path = vistafolderDialog.SelectedPath;
                }
                else
                {
                    // Cancelled the browsing process
                    _cancelstatus = true;
                }
            }
            else
            {
                DialogResult result = _oldFolderDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = _oldFolderDialog.SelectedPath;
                }
                else
                {
                    // Cancelled the browsing process
                    _cancelstatus = true;
                }
            }

            return path;
        }

        /// <summary>
        /// Takes the path and the selected tag, Checks if the path is a valid path (not zip, cdrom, synclessfolder)
        /// and if it is, check if the tag exists. If all is ready, enable the autocompletebox, if not disable it
        /// </summary>
        /// <param name="path">Path Selected</param>
        /// <param name="selectedTag">Tag Selected</param>
        private void ProcessPath(string path, string selectedTag)
        {
            try
            {
                if (path != "")
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    if (di.Exists && !FileFolderHelper.IsFile(path)) // detect zip files and such files
                    {
                        if (FileFolderHelper.IsCDRomDrive(path))
                        {
                            DialogHelper.ShowError(this, "Invalid Folder",
                                                   "You cannot tag any folder from a CD/DVD-Rom drive.");
                            if (!_isTaggedNormally)
                            {
                                _isInvalidFolder = true;
                            }
                        }
                        else if (FileFolderHelper.IsSynclessFolder(path))
                        {
                            DialogHelper.ShowError(this, "Invalid Folder", "You cannot tag this folder.");
                            if (!_isTaggedNormally)
                            {
                                _isInvalidFolder = true;
                            }
                        }
                        else
                        {
                            TxtBoxPath.Text = path;
                            ACBName.IsEnabled = true;
                            ACBName.ItemsSource = _main.LogicLayer.GetAllTags(); // get all tags pre-defined in the system
                            if (selectedTag == null) // if no pre-defined tag
                            {
                                TagToUse = di.Name; // use directory name
                            }
                            else
                            {
                                TagToUse = selectedTag; // use the pre-defined selection
                            }
                        }
                    }
                    else
                    {
                        ACBName.IsEnabled = false;
                        ACBName.ItemsSource = new List<string>();
                    }
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        /// <summary>
        /// Button to initiate the Tagging Process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            try
            {
                if (TagToUse != "")
                {
                    // Make sure path has substance and is not a file
                    if (_path != "" && !FileFolderHelper.IsFile(_path))
                    {
                        // trigger the warning choices
                        bool tocontinue1 = TriggerLongPathWarning();
                        bool tocontinue2 = TriggerDriveWarning();

                        // if user happens to encounter both warnings and click yes/true, proceed
                        if (tocontinue1 && tocontinue2)
                        {
                            try
                            {
                                DirectoryInfo di = new DirectoryInfo(_path);
                                // Check once again if the folder exists and that the path selected is not a File
                                if (di.Exists && !FileFolderHelper.IsFile(_path))
                                {
                                    _main.CreateTag(TagToUse);

                                    TagView tv = _main.LogicLayer.GetTag(TagToUse);

                                    if(tv != null)
                                    {
                                        // if Tag is not synchronizing
                                        if (!tv.IsLocked)
                                        {
                                            tv = _main.LogicLayer.Tag(TagToUse, new DirectoryInfo(_path));

                                            // if tagview not empty, tag it
                                            if (tv != null)
                                            {
                                                _main.InitializeTagList();
                                                _main.SelectTag(TagToUse);
                                                if (_notifyUser)
                                                    _main.NotifyBalloon("Folder Tagged",
                                                                        _path + " has been tagged to " + TagToUse);
                                                Close();
                                            }
                                            else
                                            {
                                                DialogHelper.ShowError(this, "Tag Not Found",
                                                                       "Please Try Again.");
                                                Close();
                                            }
                                        }
                                        else
                                        {
                                            DialogHelper.ShowError(this, TagToUse + " is Synchronizing",
                                                                   "You cannot tag a folder while the tag is synchronizing.");
                                            BtnOk.IsEnabled = true;
                                        }
                                    } else
                                    {
                                        DialogHelper.ShowError(this, "Tag Error",
                                                               "Tag Error Occured. Please Try Again.");
                                        Close();
                                    }
                                }
                                else
                                {
                                    DialogHelper.ShowError(this, "Invalid/Missing Folder",
                                                           "You cannot tag the specified folder");
                                    BtnOk.IsEnabled = true;
                                }
                            }
                            catch (RecursiveDirectoryException)
                            {
                                DialogHelper.ShowError(this, "Folder cannot be tagged",
                                                       "Folder could not be tagged as it is a sub-folder/parent/ancestor of a folder which is already tagged.");
                                BtnOk.IsEnabled = true;
                            }
                            catch (PathAlreadyExistsException)
                            {
                                DialogHelper.ShowError(this, "Path Already Exists",
                                                       "The path you tried to tag is already tagged.");
                                BtnOk.IsEnabled = true;
                            }
                        }
                        else
                        {
                            BtnOk.IsEnabled = true;
                        }
                    }
                    else
                    {
                        DialogHelper.ShowError(this, "Folder Not Selected", "Please select a folder to tag.");
                        BtnOk.IsEnabled = true;
                    }
                }
                else
                {
                    DialogHelper.ShowError(this, "TagToUse Empty", "Please specify a tagname.");
                    BtnOk.IsEnabled = true;
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
                Close();
            }
        }

        /// <summary>
        /// Display a dialog warning choice window when user chooses a drive itself to see if user wants to proceed.
        /// </summary>
        /// <returns></returns>
        private bool TriggerDriveWarning()
        {
            DriveInfo di = new DriveInfo(_path);
            if (di.Name == _path)
            {
                bool result = DialogHelper.ShowWarning(this, "Tag Drive Warning",
                                                       "You are about to tag " + _path +
                                                       "\nAre you sure you wish to continue?");

                return result;
            }

            return true;
        }

        /// <summary>
        /// Display a dialog warning choice window for long path names to see if user wants to proceed.
        /// </summary>
        /// <returns></returns>
        private bool TriggerLongPathWarning()
        {
            if (_path.Length > 200)
            {
                bool result = DialogHelper.ShowWarning(this, "Long Path Name Warning",
                                                       "NTFS File System does not handle paths which are 248 characters or more in length properly. \nAre you sure you wish to continue?");

                return result;
            }

            return true;
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
        /// On Windows Load, Focus on AutocompleteBox so that user can type tagname
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(ACBName);
        }

        /// <summary>
        /// Initiates the SelectPath Process, by taking in a new path, then takes that with the tagname
        /// and Processes the Path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            _selectedTag = TagToUse;
            _path = SelectPath();
            ProcessPath(_path, _selectedTag);
        }

        /// <summary>
        ///	Ugly Hack for Overwritting the AutoCompleteBox Enter Behavior because it does not accept the Enter Key
        /// If the enter key is entered and the popup is closed, it would imply that the user wishes to proceed
        /// to finalizing tagging by simulating a click on the BtnOk button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _popupclosed)
            {
                BtnOk.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        /// <summary>
        /// Sets the _popupclosed to false everytime the popup is opened. This is to assist Text_PreviewKeyDown
        /// to know if the popup menu is opened/closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_Opened(object sender, EventArgs e)
        {
            _popupclosed = false;
        }

        /// <summary>
        /// Sets the _popupclosed to true everytime the popup is closed. This is to assist Text_PreviewKeyDown
        /// to know if the popup menu is opened/closed..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_Closed(object sender, EventArgs e)
        {
            _popupclosed = true;
        }

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
            if (_closingAnimationNotCompleted)
            {
                BtnCancel.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }
    }
}