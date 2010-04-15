/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.Windows.Threading;
using Syncless.Core;
using Syncless.Notification;

namespace SynclessUI.Notification
{
    /// <summary>
    /// SyncProgressWatcher to watch a particular SyncProgress as an observer for any changes to the SyncProgress
    /// </summary>
    public class SyncProgressWatcher : ISyncProgressObserver
    {
        #region ISyncProgressObserver Members
        private MainWindow _main;
        private SyncProgress _progress;

        /// <summary>
        /// Current Progress to be tracked by the SyncProgressWatcher
        /// </summary>
        public SyncProgress Progress
        {
            get { return _progress; }
            set { _progress = value; }
        }

        /// <summary>
        /// Initialize the SyncProgressWatcher Object and adds the SyncProgressWatcher as an observer to the progress
        /// </summary>
        /// <param name="main">Reference to the MainWindow</param>
        /// <param name="tagName">Tagname associated with the SyncProgressWatcher</param>
        /// <param name="p">SyncProgress to watch</param>
        public SyncProgressWatcher(MainWindow main, string tagName, SyncProgress p)
        {
            _main = main;
            _progress = p;
            _progress.AddObserver(this);
            SyncStart();
        }

        /// <summary>
        /// Notifies the MainWindow that a Sync Operation has started
        /// Also calls StateChanged()
        /// </summary>
        private void SyncStart()
        {
            _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                _main.ProgressNotifySyncStart(Progress);
                StateChanged();
            }));
        }

        /// <summary>
        /// Invokes StateChanged()
        /// </summary>
        public void InvokeStateChanged()
        {
            _main.ProgressBarSync.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(StateChanged));
        }

        /// <summary>
        /// Check _progress.State and activates the corresponding method of the MainWindow
        /// </summary>
        public void StateChanged()
        {
            switch (_progress.State)
            {
                case SyncState.Analyzing:
                    _main.ProgressNotifyAnalyzing(Progress);
                    break;
                case SyncState.Synchronizing:
                    _main.ProgressNotifySynchronizing(Progress);
                    break;
                case SyncState.Finalizing:
                    _main.ProgressNotifyFinalizing(Progress);
                    break;
                case SyncState.Finished:
                    _main.ProgressNotifySyncComplete(Progress);
                    break;
            }
        }

        /// <summary>
        /// Notifies the MainWindow that the sync progress has changed by calling the ProgressNotifyChange method in the MainWindow
        /// </summary>
        public void ProgressChanged()
        {
            _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => _main.ProgressNotifyChange(Progress)));
            ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Current Percent : " + _progress.PercentComplete + "(" + _progress.Message + ")");
        }

        /// <summary>
        /// Notifies the MainWindow that sync has completed by calling the StateChanged() method
        /// </summary>
        public void SyncComplete()
        {
            _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(StateChanged));
            ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Sync Complete");
        }

        #endregion
    }
}
