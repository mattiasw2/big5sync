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
        private List<Filter> filters;
        
		public TagDetailsWindow(string tagname, MainWindow main)
        {
            InitializeComponent();
			_main = main;
			_tagname = tagname;
			LblTag_Details.Content = "Tag Details for " + _tagname;
			filters = _main.gui.GetAllFilters(_tagname);
			PopulateFilterStringList(false);
			TxtBoxPattern.IsEnabled = false;
			CmbBoxMode.IsEnabled = false;
        }
		
        private void PopulateFilterStringList(bool selectoriginal)
        {
			int index = -1;
			
			if(selectoriginal) {
				index = ListFilters.SelectedIndex;
			}
			
            List<string> generatedFilterStringList = new List<string>();
			
			int i = 1;
			
            foreach (Filter f in filters)
            {
                if (f is ExtensionFilter)
                {
                    ExtensionFilter ef = (ExtensionFilter)f;
                    generatedFilterStringList.Add(i + ". " + "[Ext.]" + ef.Pattern);
                }
				i++;
            }

            ListFilters.ItemsSource = null;
			ListFilters.ItemsSource = generatedFilterStringList;
			
			if(selectoriginal) {
				ListFilters.SelectedIndex = index;
			}
			
			if(generatedFilterStringList.Count == 0) {
				TxtBoxPattern.IsEnabled = false;
				TxtBoxPattern.Text = "";
				CmbBoxMode.IsEnabled = false;
				CmbBoxMode.SelectedIndex = -1;
			}
        }

        private void SelectFilter(Filter f) {
            int index = filters.IndexOf(f);
            ListFilters.SelectedIndex = index;
        }
		
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool result = _main.gui.UpdateFilterList(_tagname, filters);
			this.Close();
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

		private void BtnAddFilter_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            ExtensionFilter ef = (ExtensionFilter) FilterFactory.CreateExtensionFilter("*.*", Syncless.Filters.FilterMode.INCLUDE);
            filters.Add(ef);

            PopulateFilterStringList(false);
            SelectFilter(ef);
		}

		private void BtnRemoveFilter_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if(ListFilters.SelectedIndex != -1) {
				int index = ListFilters.SelectedIndex;
				filters.RemoveAt(index);

                TxtBoxPattern.IsEnabled = false;
                TxtBoxPattern.Text = "";
                CmbBoxMode.IsEnabled = false;
                CmbBoxMode.SelectedIndex = -1;
				
				PopulateFilterStringList(false);
			}
		}

		private void ListFilters_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
            int index = ListFilters.SelectedIndex;

            if (index != -1)
            {
				TxtBoxPattern.IsEnabled = true;
				CmbBoxMode.IsEnabled = true;
				
                Filter f = filters[index];
                if (f is ExtensionFilter)
                {
                    ExtensionFilter ef = (ExtensionFilter) f;
                    TxtBoxPattern.Text = ef.Pattern;
                    if (ef.Mode == Syncless.Filters.FilterMode.INCLUDE)
                        CmbBoxMode.SelectedIndex = 0;
                    else if (ef.Mode == Syncless.Filters.FilterMode.EXCLUDE)
                        CmbBoxMode.SelectedIndex = 1;
                }
            }
		}


		private void CmbBoxMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
            if (ListFilters.SelectedIndex != -1 && ListFilters.SelectedIndex <= filters.Count)
            {
                Filter f = filters[ListFilters.SelectedIndex];

                if (CmbBoxMode.SelectedIndex == 0)
                    f.Mode = Syncless.Filters.FilterMode.INCLUDE;
                else if (CmbBoxMode.SelectedIndex == 1)
                    f.Mode = Syncless.Filters.FilterMode.EXCLUDE;
            }
		}
		
        private void TabItemFiltering_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
        	SpecificFilterGrid.Visibility = System.Windows.Visibility.Visible;
        }
		
        private void TabItemProperties_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
        	SpecificFilterGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        private void TabItemVersioning_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
        	SpecificFilterGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        private void TxtBoxPattern_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ListFilters.SelectedIndex != -1)
            {
                Filter f = filters[ListFilters.SelectedIndex];
                if (f is ExtensionFilter)
                {
                    ExtensionFilter ef = (ExtensionFilter)f;
                    ef.Pattern = TxtBoxPattern.Text;
                }
                PopulateFilterStringList(true);
            }
        }
    }
}
