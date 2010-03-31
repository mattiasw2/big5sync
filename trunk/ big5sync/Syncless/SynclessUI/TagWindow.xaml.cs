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
        private const string DialogDescription = "Please select a folder to tag.";
        private readonly MainWindow _main;
        private bool _cancelstatus = false;
        private bool _notifyUser;
        private bool _closingAnimationNotCompleted = true;
        private FolderBrowserDialog _ofd = new FolderBrowserDialog();

        private string _path;
        private bool _popupclosed = true;
        private string _selectedTag;
        private VistaFolderBrowserDialog vistafolderDialog = new VistaFolderBrowserDialog();
        private bool _isTagNormally;
        private bool _isInvalidFolder;

        public TagWindow(MainWindow main, string path, string tagname, bool notifyUser)
        {
            InitializeComponent();

            _main = main;

            if(tagname != null)
            {
                int maxlength = tagname.Length > 20 ? 20 : tagname.Length;
                tagname = tagname.Substring(0, maxlength);
            }

            _selectedTag = tagname;
            _notifyUser = notifyUser;

            InitializeFolderDialogs();

            ACBName.IsEnabled = false;
            
            _isTagNormally = path == "" ? true : false;
            _isInvalidFolder = false;
            _path = path == "" ? SelectPath() : path;

            if (_cancelstatus)
            {
                Close();
            }
            else
            {
                ProcessPath(_path, _selectedTag);
                if (!_isTagNormally && _isInvalidFolder)
                {
                    Close();
                }
                else
                {
                    ShowDialog();
                }
            }
        }

        private string Tagname
        {
            get
            {
                if (ACBName.Text != null) return ACBName.Text.Trim();
                else return "";
            }
        }

        private void InitializeFolderDialogs()
        {
            vistafolderDialog.Description = DialogDescription;
            vistafolderDialog.UseDescriptionForTitle = true;
                // This applies to the Vista style dialog only, not the old dialog.
            vistafolderDialog.ShowNewFolderButton = true;
            _ofd.Description = DialogDescription;
            _ofd.ShowNewFolderButton = true;
        }

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
                    _cancelstatus = true;
                }
            }
            else
            {
                DialogResult result = _ofd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = _ofd.SelectedPath;
                }
                else
                {
                    _cancelstatus = true;
                }
            }

            return path;
        }

        private void ProcessPath(string path, string selectedTag)
        {
            try
            {
                if (path != "")
                {
                    var di = new DirectoryInfo(path);
                    if (di.Exists && !FileHelper.IsFile(path))
                    {
                        if (FileHelper.IsCDRomDrive(path))
                        {
                            DialogHelper.ShowError("Invalid Folder", "You cannot tag any folder from a CD/DVD-Rom drive.");
                            if (!_isTagNormally)
                            {
                                _isInvalidFolder = true;
                            }
                        }
                        else if (FileHelper.IsSynclessFolder(path))
                        {
                            DialogHelper.ShowError("Invalid Folder", "You cannot tag this folder.");
                            if (!_isTagNormally)
                            {
                                _isInvalidFolder = true;
                            }
                        }
                        else
                        {
                            TxtBoxPath.Text = path;
                            ACBName.IsEnabled = true;
                            ACBName.ItemsSource = _main.Gui.GetAllTags();
                            if (selectedTag == null)
                            {
                                ACBName.Text = di.Name;
                            }
                            else
                            {
                                ACBName.Text = selectedTag;
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
                DialogHelper.DisplayUnhandledExceptionMessage();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            try
            {
                if (Tagname != "")
                {
                    if (_path != "" && !FileHelper.IsFile(_path))
                    {
                        bool tocontinue1 = TriggerLongPathWarning();
                        bool tocontinue2 = TriggerDriveWarning();

                        if (tocontinue1 && tocontinue2)
                        {
                            _main.CreateTag(Tagname);

                            TagView tv1 = null;

                            try
                            {
                                if (!_main.Gui.GetTag(Tagname).IsLocked)
                                {
                                    tv1 = _main.Gui.Tag(Tagname, new DirectoryInfo(_path));

                                    if (tv1 != null)
                                    {
                                        _main.InitializeTagList();
                                        _main.SelectTag(Tagname);
                                        if (_notifyUser)
                                            _main.NotifyBalloon("Folder Tagged",
                                                                _path + " has been tagged to " + Tagname);
                                        Close();
                                    }
                                    else
                                    {
                                        DialogHelper.ShowError("Tag Error", "Tag Error Occured. Please Try Again.");
                                        BtnOk.IsEnabled = true;
                                    }
                                }
                                else
                                {
                                    DialogHelper.ShowError(Tagname + " is Synchronizing",
                                                           "You cannot tag a folder while the tag is synchronizing.");
                                    BtnOk.IsEnabled = true;
                                }
                            }
                            catch (RecursiveDirectoryException)
                            {
                                DialogHelper.ShowError("Folder cannot be tagged",
                                                       "Folder could not be tagged as it is a sub-folder/parent/ancestor of a folder which is already tagged.");
                                BtnOk.IsEnabled = true;
                            }
                            catch (PathAlreadyExistsException)
                            {
                                DialogHelper.ShowError("Path Already Exists",
                                                       "The path you tried to tag is already tagged.");
                                BtnOk.IsEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        DialogHelper.ShowError("Folder Not Selected", "Please select a folder to tag.");
                        BtnOk.IsEnabled = true;
                    }
                }
                else
                {
                    DialogHelper.ShowError("Tagname Empty", "Please specify a tagname.");
                    BtnOk.IsEnabled = true;
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage();
                Close();
            }
        }

        private bool TriggerDriveWarning()
        {
            var di = new DriveInfo(_path);
            if (di.Name == _path)
            {
                bool result = DialogHelper.ShowWarning("Tag Drive Warning",
                                                       "You are about to tag " + _path +
                                                       "\nAre you sure you wish to continue?");

                return result;
            }

            return true;
        }

        private bool TriggerLongPathWarning()
        {
            if (_path.Length > 200)
            {
                bool result = DialogHelper.ShowWarning("Long Path Name Warning",
                                                       "NTFS File System does not handle paths which are 248 characters or more in length properly. \nAre you sure you wish to continue?");

                return result;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(ACBName);
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            _selectedTag = ACBName.Text;
            string path = SelectPath();
            ProcessPath(path, _selectedTag);
        }

        /// <summary>
        ///	Ugly Hack for Overwritting the ACB Enter Behavior because it does not accept the Enter Key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _popupclosed == true)
            {
                BtnOk.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            _popupclosed = false;
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            _popupclosed = true;
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