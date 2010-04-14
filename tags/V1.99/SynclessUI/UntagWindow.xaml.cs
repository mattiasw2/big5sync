using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Syncless.Core.Exceptions;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for UntagWindow.xaml
    /// </summary>
    public partial class UntagWindow : Window
    {
        private readonly MainWindow _main;
        private bool _notifyUser;
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete

        public UntagWindow(MainWindow main, string clipath, bool notifyUser)
        {
            try
            {
                InitializeComponent();

                _main = main;
                _notifyUser = notifyUser;
                Owner = _main;
                ShowInTaskbar = false;

                var tagListByFolder = new List<string>();
                DirectoryInfo di = null;
                try
                {
                    di = new DirectoryInfo(clipath);
                }
                catch
                {
                }

                if (di != null)
                {
                    tagListByFolder = _main.Gui.GetTags(di);
                }

                if (tagListByFolder != null && tagListByFolder.Count != 0)
                {
                    TxtBoxPath.Text = clipath;
                    taglist.ItemsSource = tagListByFolder;
                    if (tagListByFolder.Count == 1)
                    {
                        taglist.SelectedIndex = 0;
                        taglist.Focus();
                    }
                    ShowDialog();
                    Topmost = true;
                    Topmost = false;
                }
                else
                {
                    DialogHelper.ShowError(this, "No Tags Found",
                                           "The folder you were trying to untag had no tags on it.");

                    Close();
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        private string Path
        {
            get { return TxtBoxPath.Text; }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            try
            {
                string lasttagged = "";
                if (taglist.SelectedIndex == -1)
                {
                    DialogHelper.ShowError(this, "Tag not Selected",
                                           "Please select the particular tag to untag the folder from.");
                    BtnOk.IsEnabled = true;
                    return;
                }

                foreach (string t in taglist.SelectedItems)
                {
                    if (!_main.Gui.GetTag(t).IsLocked)
                    {
                        int result = _main.Gui.Untag(t, new DirectoryInfo(Path));
                        lasttagged = t;
                        if (result != 1)
                        {
                            DialogHelper.ShowError(this, "Untagging Error", t + " could not be untagged from " + Path);
                        }
                        else
                        {
                            if (_notifyUser)
                            {
                                _main.NotifyBalloon("Untagging Successful", Path + " has been untagged from " + t);
                                //success
                                _main.ResetTagSyncStatus(t);
                            }
                        }
                    }
                    else
                    {
                        DialogHelper.ShowError(this, t + " is Synchronizing",
                                               "You cannot untag a folder while the tag is synchronizing.");
                        BtnOk.IsEnabled = true;
                    }
                }
                _main.SelectTag(lasttagged);
                Close();
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            Close();
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