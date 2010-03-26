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
        
		public UntagWindow(MainWindow main, string clipath)
        {
            try
            {
                InitializeComponent();

                _main = main;

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
                
                if (tagListByFolder.Count != 0)
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

                    this.Close();
                }
            }
            catch (UnhandledException)
            {
                _main.DisplayUnhandledExceptionMessage();
            }
        }
		
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (taglist.SelectedIndex == -1)
                {
                    DialogsHelper.ShowError("Tag not Selected", "Please select the particular tag to untag the folder from.");
                    return;
                }

                foreach (string t in taglist.SelectedItems)
                {
                    int result = _main.Gui.Untag(t, new DirectoryInfo(TxtBoxPath.Text));
                    if (result != 1)
                    {
                        DialogsHelper.ShowError("Untagging Error", t + " could not be untagged from " + TxtBoxPath.Text);
                    }
                }
                _main.InitializeTagList();
                this.Close();
            }
            catch (UnhandledException)
            {
                _main.DisplayUnhandledExceptionMessage();
            }
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
