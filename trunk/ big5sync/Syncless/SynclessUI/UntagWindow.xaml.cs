using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
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
        private string Path
        {
            get { return TxtBoxPath.Text; }
        }
        
		public UntagWindow(MainWindow main, string clipath, bool notifyUser)
        {
            try
            {
                InitializeComponent();

                _main = main;
                _notifyUser = notifyUser;

                List<string> tagListByFolder = new List<string>();
                DirectoryInfo di = null;
                try
                {
                    di = new DirectoryInfo(clipath);
                } catch { }

                if(di != null)
                {
                    tagListByFolder = _main.Gui.GetTags(di);
                }

                if (tagListByFolder != null && tagListByFolder.Count != 0)
                {
                    TxtBoxPath.Text = clipath;
                    taglist.ItemsSource = tagListByFolder;
                    if(tagListByFolder.Count == 1)
                    {
                        taglist.SelectedIndex = 0;
                        taglist.Focus();
                    }
                    this.ShowDialog();
                }
                else
                {
                    DialogsHelper.ShowError("No Tags Found", "The folder you were trying to untag had no tags on it.");

                    CloseWindow();
                }
            }
            catch (UnhandledException)
            {
                DialogsHelper.DisplayUnhandledExceptionMessage();
            }
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			BtnOk.IsEnabled = false;
            try
            {
                string lasttagged = "";
                if (taglist.SelectedIndex == -1)
                {
                    DialogsHelper.ShowError("Tag not Selected", "Please select the particular tag to untag the folder from.");
					BtnOk.IsEnabled = true;
                    return;
                }
                
                foreach (string t in taglist.SelectedItems)
                {
                    if(!_main.Gui.GetTag(t).IsLocked) {
                        int result = _main.Gui.Untag(t, new DirectoryInfo(Path));
                        lasttagged = t;
                        if (result != 1)
                        {
                            DialogsHelper.ShowError("Untagging Error", t + " could not be untagged from " + Path);
                        } else
                        {
                            if(_notifyUser)
                                _main.NotifyBalloon("Untagging Successful", Path + " has been untagged from " + t);
                        }
                    }
                    else
                    {
                        DialogsHelper.ShowError(t + " is Synchronizing",
                                                "You cannot untag a folder while the tag is synchronizing.");
						BtnOk.IsEnabled = true;
                    }
                }
                _main.SelectTag(lasttagged);
                CloseWindow();
            }
            catch (UnhandledException)
            {
                DialogsHelper.DisplayUnhandledExceptionMessage();
				CloseWindow();
            }
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseWindow();
        }

		private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.DragMove();
		}
		
		private void CloseWindow() {
            FormFadeOut.Begin();
		}
		
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
