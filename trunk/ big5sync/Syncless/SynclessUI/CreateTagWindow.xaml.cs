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
using Syncless.Core.Exceptions;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for CreateTagWindow.xaml
    /// </summary>
    public partial class CreateTagWindow : Window
    {		
		private MainWindow _main;
        
		public CreateTagWindow(MainWindow main)
        {
            InitializeComponent();
			
			_main = main;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string tagName = TxtBoxTagName.Text.Trim();

                if (tagName != "")
                {
                    bool tagexists = false;

                    tagexists = _main.CreateTag(tagName);

                    if (!tagexists)
                    {
                        string messageBoxText = "Please specify another tagname.";
                        string caption = "Tag Already Exist";
                        MessageBoxButton button = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Error;

                        MessageBox.Show(messageBoxText, caption, button, icon);
                    }
                    else
                    {
                        this.Close();
                    }
                }
                else
                {
                    string messageBoxText = "Please specify a tagname.";
                    string caption = "Tagname Empty";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
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

		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Keyboard.Focus(TxtBoxTagName);
		}
    }
}
