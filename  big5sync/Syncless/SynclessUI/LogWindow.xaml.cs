using System;
using System.Collections.Generic;
using System.Data;
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
        private bool _closingAnimationNotCompleted = true;
        private bool _showApplicationLog = Settings.Default.ShowApplicationLog;
        private bool _showSynchronizationLog = Settings.Default.ShowSynchronizationLog;
        private bool _showFileSystemLog = Settings.Default.ShowFileSystemLog;

        public LogWindow(MainWindow main)
        {
            bool encounteredError = false;
            _main = main;
            Owner = _main;
            ShowInTaskbar = false;
            
            try
            {
                InitDataTable();
                PopulateLogData();
            }
            catch (LogFileCorruptedException)
            {
                encounteredError = true;
                DialogHelper.ShowError(this, "Log File Corrupted", "Stored log files have been corrupted and will be deleted.");
            }
            catch (UnhandledException)
            {
                encounteredError = true;
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }

            if (!encounteredError)
            {
                InitializeComponent();
                ChkBoxApplicationLog.IsChecked = _showApplicationLog;
                ChkBoxSynchronizationLog.IsChecked = _showSynchronizationLog;
                ChkBoxFileSystem.IsChecked = _showFileSystemLog;
                ShowDialog();
                datagrid.UpdateLayout();
            }
        }

        private void InitDataTable()
        {
            _LogData = new DataTable();
            _LogData.Columns.Add(new DataColumn("Category", typeof(string)));
            _LogData.Columns.Add(new DataColumn("Event Type", typeof(string)));
            _LogData.Columns.Add(new DataColumn("Message", typeof(string)));
            _LogData.Columns.Add(new DataColumn("Timestamp", typeof(string)));
        }

        public DataTable LogData
        {
            get { return _LogData; }
        }

        private void PopulateLogData()
        {
            List<LogData> log = _main.Gui.ReadLog();
            
            _LogData.Clear();
			
            foreach (LogData l in log)
            {
                DataRow row = _LogData.NewRow();
                
                string category = "";
                string eventType = "";

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

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            Close();
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
                SaveLogSettings();
                BtnOk.IsCancel = false;
				this.IsHitTestVisible = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }
		
		private void SaveLogSettings() {;
		    Settings.Default.ShowApplicationLog = _showApplicationLog;
		    Settings.Default.ShowSynchronizationLog = _showSynchronizationLog;
		    Settings.Default.ShowFileSystemLog = _showFileSystemLog;

            Settings.Default.Save();
		}

        private void ChkBoxApplicationLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _showApplicationLog = (bool)ChkBoxApplicationLog.IsChecked;
            PopulateLogData();
            datagrid.UpdateLayout();
        }

        private void ChkBoxSynchronizationLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _showSynchronizationLog = (bool)ChkBoxSynchronizationLog.IsChecked;
            PopulateLogData();
            datagrid.UpdateLayout();
        }

        private void ChkBoxFileSystem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _showFileSystemLog = (bool)ChkBoxFileSystem.IsChecked;
            PopulateLogData();
            datagrid.UpdateLayout();
        }

        private void BtnClearLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                _main.Gui.ClearLog();
                _LogData.Clear();
                datagrid.UpdateLayout();
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
            }
        }
    }
}