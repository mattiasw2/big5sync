/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Syncless.Core.Exceptions;
using Syncless.Logging;
using SynclessUI.Helper;
using SynclessUI.Properties;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private DataTable _LogData;
        private MainWindow _main;
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete

        // Settings to decide if the particular log should be showed.
        private bool _showApplicationLog = Settings.Default.ShowApplicationLog;
        private bool _showSynchronizationLog = Settings.Default.ShowSynchronizationLog;
        private bool _showFileSystemLog = Settings.Default.ShowFileSystemLog;


        /// <summary>
        /// Initializes the LogWindow. LogWindow will not show upon any error during Initialization
        /// </summary>
        /// <param name="main">Reference to the Main Window</param>
        public LogWindow(MainWindow main)
        {
            bool encounteredInitError = false;
            _main = main;

            // Sets up general window properties
            Owner = _main;
            ShowInTaskbar = false;

            // Attemps to initialize and populate the Log DataTable
            try
            {
                InitializeLogDataTable();
                PopulateLogDataTable();
            }
            catch (LogFileCorruptedException)
            {
                encounteredInitError = true;
                DialogHelper.ShowError(this, "Log File Corrupted",
                                       "Stored log files have been corrupted and will be deleted.");
            }
            catch (UnhandledException)
            {
                encounteredInitError = true;
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }

            // If no error, proceed on to display the window and its components
            if (!encounteredInitError)
            {
                InitializeComponent();
                ChkBoxApplicationLog.IsChecked = _showApplicationLog;
                ChkBoxSynchronizationLog.IsChecked = _showSynchronizationLog;
                ChkBoxFileSystem.IsChecked = _showFileSystemLog;
                ShowDialog();
                dataGrid.UpdateLayout();
            }
        }

        #region LogDataTable

        /// <summary>
        /// Initializes the Log DataTable
        /// </summary>
        private void InitializeLogDataTable()
        {
            _LogData = new DataTable();
            _LogData.Columns.Add(new DataColumn("Category", typeof (string)));
            _LogData.Columns.Add(new DataColumn("Event Type", typeof (string)));
            _LogData.Columns.Add(new DataColumn("Message", typeof (string)));
            _LogData.Columns.Add(new DataColumn("Timestamp", typeof (string)));
        }

        /// <summary>
        /// Gets the Log DataTable
        /// </summary>
        public DataTable LogData
        {
            get { return _LogData; }
        }

        /// <summary>
        /// Populates the Log DataTable with data from the log file
        /// </summary>
        private void PopulateLogDataTable()
        {
            List<LogData> log = _main.LogicLayer.ReadLog();

            _LogData.Clear();

            // For each row in LogData, add to LogDataTable
            foreach (LogData l in log)
            {
                DataRow row = _LogData.NewRow();

                string category = "";
                string eventType = "";

                // Gets Log Category Information
                switch (l.LogCategory)
                {
                    case LogCategoryType.APPEVENT:
                        if (!_showApplicationLog) continue;
                        category = "Application";
                        break;
                    case LogCategoryType.FSCHANGE:
                        if (!_showFileSystemLog) continue;
                        category = "Filesystem";
                        break;
                    case LogCategoryType.SYNC:
                        if (!_showSynchronizationLog) continue;
                        category = "Sync";
                        break;
                    case LogCategoryType.UNKNOWN:
                        continue;
                }

                // Gets Log Event Information
                switch (l.LogEvent)
                {
                    case LogEventType.SYNC_STARTED:
                        eventType = "Sync Started";
                        break;
                    case LogEventType.SYNC_STOPPED:
                        eventType = "Sync Stopped";
                        break;
                    case LogEventType.APPEVENT_DRIVE_ADDED:
                        eventType = "Drive Added";
                        break;
                    case LogEventType.APPEVENT_DRIVE_RENAMED:
                        eventType = "Drive Renamed";
                        break;
                    case LogEventType.APPEVENT_PROFILE_LOAD_FAILED:
                        eventType = "Profile Loading Failed";
                        break;
                    case LogEventType.APPEVENT_TAG_CREATED:
                        eventType = "Tag Created";
                        break;
                    case LogEventType.APPEVENT_TAG_DELETED:
                        eventType = "Tag Deleted";
                        break;
                    case LogEventType.APPEVENT_TAG_CONFIG_UPDATED:
                        eventType = "Tag Updated";
                        break;
                    case LogEventType.APPEVENT_FOLDER_TAGGED:
                        eventType = "Folder Tagged";
                        break;
                    case LogEventType.APPEVENT_FOLDER_UNTAGGED:
                        eventType = "Folder Untagged";
                        break;
                    case LogEventType.FSCHANGE_CREATED:
                        eventType = "File Created";
                        break;
                    case LogEventType.FSCHANGE_MODIFIED:
                        eventType = "File Modified";
                        break;
                    case LogEventType.FSCHANGE_DELETED:
                        eventType = "File Deleted";
                        break;
                    case LogEventType.FSCHANGE_RENAMED:
                        eventType = "File Renamed";
                        break;
                    case LogEventType.FSCHANGE_ERROR:
                        eventType = "File Error";
                        break;
                    case LogEventType.FSCHANGE_ARCHIVED:
                        eventType = "File Archived";
                        break;
                    case LogEventType.FSCHANGE_CONFLICT:
                        eventType = "File Conflict";
                        break;
                    case LogEventType.UNKNOWN:
                        category = "Unknown";
                        break;
                }

                row["Category"] = category;
                row["Event Type"] = eventType;
                row["Message"] = l.Message;
                row["Timestamp"] = l.Timestamp;

                _LogData.Rows.Add(row);
            }
        }

        #endregion

        #region Command Panel

        /// <summary>
        /// Saves the Log Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            SaveLogSettings();
            Close();
        }

        /// <summary>
        /// Saves logs options into Application Settings
        /// </summary>
        private void SaveLogSettings()
        {
            Settings.Default.ShowApplicationLog = _showApplicationLog;
            Settings.Default.ShowSynchronizationLog = _showSynchronizationLog;
            Settings.Default.ShowFileSystemLog = _showFileSystemLog;

            Settings.Default.Save();
        }

        #endregion

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
        /// Event handler for Window_Closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closingAnimationNotCompleted)
            {
                BtnOk.IsCancel = false;
                IsHitTestVisible = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        #endregion

        #region LogDataGrid & Related Components

        /// <summary>
        /// Saves log options and repopulates datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBoxApplicationLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _showApplicationLog = (bool) ChkBoxApplicationLog.IsChecked;
            PopulatesDataGrid();
        }

        /// <summary>
        /// Saves log options and repopulates datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBoxSynchronizationLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _showSynchronizationLog = (bool) ChkBoxSynchronizationLog.IsChecked;
            PopulatesDataGrid();
        }

        /// <summary>
        /// Saves log options and repopulates datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBoxFileSystem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _showFileSystemLog = (bool) ChkBoxFileSystem.IsChecked;
            PopulatesDataGrid();
        }

        /// <summary>
        /// Repopulates the datagrid and updates its layout
        /// </summary>
        private void PopulatesDataGrid()
        {
            PopulateLogDataTable();
            dataGrid.UpdateLayout();
        }

        /// <summary>
        /// Clear the current log file and the datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClearLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                _main.LogicLayer.ClearLog();
                _LogData.Clear();
                dataGrid.UpdateLayout();
            }
            catch (IOException)
            {
                DialogHelper.ShowError(this, "Clear Log Failed", "Unable to clear logs at the moment. Please try again later.");
            }
            catch (UnauthorizedAccessException)
            {
                DialogHelper.ShowError(this, "Clear Log Failed", "Unable to clear logs at the moment. Please try again later.");
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }

        #endregion
    }
}