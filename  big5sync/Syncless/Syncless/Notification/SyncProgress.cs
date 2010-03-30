using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Syncless.Notification
{
    public class SyncProgress
    {
        private List<ISyncProgressObserver> _observerList;
        private SyncState _state;
        private bool _notifyProgressChange;
        private bool _completed;
        private Thread _worker;
        public SyncState State
        {
            get { return _state; }
            set { _state = value; }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
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
                else if (_state == SyncState.Synchronizing)
                {
                    double completed = _syncCompletedJobs + _syncFailedJobs;
                    double total = _syncjobtotal;
                    return completed / total * 100.0;
                }
                else if (_state == SyncState.Finalizing)
                {
                    double completed = _finalisingCompletedJobs;
                    double total = _finalisingJobTotal;
                    return completed / total * 100.0;
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

        public void Cancel()
        {
            _state = SyncState.Cancelled;
            if (_worker != null)
            {
                _worker.Abort();
            }
        }

        private void InvokeChange()
        {
            if(!_notifyProgressChange)
            {
                _notifyProgressChange= true;
                _worker.Interrupt();
            }
        }
        public delegate void MethodASync();
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
        public SyncProgress()
        {
            _syncjobtotal = 0;
            _syncFailedJobs = 0;
            _syncCompletedJobs = 0;
            _observerList = new List<ISyncProgressObserver>();
            _notifyProgressChange = false;
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
            if (_state != SyncState.Started)
            {
                return false;
            }
            _state = SyncState.Analyzing;
            _message = "Analyzing";
            TriggerStateChanged();
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
            _message = "Synchronzing";
            TriggerStateChanged();
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
            _message = "Finalizing";
            TriggerStateChanged();
            return true;
        }
        public bool ChangeToFinished()
        {
            if (_state != SyncState.Finalizing)
            {
                return false;
            }
            _completed = true;
            _message = "Finishing";
            //_state = SyncState.Finished;
            //TriggerStateChanged();
            TriggerSyncComplete();
            return true;
        }
        private void TriggerSyncComplete()
        {
            foreach (ISyncProgressObserver obs in _observerList)
            {
                MethodASync sync = new MethodASync(obs.SyncComplete);
                sync.BeginInvoke(null, null);
                //obs.SyncComplete(); 
            }
        }
        private void TriggerStateChanged()
        {
            foreach (ISyncProgressObserver obs in _observerList)
            {
                MethodASync sync = new MethodASync(obs.StateChanged);
                sync.BeginInvoke(null, null);
                //obs.StateChanged();
            }
        }

        #region thread
        private void Notifier()
        {
            while (!_completed)
            {
                try
                {
                    if (_notifyProgressChange)
                    {
                        _notifyProgressChange = false;
                        foreach (ISyncProgressObserver obs in _observerList)
                        {
                            obs.ProgressChanged();
                        }

                    }
                    Thread.Sleep(10000);
                }
                catch (ThreadInterruptedException)
                {
                }
                catch (ThreadAbortException)
                {
                    foreach (ISyncProgressObserver obs in _observerList)
                    {
                        obs.ProgressChanged();
                    }
                }
            }
            //DO a final State Change

            foreach (ISyncProgressObserver obs in _observerList)
            {
                obs.ProgressChanged();
            }
        }

        #endregion
    }

}
