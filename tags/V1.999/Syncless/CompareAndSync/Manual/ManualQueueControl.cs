/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.CompareAndSync.Request;
using Syncless.Core;
using Syncless.Notification;

namespace Syncless.CompareAndSync.Manual
{
    /// <summary>
    /// Class for queuing manual sync jobs. Based off http://moazzam-khan.com/blog/?p=418
    /// </summary>
    public class ManualQueueControl : IDisposable
    {
        private readonly List<Thread> _threads = new List<Thread>();
        private const int ThreadsToUse = 1;
        private static readonly object Locker = new object();
        private readonly List<ManualSyncRequest> _jobs = new List<ManualSyncRequest>();
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private static ManualQueueControl _instance;
        private readonly HashSet<string> _queuedJobsLookup = new HashSet<string>();
        private ManualSyncRequest _currJob;
        private SyncProgress _currJobProgress;
        private HashSet<string> _isPendingCancel = new HashSet<string>();

        private ManualQueueControl()
        {
            for (int i = 0; i < ThreadsToUse; i++)
            {
                Thread t = new Thread(Work);
                _threads.Add(t);
                t.Start();
            }
        }

        /// <summary>
        /// Singleton pattern. Returns the current instance if not null, or a new instance if current instance is null.
        /// </summary>
        public static ManualQueueControl Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ManualQueueControl();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Adds a ManualSyncRequest to the queue.
        /// </summary>
        /// <param name="item">ManualSyncRequest</param>
        /// <exception cref="ArgumentNullException">Thrown when ManualSyncRequest is null</exception>
        public void AddSyncJob(ManualSyncRequest item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            lock (Locker)
            {
                if (!_queuedJobsLookup.Contains(item.TagName))
                {
                    _jobs.Add(item);
                    _queuedJobsLookup.Add(item.TagName);
                }
            }
            _wh.Set();
        }

        /// <summary>
        /// Attempts to cancel a synchronization job and returns true if it can be cancelled and false if it cannot be cancelled.
        /// </summary>
        /// <param name="item">CancelSyncRequest</param>
        /// <returns>True if the job can be cancelled, false if it cannot be cancelled</returns>
        public bool CancelSyncJob(CancelSyncRequest item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            lock (Locker)
            {
                if (_queuedJobsLookup.Contains(item.TagName))
                {
                    for (int i = 0; i < _jobs.Count; i++)
                    {
                        if (_jobs[i].TagName == item.TagName)
                        {
                            _jobs.RemoveAt(i);
                            _queuedJobsLookup.Remove(item.TagName);
                            ServiceLocator.UIPriorityQueue().Enqueue(new CancelSyncNotification(item.TagName, true));
                            return true;
                        }
                    }
                }
                else if (_currJob != null && _currJobProgress != null && _currJob.TagName == item.TagName)
                {
                    switch (_currJobProgress.State)
                    {
                        case SyncState.Started:
                        case SyncState.Analyzing:
                            _currJobProgress.Cancel();
                            _isPendingCancel.Add(_currJob.TagName);
                            return true;
                        default:
                            return false;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Worker thread to dequeue and process jobs in the queue.
        /// </summary>
        private void Work()
        {
            while (true)
            {
                lock (Locker)
                {
                    if (_jobs.Count > 0)
                    {
                        try
                        {
                            _currJob = _jobs[0];
                            _jobs.RemoveAt(0);

                            if (_currJob == null)
                                return;

                            _queuedJobsLookup.Remove(_currJob.TagName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
                if (_currJob != null)
                {
                    if (_currJob.Paths.Length < 2)
                    {
                        ServiceLocator.UINotificationQueue().Enqueue(new NothingToSyncNotification(_currJob.TagName));
                        if (_currJob.Notify)
                            ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(_currJob.TagName));
                        _currJobProgress = null;
                        _currJob = null;
                    }
                    else
                    {
                        SyncStartNotification notification = new SyncStartNotification(_currJob.TagName);
                        _currJobProgress = notification.Progress;
                        ServiceLocator.UINotificationQueue().Enqueue(notification);
                        RootCompareObject rco = ManualSyncer.Sync(_currJob, _currJobProgress);

                        //Set both to null
                        AbstractNotification endnotification = new SyncCompleteNotification(_currJob.TagName, rco);
                        if (_currJob != null)
                            _isPendingCancel.Remove(_currJob.TagName);
                        _currJob = null;
                        _currJobProgress = null;
                        ServiceLocator.UINotificationQueue().Enqueue(endnotification);


                    }
                }
                else
                {
                    _wh.WaitOne();
                }
            }
        }

        /// <summary>
        /// Gets a boolean indicating if the queue is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _jobs.Count == 0; }
        }

        /// <summary>
        /// Returns a boolean indicating if the tag name passed in is currently being queued.
        /// </summary>
        /// <param name="tagName">The tag name to check.</param>
        /// <returns>A boolean indicating if the tag name is being queued.</returns>
        public bool IsQueued(string tagName)
        {
            return _queuedJobsLookup.Contains(tagName);
        }

        /// <summary>
        /// Returns a boolean indicating if tag name passed in is being synced.
        /// </summary>
        /// <param name="tagName">The tag name to check.</param>
        /// <returns>A boolean indicating if the tag name is in progress.</returns>
        public bool IsSyncing(string tagName)
        {
            if (_isPendingCancel.Contains(tagName))
                return false;
            return _currJob == null ? false : tagName.Equals(_currJob.TagName);
        }

        /// <summary>
        /// Returns a boolean indicating if the tag name passed in is being queued or currently in progress.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns>A boolean indicating if the tag name passed in is being queued or currently in progress.</returns>
        public bool IsQueuedOrSyncing(string tagName)
        {
            return IsQueued(tagName) || IsSyncing(tagName);
        }

        /// <summary>
        /// Returns a boolean indicating if it is possible to terminate all jobs.
        /// </summary>
        /// <returns>A boolean indicating if it is possible to terminate all jobs.</returns>
        public bool PrepareForTermination()
        {
            if (IsEmpty && _currJob == null)
                return true;
            return false;
        }

        /// <summary>
        /// Clears the queue of all jobs, and attempts to cancel current job if possible.
        /// </summary>
        public void Terminate()
        {
            if (IsEmpty && _currJob == null)
                Dispose();
            else
            {
                lock (Locker)
                {
                    _jobs.Clear();
                    _queuedJobsLookup.Clear();
                }

                if (_currJobProgress != null && _currJob != null)
                {
                    switch (_currJobProgress.State)
                    {
                        case SyncState.Started:
                        case SyncState.Analyzing:
                            _currJobProgress.Cancel();
                            break;
                    }
                }
                Dispose();
            }
        }

        /// <summary>
        /// Adds a null value to the queue so that the worker thread will return.
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _threads.Count; i++)
            {
                lock (Locker)
                {
                    _jobs.Insert(0, null);
                }
                _wh.Set();
            }
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Join();
        }
    }
}