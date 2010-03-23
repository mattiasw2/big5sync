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
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(string caption, string message)
        {
            InitializeComponent();

            LblCaption.Content = caption;
            TxtBlkMessageBoxText.Text = message;
			
			ImgIcon.Source = GetSystemImage(MessageBoxImage.Error);
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

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
