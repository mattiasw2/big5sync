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
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete

        /// <summary>
        /// Initializes the Tag Details Window
        /// </summary>
        /// <param name="main">Reference to the Main Window</param>
        /// <param name="tagname">Tag to show in the Details Window</param>
        public TagDetailsWindow(MainWindow main, string tagname)
        {
            try
            {
                InitializeComponent();
                _main = main;
                _tagname = tagname;

                // Sets up general window properties
                Owner = _main;
                ShowInTaskbar = false;

                // Sets up all components to their default state
                filters = _main.Gui.GetAllFilters(_tagname);
                PopulateListBoxFilter(false);
                LblTag_Details.Content = "Tag Details for " + _tagname;
                TxtBoxPattern.IsEnabled = false;
                CmbBoxMode.IsEnabled = false;
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        #region General Window Components & Related Events
        /// <summary>
        /// Event handler for Canvas_MouseLeftButtonDown event. Allows user to drag the canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// On closing animation complete, set the boolean to false and closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            _closingAnimationNotCompleted = false;
            Close();
        }

        /// <summary>
        /// Event handler for Window_Closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closingAnimationNotCompleted)
            {
                BtnCancel.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        } 
        #endregion

        #region Filters
        /// <summary>
        /// Populates the ListBoxFilter by generating the new list and updates other elements of the UI correspondingly
        /// </summary>
        /// <param name="selectOriginal">Specifies if the original list selection shld be selected after repopulation</param>
        private void PopulateListBoxFilter(bool selectOriginal)
        {
            int index = -1;

            if (selectOriginal)
            {
                index = ListBoxFilters.SelectedIndex;
            }

            List<string> filterList = GenerateFilterListHelper();

            // Resets the ListBox with the new List
            ListBoxFilters.ItemsSource = null;
            ListBoxFilters.ItemsSource = filterList;

            if (selectOriginal)
            {
                ListBoxFilters.SelectedIndex = index;
            }

            // if no filters present, disable filter information
            if (filters.Count == 0)
            {
                TxtBoxPattern.IsEnabled = false;
                TxtBoxPattern.Text = "";
                CmbBoxMode.IsEnabled = false;
                CmbBoxMode.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Generates the filter list for the ListBoxFilter
        /// </summary>
        /// <returns>The generated filter list</returns>
        private List<string> GenerateFilterListHelper()
        {
            var filterList = new List<string>();
            int filterIndex = 1;
            foreach (Filter f in filters)
            {
                if (f is ExtensionFilter)
                {
                    var ef = (ExtensionFilter)f;

                    string mode = string.Empty;

                    switch (ef.Mode)
                    {
                        case FilterMode.INCLUDE:
                            mode = "[Inclusion] ";
                            break;
                        case FilterMode.EXCLUDE:
                            mode = "[Exclusion] ";
                            break;
                    }

                    filterList.Add(filterIndex + ". " + mode + " " + ef.Pattern);
                }
                filterIndex++;
            }
            return filterList;
        }

        /// <summary>
        /// Selects the filter in ListBoxFilter by Filter
        /// </summary>
        /// <param name="f">Filter to select</param>
        private void SelectFilterInListBoxFilter(Filter f)
        {
            int index = filters.IndexOf(f);
            ListBoxFilters.SelectedIndex = index;
        }

        /// <summary>
        /// Checks if there are redudant filters. Does this by removing all filters one by one and checking if a similar
        /// filter can be found.
        /// </summary>
        /// <returns>Returns true if a duplicate filter is found. Else returns false.</returns>
        private bool CheckDuplicateFilters()
        {
            for (int i = 0; i < filters.Count; i++)
            {
                Filter fi = filters[i];
                filters.RemoveAt(i);
                if (filters.Contains(fi))
                {
                    filters.Insert(i, fi);
                    return true;
                }
                filters.Insert(i, fi);
            }

            return false;
        }

        /// <summary>
        /// Adds a default filter to the filters and updates the ListBoxFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddFilter_Click(object sender, RoutedEventArgs e)
        {
            var ef = (ExtensionFilter)FilterFactory.CreateExtensionFilter("*.*", FilterMode.INCLUDE);
            filters.Add(ef);

            PopulateListBoxFilter(true);
            SelectFilterInListBoxFilter(ef);
        }

        /// <summary>
        /// Removes the filter from filters and repopulates the ListBoxFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxFilters.SelectedIndex != -1)
            {
                int index = ListBoxFilters.SelectedIndex;
                filters.RemoveAt(index);

                PopulateListBoxFilter(false);
            }
        }

        /// <summary>
        /// On selection change, update the filter information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ListBoxFilters.SelectedIndex;

            if (index != -1)
            {
                TxtBoxPattern.IsEnabled = true;
                CmbBoxMode.IsEnabled = true;

                Filter f = filters[index];
                if (f is ExtensionFilter)
                {
                    var ef = (ExtensionFilter)f;
                    TxtBoxPattern.Text = ef.Pattern;
                    if (ef.Mode == FilterMode.INCLUDE)
                        CmbBoxMode.SelectedIndex = 0;
                    else if (ef.Mode == FilterMode.EXCLUDE)
                        CmbBoxMode.SelectedIndex = 1;
                }
            }
        }

        /// <summary>
        /// On mode change, update the filter and repopulate the ListBoxFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxFilters.SelectedIndex != -1 && ListBoxFilters.SelectedIndex <= filters.Count)
            {
                Filter f = filters[ListBoxFilters.SelectedIndex];

                if (CmbBoxMode.SelectedIndex == 0)
                    f.Mode = FilterMode.INCLUDE;
                else if (CmbBoxMode.SelectedIndex == 1)
                    f.Mode = FilterMode.EXCLUDE;

                PopulateListBoxFilter(true);
            }
        }

        /// <summary>
        /// Event handler for TxtBoxPattern_LostFocus. Updates the filterlist and repopulates the corresponding UI ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBoxPattern_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ListBoxFilters.SelectedIndex != -1)
            {
                Filter f = filters[ListBoxFilters.SelectedIndex];
                if (f is ExtensionFilter)
                {
                    var ef = (ExtensionFilter)f;
                    ef.Pattern = TxtBoxPattern.Text;
                }

                PopulateListBoxFilter(true);
            }
        }

        /// <summary>
        /// Event Handler for TxtBoxPattern_PreviewLostKeyboardFocus. Detects if pattern is empty, and stops users from
        /// changing focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBoxPattern_PreviewLostKeyboardFocus(object sender,
                                                            System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (TxtBoxPattern.Text.Trim() == string.Empty && BtnCancel != e.NewFocus)
            {
                e.Handled = true;

                DialogHelper.ShowError(this, "Extension Mask Cannot be Empty", "Please input a valid extension mask.");
            }
        } 
        #endregion

        #region TabMenu
        /// <summary>
        /// Show the grid for filter information upon selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabItemFiltering_GotFocus(object sender, RoutedEventArgs e)
        {
            GridFilterInformation.Visibility = Visibility.Visible;
        } 
        #endregion

        #region Command Panel
        /// <summary>
        /// Checks to see if the tag's filters can be updated, if so, updates it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            try
            {
                // Check if particular tag is locked
                if (_main.Gui.GetTag(_tagname).IsLocked)
                {
                    BtnOk.IsEnabled = true;
                    DialogHelper.ShowError(this, _tagname + " is Synchronizing",
                                           "You cannot update tag details while the tag is synchronizing.");
                    return;
                }

                // Check if there are duplicate filters 
                if (CheckDuplicateFilters())
                {
                    DialogHelper.ShowError(this, "Duplicate Filters", "Please remove all duplicate filters.");
                    BtnOk.IsEnabled = true;
                    return;
                }

                // Update filter list
                _main.Gui.UpdateFilterList(_tagname, filters);
                Close();
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
                Close();
            }
        }

        /// <summary>
        /// Event handler for BtnCancel_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            Close();
        } 
        #endregion
    }
}