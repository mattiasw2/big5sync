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
using SynclessUI;
using System.Data;

namespace Syncless
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {		
        public MainWindow()
        {
			InitializeComponent();
						
			MinimizeToTray.Enable(this);
        }
		
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnClose_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			string messageBoxText = "Are you sure you want to exit Syncless?" +
				"\nExiting Syncless will disable seamless synchronization.";
			string caption = "Exit";
			MessageBoxButton button = MessageBoxButton.YesNo;
			MessageBoxImage icon = MessageBoxImage.Warning;
	
			MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
			
			switch (result) {
				case MessageBoxResult.Yes:
					this.Close();
					break;
				case MessageBoxResult.No:
					break;
			}        	
        }

        private void BtnMin_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			this.WindowState = WindowState.Minimized;
        }
    }
}
