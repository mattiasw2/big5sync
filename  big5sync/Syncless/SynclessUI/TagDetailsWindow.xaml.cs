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
using Ionic.Utils;
using System.IO;
using Syncless.Core;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for TagDetailsWindow.xaml
    /// </summary>
    public partial class TagDetailsWindow : Window
    {		
		private MainWindow _main;
        
		public TagDetailsWindow(MainWindow main)
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
			this.Close();
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

		private void BtnAddFilter_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			List<string> source = new List<string>();
			source.Add("Test");
			ListFilters.ItemsSource = source;
		}

		private void BtnRemoveFilter_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// TODO: Add event handler implementation here.
		}

		private void TabItemFiltering_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			SpecificFilterGrid.Visibility = System.Windows.Visibility.Visible;
		}

		private void TabItemVersioning_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			SpecificFilterGrid.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void TabItemProperties_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			SpecificFilterGrid.Visibility = System.Windows.Visibility.Hidden;
		}
    }
}
