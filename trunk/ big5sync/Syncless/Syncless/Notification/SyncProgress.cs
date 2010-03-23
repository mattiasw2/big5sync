﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class SyncProgress
    {

        private List<ISyncProgressObserver> _observerList;
        private SyncState _state;
        public SyncState State
        {
            get { return _state; }
            set { _state = value; }
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
                if (_state == SyncState.ANALYZING)
                {
                    return 0;
                }
                else if (_state == SyncState.SYNCHRONIZING)
                {
                    double completed = _syncCompletedJobs + _syncFailedJobs;
                    double total = _syncjobtotal;
                    return completed / total * 100.0;
                }
                else if (_state == SyncState.FINALIZING)
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
                case SyncState.SYNCHRONIZING:
                    if (_syncCompletedJobs + _syncFailedJobs >= _syncjobtotal)
                    {
                        return;
                    }
                    _syncCompletedJobs++;
                    break;
                case SyncState.FINALIZING:
                    if (_finalisingCompletedJobs + _finalisingFailedJobs >= _finalisingJobTotal)
                    {
                        return;
                    }
                    _finalisingCompletedJobs++;
                    break;
                default : return;
            }

            foreach (ISyncProgressObserver obs in _observerList)
            {
                MethodASync sync = new MethodASync(obs.ProgressChanged);
                sync.BeginInvoke(null, null);
            }

        }
        public delegate void MethodASync();
        public void fail()
        {
            switch (_state)
            {
                case SyncState.SYNCHRONIZING:
                    if (_syncCompletedJobs + _syncFailedJobs >= _syncjobtotal)
                    {
                        return;
                    }
                    _syncFailedJobs++;
                    break;
                case SyncState.FINALIZING:
                    if (_finalisingCompletedJobs + _finalisingFailedJobs >= _finalisingJobTotal)
                    {
                        return;
                    }
                    _finalisingFailedJobs++;
                    break;
                default: return;
            }
            foreach (ISyncProgressObserver obs in _observerList)
            {
                MethodASync sync = new MethodASync(obs.ProgressChanged);
                sync.BeginInvoke(null, null);
            }

        }
        public SyncProgress()
        {
            _syncjobtotal = 0;
            _syncFailedJobs = 0;
            _syncCompletedJobs = 0;
            _observerList = new List<ISyncProgressObserver>();
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
                _observerList.Add(obs);
            }
        }

        public bool ChangeToAnalyzing()
        {
            if (_state != SyncState.STARTED)
            {
                return false;
            }
            _state = SyncState.ANALYZING;
            TriggerStateChanged();
            return true;
        }
        public bool ChangeToSyncing(int jobcount)
        {
            _syncjobtotal = jobcount;
            if (_state != SyncState.ANALYZING)
            {
                return false;
            }
            _state = SyncState.SYNCHRONIZING;
            TriggerStateChanged();
            return true;
        }
        public bool ChangeToFinalizing(int jobcount)
        {
            _syncjobtotal = jobcount;
            if (_state != SyncState.SYNCHRONIZING)
            {
                return false;
            }
            _state = SyncState.FINALIZING;
            TriggerStateChanged();
            return false;
        }

        private void TriggerStateChanged()
        {
            foreach (ISyncProgressObserver obs in _observerList)
            {
                MethodASync sync = new MethodASync(obs.StateChanged);
                sync.BeginInvoke(null, null);
            }
        }
    }

}