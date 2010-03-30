using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Core;
using Syncless.Notification;
using SynclessUI;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace SynclessUI.Notification
{
    public class SyncProgressWatcher : ISyncProgressObserver
    {
        #region ISyncProgressObserver Members
        private MainWindow _main;
        private SyncProgress _progress;
        private string _tagName;
        public SyncProgress Progress
        {
            get { return _progress; }
            set { _progress = value; }
        }

        public SyncProgressWatcher(MainWindow main, string tagName, SyncProgress p)
        {
            _main = main;
            _tagName = tagName;
            _progress = p;
            _progress.AddObserver(this);
            SyncStart();
        }

        private void SyncStart()
        {
            _main.LblStatusText.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (Action)(() =>
            {
                _main.ProgressNotifySyncStart();
            }));
            StateChanged();
        }

        public void StateChanged()
        {
            if (_progress.State == SyncState.Analyzing)
            {
                _main.ProgressBarSync.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                             (Action)(() =>
                                                                           {
                                                                               _main.ProgressNotifyAnalyzing();
                                                                           }));
            }
            else if (_progress.State == SyncState.Synchronizing)
            {
                _main.ProgressBarSync.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                             (Action)(() =>
                                                                           {
                                                                               _main.ProgressNotifySynchronizing();
                                                                           }));
            }
            else if (_progress.State == SyncState.Finalizing)
            {
                _main.ProgressBarSync.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                             (Action)(() =>
                                                             {
                                                                 _main.ProgressNotifyFinalizing();
                                                             }));
            }
            else if (_progress.State == SyncState.Finished)
            {
                _main.ProgressBarSync.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                             (Action)(() =>
                                                             {
                                                                 _main.ProgressNotifySyncComplete();
                                                             }));
            }

            Console.WriteLine("State Changed (New State : " + _progress.State + ")");
        }

        public void ProgressChanged()
        {

            _main.ProgressBarSync.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
           (Action)(() =>
            {
                _main.ProgressNotifyChange();
            }));

            ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Current Percent : " + _progress.PercentComplete + "(" + _progress.Message + ")");
        }

        public void SyncComplete()
        {
            StateChanged();
            ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Sync Complete");
        }

        #endregion
    }
}
