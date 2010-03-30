using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Syncless.Core.Exceptions;
using Syncless.Logging;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private DataTable _LogData;
        private MainWindow _main;

        public LogWindow(MainWindow main)
        {
            _main = main;
            bool encounteredError = false;

            try
            {
                List<LogData> log = _main.Gui.ReadLog();
                PopulateLogData(log);
            }
            catch (LogFileCorruptedException)
            {
                DialogHelper.ShowError("Log File Corrupted", "Stored log files have been corrupted and will be deleted.");
            }
            catch (UnhandledException)
            {
                encounteredError = true;
                DialogHelper.DisplayUnhandledExceptionMessage();
            }

            if (!encounteredError)
            {
                InitializeComponent();
                ShowDialog();
            }
        }

        public DataTable LogData
        {
            get { return _LogData; }
        }

        private void PopulateLogData(List<LogData> log)
        {
            _LogData = new DataTable();
            _LogData.Columns.Add(new DataColumn("Category", typeof (string)));
            _LogData.Columns.Add(new DataColumn("Event Type", typeof (string)));
            _LogData.Columns.Add(new DataColumn("Message", typeof (string)));
            _LogData.Columns.Add(new DataColumn("Timestamp", typeof (string)));

            foreach (LogData l in log)
            {
                LogEventType @event = l.LogEvent;

                DataRow row = _LogData.NewRow();


                string category = "";
                string eventType = "";

                switch (l.LogCategory)
                {
                    case LogCategoryType.APPEVENT:
                        category = "Application";
                        break;
                    case LogCategoryType.FSCHANGE:
                        category = "Filesystem";
                        break;
                    case LogCategoryType.SYNC:
                        category = "Sync";
                        break;
                    case LogCategoryType.UNKNOWN:
                        category = "Unknown";
                        break;
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
            CloseWindow();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseWindow()
        {
            FormFadeOut.Begin();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
        }
    }
}