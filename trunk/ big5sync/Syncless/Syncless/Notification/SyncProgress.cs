using System;
using System.Collections.Generic;
using System.Threading;

namespace Syncless.Notification
{
    public class SyncProgress : Progress
    {
        private List<ISyncProgressObserver> _observerList;
        private bool _notifyProgressChange;
        private bool _notifyStateChange;
        private bool _completed;
        private Thread _worker;
        
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);

        

        

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
                if (State == SyncState.Analyzing)
                {
                    return 0;
                }
                if (State == SyncState.Synchronizing)
                {
                    double completed = _syncCompletedJobs + _syncFailedJobs;
                    double total = _syncjobtotal;
                    return completed / total * 100.0;
                }
                if (State == SyncState.Finalizing)
                {
                    double completed = _finalisingCompletedJobs;
                    double total = _finalisingJobTotal;
                    return completed / total * 100.0;
                }
                if (State == SyncState.Finished)
                {
                    return 100;
                }
                return 0;
            }
        }

        public override void Complete()
        {
            switch (State)
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

        public override void Fail()
        {
            switch (State)
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

        public override void Cancel()
        {
            State = SyncState.Cancelled;
            if (_worker != null)
            {
                _wh.Set();
            }
        }

       

        public override void Update()
        {
            InvokeChange();
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

        public SyncProgress(string tagName):base(tagName)
        {
            
            _syncjobtotal = 0;
            _syncFailedJobs = 0;
            _syncCompletedJobs = 0;
            _observerList = new List<ISyncProgressObserver>();
            _notifyProgressChange = false;
            _notifyStateChange = false;
            _completed = false;
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
            if (State != SyncState.Started)
            {
                return false;
            }
            State = SyncState.Analyzing;

            TriggerStateChanged();

            _wh.Set();
            return true;
        }
        public bool ChangeToSyncing(int jobcount)
        {
            _syncjobtotal = jobcount;
            if (State != SyncState.Analyzing)
            {
                return false;
            }
            State = SyncState.Synchronizing;

            TriggerStateChanged();

            _wh.Set();
            return true;
        }
        public bool ChangeToFinalizing(int jobcount)
        {
            _finalisingJobTotal = jobcount;
            if (State != SyncState.Synchronizing)
            {
                return false;
            }

            State = SyncState.Finalizing;

            TriggerStateChanged();

            _wh.Set();
            return true;
        }
        public bool ChangeToFinished()
        {

            if (State != SyncState.Finalizing)
            {
                return false;
            }

            _completed = true;
            State = SyncState.Finished;
            TriggerSyncComplete();

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
            while (!_completed && State != SyncState.Cancelled)
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
