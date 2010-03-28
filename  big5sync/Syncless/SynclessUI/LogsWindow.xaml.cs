using System;
using System.Windows;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogsWindow : Window
    {		
		private MainWindow _main;
        
		public LogsWindow(MainWindow main)
        {
			_main = main;
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			this.Close();
        }

		private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.DragMove();
		}
    }
}
