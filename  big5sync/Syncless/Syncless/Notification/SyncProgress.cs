using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class SyncProgress
    {
        private List<Update> _eventHandlerList;
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
        public delegate void Update(double currentPercentComplete);
        public double PercentComplete
        {
            get
            {
                double completed = _completedJob;
                double total = _totalJob;
                return completed / total * 100;
            }
        }
        public SyncProgress(int total)
        {
            _totalJob = total;
            _failedJob = 0;
            _completedJob = 0;
            _eventHandlerList = new List<Update>();
        }
        public void complete()
        {
            if (_completedJob + _failedJob >= _totalJob)
            {
                return;
            }
            _completedJob++;
            foreach (Update handler in _eventHandlerList)
            {
                handler.BeginInvoke(PercentComplete,null, null);
            }

        }
        public void fail()
        {
            if (_completedJob + _failedJob >= _totalJob)
            {
                return;
            }
            _failedJob++;
            foreach (Update handler in _eventHandlerList)
            {
                handler.BeginInvoke(PercentComplete,null, null);
            }

        }
        public void AddChangeHandler(Update handler)
        {
            if (_eventHandlerList.Contains(handler))
            {
                return;
            }
            _eventHandlerList.Add(handler);
        }
        public void RemoveChangeHandler(Update handler)
        {
            if (_eventHandlerList.Contains(handler))
            {
                _eventHandlerList.Remove(handler);
            }
        }

    }
}
