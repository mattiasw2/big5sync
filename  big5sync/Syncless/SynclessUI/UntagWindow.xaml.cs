using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Syncless.Core;
using Syncless.Core.Exceptions;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for UntagWindow.xaml
    /// </summary>
    public partial class UntagWindow : Window
    {		
		private MainWindow _main;
        
		public UntagWindow(MainWindow main, string clipath)
        {
            try
            {
                InitializeComponent();

                _main = main;

                List<string> tagListByFolder = _main.gui.GetTags(new DirectoryInfo(clipath));
                if (tagListByFolder.Count != 0)
                {
                    TxtBoxPath.Text = clipath;
                    taglist.ItemsSource = tagListByFolder;
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
                if (taglist.SelectedIndex == -1) return;

                foreach (string t in taglist.SelectedItems)
                {
                    int result = _main.gui.Untag(t, new DirectoryInfo(TxtBoxPath.Text));
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
