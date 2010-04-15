/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Syncless.Core.Exceptions;
using Syncless.Core.View;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for UntagWindow.xaml. This window is activated when the user untags a folder from the Shell
    /// </summary>
    public partial class UntagWindow : Window
    {
        private readonly MainWindow _main;
        private bool _notifyUser;
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete

        /// <summary>
        /// 
        /// </summary>
        /// <param name="main">Reference to Main Window</param>
        /// <param name="path">Path to untag from</param>
        /// <param name="notifyUser">If true, tray notification enabled. Else if false, do not notify.</param>
        public UntagWindow(MainWindow main, string path, bool notifyUser)
        {
            InitializeComponent();

            // Set up window properties
            _main = main;
            _notifyUser = notifyUser;
            Owner = _main;
            ShowInTaskbar = false;

            try
            {
                // Gets tag list for folder which was tagged
                List<string> tagListByFolder = null;

                // Check if directory selected is valid, and if it is, get the tag list of folders.
                DirectoryInfo di;
                try
                {
                    di = new DirectoryInfo(path);
                    tagListByFolder = _main.LogicLayer.GetTags(di);
                }
                catch {}

                // if there are tags on the folder
                if (tagListByFolder != null && tagListByFolder.Count != 0)
                {
                    TxtBoxPath.Text = path;
                    taglist.ItemsSource = tagListByFolder;
                    // Populate the list and selects the first tag on the list if there is only 1 tag involved.
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

        /// <summary>
        /// Path to untag
        /// </summary>
        private string Path
        {
            get { return TxtBoxPath.Text; }
        }

        /// <summary>
        /// Event handler for BtnOk_Click event. Trys to untag every tag that was selected and then closes the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            try
            {
                // gets the last untagged tag
                string lastuntagged = "";

                // if no tag selected for tagging
                if (taglist.SelectedIndex == -1)
                {
                    DialogHelper.ShowError(this, "Tag not Selected",
                                           "Please select the particular tag to untag the folder from.");
                    BtnOk.IsEnabled = true;
                    return;
                }

                // for every tag selected, untag them if it is not locked
                foreach (string t in taglist.SelectedItems)
                {
                    TagView currentTag = _main.LogicLayer.GetTag(t);
                    if (currentTag != null)
                    {
                        // if current tag is not locked
                        if(!currentTag.IsLocked)
                        {
                            int result = _main.LogicLayer.Untag(t, new DirectoryInfo(Path));
                            lastuntagged = t;
                            if (result != 1)
                            {
                                DialogHelper.ShowError(this, "Untagging Error", t + " could not be untagged from " + Path);
                            }
                            else
                            {
                                // if tray notification is enabled, inform user
                                if (_notifyUser)
                                {
                                    _main.NotifyBalloon("Untagging Successful", Path + " has been untagged from " + t);
                                }

                                _main.ResetTagSyncStatus(t);
                            }
                        }
                        else
                        {
                            DialogHelper.ShowError(this, t + " is Synchronizing",
                                                   "You cannot untag a folder while the tag is synchronizing.");
                        }
                    } else
                    {
                        DialogHelper.ShowError(this, "Tag not Found", "Please try again.");
                    }
                }
                
                // select the last tag that was untagged
                _main.SelectTag(lastuntagged);
                Close();
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
                Close();
            }
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