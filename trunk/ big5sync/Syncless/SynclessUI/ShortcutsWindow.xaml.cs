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

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for ShortcutsWindow.xaml
    /// </summary>
    public partial class ShortcutsWindow : Window
    {
		public ShortcutsWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

		private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			this.Close();
		}

		private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.Close();
		}
    }
}
