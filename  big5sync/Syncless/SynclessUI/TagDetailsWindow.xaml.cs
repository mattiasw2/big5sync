using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Syncless.Core.Exceptions;
using Syncless.Filters;
using SynclessUI.Helper;

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
        private bool _closingAnimationNotCompleted = true;

        public TagDetailsWindow(string tagname, MainWindow main)
        {
            try
            {
                InitializeComponent();
                _main = main;
                _tagname = tagname;
                LblTag_Details.Content = "Tag Details for " + _tagname;
                filters = _main.Gui.GetAllFilters(_tagname);
                PopulateFilterStringList(false);
                TxtBoxPattern.IsEnabled = false;
                CmbBoxMode.IsEnabled = false;
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage();
            }
        }

        private void PopulateFilterStringList(bool selectoriginal)
        {
            int index = -1;

            if (selectoriginal)
            {
                index = ListFilters.SelectedIndex;
            }

            var generatedFilterStringList = new List<string>();

            int i = 1;

            foreach (Filter f in filters)
            {
                if (f is ExtensionFilter)
                {
                    var ef = (ExtensionFilter) f;
                    generatedFilterStringList.Add(i + ". " + ef.Pattern);
                }
                i++;
            }

            ListFilters.ItemsSource = null;
            ListFilters.ItemsSource = generatedFilterStringList;

            if (selectoriginal)
            {
                ListFilters.SelectedIndex = index;
            }

            if (generatedFilterStringList.Count == 0)
            {
                TxtBoxPattern.IsEnabled = false;
                TxtBoxPattern.Text = "";
                CmbBoxMode.IsEnabled = false;
                CmbBoxMode.SelectedIndex = -1;
            }
        }

        private void SelectFilter(Filter f)
        {
            int index = filters.IndexOf(f);
            ListFilters.SelectedIndex = index;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            try
            {
                if (!_main.Gui.GetTag(_tagname).IsLocked)
                {
                    bool result = _main.Gui.UpdateFilterList(_tagname, filters);
                    Close();
                }
                else
                {
                    BtnOk.IsEnabled = true;
                    DialogHelper.ShowError(_tagname + " is Synchronizing",
                                           "You cannot update tag details while the tag is synchronizing.");
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage();
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            Close();
        }

        private void BtnAddFilter_Click(object sender, RoutedEventArgs e)
        {
            var ef = (ExtensionFilter) FilterFactory.CreateExtensionFilter("*.*", FilterMode.INCLUDE);
            filters.Add(ef);

            PopulateFilterStringList(true);
            SelectFilter(ef);
        }

        private void BtnRemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            if (ListFilters.SelectedIndex != -1)
            {
                int index = ListFilters.SelectedIndex;
                filters.RemoveAt(index);

                PopulateFilterStringList(false);
            }
        }

        private void ListFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ListFilters.SelectedIndex;

            if (index != -1)
            {
                TxtBoxPattern.IsEnabled = true;
                CmbBoxMode.IsEnabled = true;

                Filter f = filters[index];
                if (f is ExtensionFilter)
                {
                    var ef = (ExtensionFilter) f;
                    TxtBoxPattern.Text = ef.Pattern;
                    if (ef.Mode == FilterMode.INCLUDE)
                        CmbBoxMode.SelectedIndex = 0;
                    else if (ef.Mode == FilterMode.EXCLUDE)
                        CmbBoxMode.SelectedIndex = 1;
                }
            }
        }

        private void CmbBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListFilters.SelectedIndex != -1 && ListFilters.SelectedIndex <= filters.Count)
            {
                Filter f = filters[ListFilters.SelectedIndex];

                if (CmbBoxMode.SelectedIndex == 0)
                    f.Mode = FilterMode.INCLUDE;
                else if (CmbBoxMode.SelectedIndex == 1)
                    f.Mode = FilterMode.EXCLUDE;
            }
        }

        private void TabItemFiltering_GotFocus(object sender, RoutedEventArgs e)
        {
            SpecificFilterGrid.Visibility = Visibility.Visible;
        }

        private void TabItemProperties_GotFocus(object sender, RoutedEventArgs e)
        {
            SpecificFilterGrid.Visibility = Visibility.Hidden;
        }

        private void TabItemVersioning_GotFocus(object sender, RoutedEventArgs e)
        {
            SpecificFilterGrid.Visibility = Visibility.Hidden;
        }

        private void TxtBoxPattern_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ListFilters.SelectedIndex != -1)
            {
                if(!CheckIfFilterExist(TxtBoxPattern.Text))
                {
                    Filter f = filters[ListFilters.SelectedIndex];
                    if (f is ExtensionFilter)
                    {
                        var ef = (ExtensionFilter)f;
                        ef.Pattern = TxtBoxPattern.Text;
                    }

                    PopulateFilterStringList(true);
                }                
            }
        }

        private bool CheckIfFilterExist(string pattern)
        {
            bool exist = false;

            foreach(Filter f in filters)
            {
                ExtensionFilter ef = (ExtensionFilter) f;
                if(ef.Pattern == pattern)
                {
                    exist = true;
                    break;
                }
            }

            return exist;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            _closingAnimationNotCompleted = false;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closingAnimationNotCompleted)
            {
                BtnCancel.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }
    }
}