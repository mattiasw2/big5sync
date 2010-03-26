using System.Windows;
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
			
			Properties.Settings.Default.EnableShellIntegration = choice;
			Properties.Settings.Default.Save();
			this.Close();
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
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
    }
}
