using System;
using System.Windows;
using System.Windows.Input;
using Syncless.Core.Exceptions;
using SynclessUI.Helper;

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
                        DialogsHelper.ShowError("Tag Already Exist", "Please specify another tagname.");
                    }
                    else
                    {
                        FormFadeOut.Begin();
                    }
                }
                else
                {
                    DialogsHelper.ShowError("Tagname Empty", "Please specify a tagname.");
                }
            }
            catch (UnhandledException)
            {
                DialogsHelper.DisplayUnhandledExceptionMessage();
            }
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	FormFadeOut.Begin();
        }

		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Keyboard.Focus(TxtBoxTagName);
		}

		private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.DragMove();
		}
		
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
