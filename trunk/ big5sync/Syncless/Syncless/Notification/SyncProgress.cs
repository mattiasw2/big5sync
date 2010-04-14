/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System.Collections.Generic;
using System.Threading;
using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// Sync Progress object representing the Sync Progress
    /// </summary>
    public class SyncProgress : Progress
    {
        /// <summary>
        /// List of observer
        /// </summary>
        private List<ISyncProgressObserver> _observerList;
        private bool _notifyProgressChange;
        private bool _notifyStateChange;
        private bool _completed;
        /// <summary>
        /// the thread incharge of notifying the observer of the change.
        /// </summary>
        private Thread _worker;
        
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        
        #region Sync
        /// <summary>
        /// Totaly Sync Job Count
        /// </summary>
        public int SyncJobTotal { get; private set; }
        /// <summary>
        /// Current Compeleted Sync Job
        /// </summary>
        public int SyncCompletedJobs { get; private set; }
        /// <summary>
        /// Current Fail Sync Job
        /// </summary>
        public int SyncFailedJobs { get; private set; }

        #endregion

        #region Finalising
        /// <summary>
        /// Total Finalising Job Count
        /// </summary>
        public int FinalisingJobTotal { get; set; }
        /// <summary>
        /// Current Completed Finalising Job
        /// </summary>
        public int FinalisingCompletedJobs { get; set; }
        /// <summary>
        /// Current Fail Finalising Job
        /// </summary>
        public int FinalisingFailedJobs { get; set; }

        #endregion

        /// <summary>
        /// Get the current percentage compelete
        ///  If the <see cref="SyncState"/> is Analyzing, it always return 0
        ///  If the <see cref="SyncState"/> is Synchronzing , it return the number of (completed sync job + fail sync job )/ total job * 100.0;
        ///  If the <see cref="SyncState"/> is Finalising, it return the number of (completed finalising job  + fail finalising job) / total finalising job * 100.0;
        ///  If the <see cref="SyncState"/> is Finished, it always return 100.0
        /// </summary>
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
                    double completed = SyncCompletedJobs + SyncFailedJobs;
                    double total = SyncJobTotal;
                    return completed / total * 100.0;
                }
                if (State == SyncState.Finalizing)
                {
                    double completed = FinalisingCompletedJobs;
                    double total = FinalisingJobTotal;
                    return completed / total * 100.0;
                }
                if (State == SyncState.Finished)
                {
                    return 100;
                }
                return 0;
            }
        }
        /// <summary>
        /// Inform the Sync Progress that a Job is Completed
        /// </summary>
        public override void Complete()
        {
            switch (State)
            {
                case SyncState.Synchronizing:
                    if (SyncCompletedJobs + SyncFailedJobs >= SyncJobTotal)
                    {
                        return;
                    }
                    SyncCompletedJobs++;
                    break;
                case SyncState.Finalizing:
                    if (FinalisingCompletedJobs + FinalisingFailedJobs >= FinalisingJobTotal)
                    {
                        return;
                    }
                    FinalisingCompletedJobs++;
                    break;
                default: return;
            }
            InvokeChange();
        }
        /// <summary>
        /// Inform the Sync Progress that a Job failed
        /// </summary>
        public override void Fail()
        {
            switch (State)
            {
                case SyncState.Synchronizing:
                    if (SyncCompletedJobs + SyncFailedJobs >= SyncJobTotal)
                    {
                        return;
                    }
                    SyncFailedJobs++;
                    break;
                case SyncState.Finalizing:
                    if (FinalisingCompletedJobs + FinalisingFailedJobs >= FinalisingJobTotal)
                    {
                        return;
                    }
                    FinalisingFailedJobs++;
                    break;
                default: return;
            }
            InvokeChange();

        }
        /// <summary>
        /// Cancel the Sync Progress
        /// </summary>
        public override void Cancel()
        {
            State = SyncState.Cancelled;
            if (_worker != null)
            {
                _wh.Set();
            }
        }
        /// <summary>
        /// Inform all the observer that progress is updated
        /// </summary>
        public override void Update()
        {
            InvokeChange();
        }
        /// <summary>
        /// Inform all the observer that progress is updated
        /// </summary>
        private void InvokeChange()
        {
            if (!_notifyProgressChange)
            {
                _notifyProgressChange = true;
                //_worker.Interrupt();
                _wh.Set();
            }
        }
        /// <summary>
        /// Initialize a SyncProgress
        /// </summary>
        /// <param name="tagName">name of the <see cref="Tag"/></param>
        public SyncProgress(string tagName):base(tagName)
        {
            
            SyncJobTotal = 0;
            SyncFailedJobs = 0;
            SyncCompletedJobs = 0;
            _observerList = new List<ISyncProgressObserver>();
            _notifyProgressChange = false;
            _notifyStateChange = false;
            _completed = false;
            _worker = new Thread(Notifier);
            _worker.Start();
        }

        /// <summary>
        /// Register a observer to the progress
        /// </summary>
        /// <param name="obs">The observer</param>
        public void AddObserver(ISyncProgressObserver obs)
        {
            if (_observerList.Contains(obs))
            {
                return;
            }
            _observerList.Add(obs);
        }
        /// <summary>
        /// Unregister a observer to the progress
        /// </summary>
        /// <param name="obs">The observer</param>
        public void RemoveObserver(ISyncProgressObserver obs)
        {
            if (_observerList.Contains(obs))
            {
                _observerList.Remove(obs);
            }
        }
        /// <summary>
        /// Change the <see cref="SyncState"/> to Analyzing
        /// </summary>
        /// <returns>true if the state can be changed, otherwise false</returns>
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
        /// <summary>
        /// Change the <see cref="SyncState"/> to Synchronzing
        /// </summary>
        /// <param name="jobcount">total number of job</param>
        /// <returns>true if the state can be changed, otherwise false</returns>
        public bool ChangeToSyncing(int jobcount)
        {
            SyncJobTotal = jobcount;
            if (State != SyncState.Analyzing)
            {
                return false;
            }
            State = SyncState.Synchronizing;

            TriggerStateChanged();

            _wh.Set();
            return true;
        }
        /// <summary>
        /// Change the <see cref="SyncState"/> to Finalising
        /// </summary>
        /// <param name="jobcount">total number of job</param>
        /// <returns>true if the state can be changed, otherwise false</returns>
        public bool ChangeToFinalizing(int jobcount)
        {
            FinalisingJobTotal = jobcount;
            if (State != SyncState.Synchronizing)
            {
                return false;
            }

            State = SyncState.Finalizing;

            TriggerStateChanged();

            _wh.Set();
            return true;
        }
        /// <summary>
        /// Change the <see cref="SyncState"/> to Finished
        /// </summary>
        /// <returns>true if the state can be changed, otherwise false</returns>
        public bool ChangeToFinished()
        {

            if (State != SyncState.Finalizing)
            {
                return false;
            }

            _completed = true;
            State = SyncState.Finished;
            TriggerSyncComplete();

            return true;
        }
        /// <summary>
        /// Trigger Sync complete (inform the observer)
        /// </summary>
        private void TriggerSyncComplete()
        {
            if (!_notifyStateChange)
            {
                _notifyStateChange = true;
            }
            _wh.Set();
        }
        /// <summary>
        /// Trigger State change (inform the observer)
        /// </summary>
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
                if (_notifyProgressChange)
                {
                    _notifyProgressChange = false;
                    foreach (ISyncProgressObserver obs in _observerList)
                    {
                        obs.ProgressChanged();
                    }
                    continue;
                }
                if (_notifyStateChange)
                {
                    _notifyStateChange = false;
                    foreach (ISyncProgressObserver obs in _observerList)
                    {
                        obs.InvokeStateChanged();
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
