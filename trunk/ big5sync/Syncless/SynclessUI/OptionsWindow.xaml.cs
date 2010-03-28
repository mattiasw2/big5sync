using System;
using System.Windows;
using System.Windows.Input;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {		        
		public OptionsWindow()
        {
            InitializeComponent();
            InitializeOptions();
        }
		
		private void InitializeOptions() {
			ChkBoxRegistryIntegration.IsChecked = Properties.Settings.Default.EnableShellIntegration;
            ChkBoxMinimizeToTray.IsChecked = Properties.Settings.Default.MinimizeToTray;
		}

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			bool choice = ChkBoxRegistryIntegration.IsChecked.Value;
			
			if(choice)
			{ 
				string appPath = (string) Application.Current.Properties["AppPath"];
				
				RegistryHelper.CreateRegistry(appPath);
			} else 
			{
            	RegistryHelper.RemoveRegistry();
			}

            CloseWindow();
			Properties.Settings.Default.EnableShellIntegration = choice;
            Properties.Settings.Default.MinimizeToTray = (bool) ChkBoxMinimizeToTray.IsChecked;
			Properties.Settings.Default.Save();
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseWindow();
        }

		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Application.Current.Properties["OptionsWindowIsOpened"] = true;
		}

		private void Window_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Application.Current.Properties["OptionsWindowIsOpened"] = false;
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