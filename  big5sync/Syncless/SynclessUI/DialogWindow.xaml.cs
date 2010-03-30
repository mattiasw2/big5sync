using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Media;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
		
        public bool CannotBeClosed {
			get; set;
		}

        public DialogWindow(string caption, string message, MessageBoxImage mbimg)
        {
            InitializeComponent();
			CannotBeClosed = false;
			Application.Current.Properties["DialogWindowChoice"] = false;
            LblCaption.Content = caption;
            TxtBlkMessageBoxText.Text = message;

            ImgIcon.Source = GetSystemImage(mbimg);
			DisplayCommandPanel(mbimg);
            PlayDialogSound(mbimg);
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
                case MessageBoxImage.None:
					CannotBeClosed = true;
                    this.Title = "Termination In Progress";
					this.ProgressBarTermination.IsEnabled = true;
					this.ProgressBarTermination.Visibility = System.Windows.Visibility.Visible;
                    break;
			}
		}

        private static void PlayDialogSound(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Error:
                    SystemSounds.Beep.Play();
                    break;
                case MessageBoxImage.Exclamation:
                    SystemSounds.Exclamation.Play();
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
                case MessageBoxImage.None:
                    iconsource = System.Drawing.SystemIcons.Exclamation;
                    break;
			}
			if (iconsource == null) return null;
				else return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(iconsource.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		}

        private void BtnOkCP1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			BtnOkCP1.IsEnabled = false;
        	CloseWindow();
        }
		
        private void BtnOkCP2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			BtnOkCP2.IsEnabled = false;
			Application.Current.Properties["DialogWindowChoice"] = true;
        	CloseWindow();
        }
		
        private void BtnCancelCP2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			BtnCancelCP2.IsEnabled = false;
			Application.Current.Properties["DialogWindowChoice"] = false;
        	CloseWindow();
        }

        private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }
		
		public void CloseWindow() {
            FormFadeOut.Begin();
		}
		
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        	if(CannotBeClosed)
				e.Cancel = true;
        }
    }
}
