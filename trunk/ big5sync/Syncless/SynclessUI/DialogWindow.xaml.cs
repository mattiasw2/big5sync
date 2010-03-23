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
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow(string caption, string message, MessageBoxImage mbimg)
        {
            InitializeComponent();
			
			Application.Current.Properties["DialogWindowChoice"] = false;
            LblCaption.Content = caption;
            TxtBlkMessageBoxText.Text = message;

            ImgIcon.Source = GetSystemImage(mbimg);
			DisplayCommandPanel(mbimg);
        }
		
		private void DisplayCommandPanel(MessageBoxImage icon) {
			switch (icon)
			{
				case MessageBoxImage.Error:
					this.Title = "Error";
				    this.OkCommandPanel.Visibility = System.Windows.Visibility.Visible;
					break;
				case MessageBoxImage.Exclamation:
					this.Title = "Warning";
					this.OkCancelCommandPanel.Visibility = System.Windows.Visibility.Visible;
				    break;
				case MessageBoxImage.Information:
					this.Title = "Information";
					this.OkCommandPanel.Visibility = System.Windows.Visibility.Visible;
				    break;
			}
		}
		
		private static ImageSource GetSystemImage(MessageBoxImage icon)
		{
			System.Drawing.Icon iconsource = null;
			switch (icon)
			{
				case MessageBoxImage.Error:
					iconsource = System.Drawing.SystemIcons.Error;
				    break;
				case MessageBoxImage.Exclamation:
					iconsource = System.Drawing.SystemIcons.Exclamation;
				    break;
				case MessageBoxImage.Information:
					iconsource = System.Drawing.SystemIcons.Information;
				    break;
				case MessageBoxImage.Question:
					iconsource = System.Drawing.SystemIcons.Question;
                    break;
			}
			if (iconsource == null) return null;
				else return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(iconsource.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		}


        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOkCP1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	this.Close();
        }
		
        private void BtnOkCP2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			Application.Current.Properties["DialogWindowChoice"] = true;
        	this.Close();
        }
		
        private void BtnCancelCP2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			Application.Current.Properties["DialogWindowChoice"] = false;
        	this.Close();
        }
    }
}
