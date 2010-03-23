using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Notification;
namespace SynclessUI.Notification
{
    internal class SyncProgressWatcher : ISyncProgressObserver
    {
        #region ISyncProgressObserver Members
        private SyncProgress _progress;

        public SyncProgress Progress
        {
            get { return _progress; }
            set { _progress = value; }
        }

        public SyncProgressWatcher(SyncProgress p)
        {
            _progress = p;
            _progress.AddObserver(this);
        }
        public void StateChanged()
        {
            Console.WriteLine("State Changed (New State : " + _progress.State + ")");   
        }

        public void ProgressChanged()
        {
            Console.WriteLine("Current Percent : "+_progress.PercentComplete + "("+_progress.Message+")");
        }

        public void SyncComplete()
        {
            Console.WriteLine("Sync Complete");
        }

        #endregion
    }
}
