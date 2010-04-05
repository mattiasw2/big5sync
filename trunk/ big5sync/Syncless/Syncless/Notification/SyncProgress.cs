using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Syncless.CompareAndSync.CompareObject;
using Syncless.Core;
using Syncless.Notification.UINotification;

namespace Syncless.Notification
{
    public class SyncProgress
    {
        private List<ISyncProgressObserver> _observerList;
        private SyncState _state;
        private bool _notifyProgressChange;
        private bool _notifyStateChange;
        private bool _completed;
        private Thread _worker;
        public string TagName
        {
            get;
            set;
        }
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);

        public SyncState State
        {
            get { return _state; }
        }

        public string Message
        {
            get;
            set;
        }

        #region Sync
        private int _syncjobtotal;
        public int SyncJobTotal
        {
            get { return _syncjobtotal; }
        }
        private int _syncCompletedJobs;
        public int SyncCompletedJobs
        {
            get { return _syncCompletedJobs; }
        }
        private int _syncFailedJobs;
        public int SyncFailedJobs
        {
            get { return _syncFailedJobs; }
        }
        #endregion

        #region Finalising
        private int _finalisingJobTotal;
        private int _finalisingCompletedJobs;
        private int _finalisingFailedJobs;
        private AbstractNotification _notification;

        public int FinalisingJobTotal
        {
            get { return _finalisingJobTotal; }
            set { _finalisingJobTotal = value; }
        }
        public int FinalisingCompletedJobs
        {
            get { return _finalisingCompletedJobs; }
            set { _finalisingCompletedJobs = value; }
        }
        public int FinalisingFailedJobs
        {
            get { return _finalisingFailedJobs; }
            set { _finalisingFailedJobs = value; }
        }
        #endregion

        public double PercentComplete
        {
            get
            {
                if (_state == SyncState.Analyzing)
                {
                    return 0;
                }
                if (_state == SyncState.Synchronizing)
                {
                    double completed = _syncCompletedJobs + _syncFailedJobs;
                    double total = _syncjobtotal;
                    return completed / total * 100.0;
                }
                if (_state == SyncState.Finalizing)
                {
                    double completed = _finalisingCompletedJobs;
                    double total = _finalisingJobTotal;
                    return completed / total * 100.0;
                }
                if (_state == SyncState.Finished)
                {
                    return 100;
                }
                return 0;
            }
        }

        public void complete()
        {
            switch (_state)
            {
                case SyncState.Synchronizing:
                    if (_syncCompletedJobs + _syncFailedJobs >= _syncjobtotal)
                    {
                        return;
                    }
                    _syncCompletedJobs++;
                    break;
                case SyncState.Finalizing:
                    if (_finalisingCompletedJobs + _finalisingFailedJobs >= _finalisingJobTotal)
                    {
                        return;
                    }
                    _finalisingCompletedJobs++;
                    break;
                default: return;
            }
            InvokeChange();
        }

        public void fail()
        {
            switch (_state)
            {
                case SyncState.Synchronizing:
                    if (_syncCompletedJobs + _syncFailedJobs >= _syncjobtotal)
                    {
                        return;
                    }
                    _syncFailedJobs++;
                    break;
                case SyncState.Finalizing:
                    if (_finalisingCompletedJobs + _finalisingFailedJobs >= _finalisingJobTotal)
                    {
                        return;
                    }
                    _finalisingFailedJobs++;
                    break;
                default: return;
            }
            InvokeChange();

        }

        public void Cancel()
        {
            _state = SyncState.Cancelled;
            if (_worker != null)
            {
                _wh.Set();
            }
        }

        public bool IsCancel
        {
            get { return _state == SyncState.Cancelled; }
        }

            private void InvokeChange()
        {
            if (!_notifyProgressChange)
            {
                _notifyProgressChange = true;
                //_worker.Interrupt();
                _wh.Set();
            }
        }
        
        public SyncProgress(string tagName)
        {
            _state = SyncState.Started;
            _syncjobtotal = 0;
            _syncFailedJobs = 0;
            _syncCompletedJobs = 0;
            _observerList = new List<ISyncProgressObserver>();
            _notifyProgressChange = false;
            _notifyStateChange = false;
            _completed = false;
            TagName = tagName;
            _worker = new Thread(Notifier);
            _worker.Start();
        }

        public void AddObserver(ISyncProgressObserver obs)
        {
            if (_observerList.Contains(obs))
            {
                return;
            }
            _observerList.Add(obs);
        }
        public void RemoveObserver(ISyncProgressObserver obs)
        {
            if (_observerList.Contains(obs))
            {
                _observerList.Remove(obs);
            }
        }

        public bool ChangeToAnalyzing()
        {
            if (_state != SyncState.Started)
            {
                return false;
            }
            _state = SyncState.Analyzing;
            
            TriggerStateChanged();
            
            _wh.Set();
            return true;
        }
        public bool ChangeToSyncing(int jobcount)
        {
            _syncjobtotal = jobcount;
            if (_state != SyncState.Analyzing)
            {
                return false;
            }
            _state = SyncState.Synchronizing;
            
            TriggerStateChanged();
            
            _wh.Set();
            return true;
        }
        public bool ChangeToFinalizing(int jobcount)
        {
            _finalisingJobTotal = jobcount;
            if (_state != SyncState.Synchronizing)
            {
                return false;
            }

            _state = SyncState.Finalizing;
            
            TriggerStateChanged();
            
            _wh.Set();
            return true;
        }
        public bool ChangeToFinished(AbstractNotification notification)
        {
            
            if (_state != SyncState.Finalizing)
            {
                return false;
            }
            _notification = notification;
            _completed = true;
            _state = SyncState.Finished;
            TriggerSyncComplete();
            ServiceLocator.UINotificationQueue().Enqueue(notification);
            _wh.Set();
            return true;
        }
        private void TriggerSyncComplete()
        {
            if (!_notifyStateChange)
            {
                _notifyStateChange = true;
            }
            _wh.Set();
        }
        private void TriggerStateChanged()
        {
            if (!_notifyStateChange)
            {
                _notifyStateChange = true;
            }
            _wh.Set();
        }

        #region thread
        private void Notifier()
        {
            while (!_completed && _state != SyncState.Cancelled)
            {
                if (_notifyStateChange)
                {
                    _notifyStateChange = false;
                    foreach (ISyncProgressObserver obs in _observerList)
                    {
                        obs.StateChanged();
                    }
                    continue;
                }
                if (_notifyProgressChange)
                {
                    _notifyProgressChange = false;
                    foreach (ISyncProgressObserver obs in _observerList)
                    {
                        obs.ProgressChanged();
                    }
                    continue;
                }
                _wh.WaitOne();
            }

            foreach (ISyncProgressObserver obs in _observerList)
            {
                obs.SyncComplete();
            }
        }

        #endregion
    }

}
