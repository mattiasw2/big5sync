using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Notification;
using SynclessUI;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace SynclessUI.Notification
{
    internal class SyncProgressWatcher : ISyncProgressObserver
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
                _main.NotifySyncStart(_tagName);
            }));
            StateChanged();
        }

        public void StateChanged()
        {
            if (_progress.State == SyncState.ANALYZING)
            {
                _main.ProgressBarSync.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               (Action)(() =>
               {
                   _main.NotifySyncAnalyzing(_tagName);
               }));
            }

            Console.WriteLine("State Changed (New State : " + _progress.State + ")");   
        }

        public void ProgressChanged()
        {
            _main.ProgressBarSync.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
           (Action)(() =>
            {
                _main.SetSyncProgress(_tagName, _progress);
            }));
            
            Console.WriteLine("Current Percent : "+_progress.PercentComplete + "("+_progress.Message+")");
        }

        public void SyncComplete()
        {
            Console.WriteLine("Sync Complete");
        }

        #endregion
    }
}
