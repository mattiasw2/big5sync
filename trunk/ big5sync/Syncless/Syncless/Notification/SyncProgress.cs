using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class SyncProgress
    {
        private List<ISyncProgressObserver> _observerList;

        private int _totalJob;

        public int TotalJob
        {
            get { return _totalJob; }
        }
        private int _completedJob;
        public int CompletedJob
        {
            get { return _completedJob; }
        }
        private int _failedJob;

        public int FailedJob
        {
            get { return _failedJob; }
        }

        public double PercentComplete
        {
            get
            {
                double completed = _completedJob + _failedJob;
                double total = _totalJob;
                return completed / total * 100;
            }
        }

        public void complete()
        {
            if (_completedJob + _failedJob >= _totalJob)
            {
                return;
            }
            _completedJob++;
            foreach (ISyncProgressObserver obs in _observerList)
            {
                MethodASync sync = new MethodASync(obs.ProgressChanged);
                sync.BeginInvoke(null, null);
            }

        }
        public delegate void MethodASync();
        public void fail()
        {
            if (_completedJob + _failedJob >= _totalJob)
            {
                return;
            }
            _failedJob++;
            foreach (ISyncProgressObserver obs in _observerList)
            {
                MethodASync sync = new MethodASync(obs.ProgressChanged);
                sync.BeginInvoke(null, null);
            }

        }
        public SyncProgress(int total)
        {
            _totalJob = total;
            _failedJob = 0;
            _completedJob = 0;
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
    }
}
