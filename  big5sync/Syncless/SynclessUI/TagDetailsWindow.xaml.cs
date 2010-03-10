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
using Syncless.Filters;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for TagDetailsWindow.xaml
    /// </summary>
    public partial class TagDetailsWindow : Window
    {		
		private MainWindow _main;
		private string _tagname;
        private List<Filter> currentfilters;
        private List<string> filterlist;
        
		public TagDetailsWindow(string tagname, MainWindow main)
        {
            InitializeComponent();
			_main = main;
			_tagname = tagname;
			LblTag_Details.Content = "Tag Details for " + _tagname;
            //PopulateFilters();
        }

        private void PopulateFilters()
        {
            currentfilters = _main.gui.GetAllFilters(_tagname);
            filterlist = new List<string>();
            foreach (Filter f in currentfilters)
            {
                if(f is ExtensionFilter) {
                    ExtensionFilter ef = (ExtensionFilter) f;
                    filterlist.Add("[Extension] " + ef.Pattern);
                }
            }

            ListFilters.ItemsSource = filterlist;

            if (currentfilters.Count != 0)
            {
                ExtensionFilter ef = (ExtensionFilter) currentfilters[0];
                TxtBoxPattern.Text = ef.Pattern;
                if (ef.Mode == Syncless.Filters.FilterMode.INCLUDE)
                    CmbBoxMode.SelectedIndex = 0;
                else if (ef.Mode == Syncless.Filters.FilterMode.EXCLUDE)
                    CmbBoxMode.SelectedItem = 1;
            }
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
            ExtensionFilter ef = (ExtensionFilter) FilterFactory.CreateExtensionFilter("", Syncless.Filters.FilterMode.INCLUDE);

            filterlist.Add("[Extension] " + ef.Pattern);
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
