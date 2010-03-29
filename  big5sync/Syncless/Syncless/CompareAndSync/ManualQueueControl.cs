using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Syncless.CompareAndSync.Request;
using Syncless.Core;
using Syncless.Notification;
using Syncless.Notification.SLLNotification;
using Syncless.Notification.UINotification;

namespace Syncless.CompareAndSync
{
    /// <summary>
    /// Class for queuing manual sync jobs. Based off http://moazzam-khan.com/blog/?p=418
    /// </summary>
    public class ManualQueueControl : IDisposable
    {
        private readonly List<Thread> _threads = new List<Thread>();
        private const int threadsToUse = 1;
        private static readonly object locker = new object();
        private readonly List<ManualSyncRequest> _jobs = new List<ManualSyncRequest>();
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private static ManualQueueControl _instance;
        private readonly HashSet<string> _queuedJobsLookup = new HashSet<string>();
        private ManualSyncRequest _currJob;
        private SyncProgress _currJobProgress;

        private ManualQueueControl()
        {
            for (int i = 0; i < threadsToUse; i++)
            {
                Thread t = new Thread(Work);
                _threads.Add(t);
                t.Start();
            }
        }

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

        //public int ThreadsToUse
        //{
        //    set { threadsToUse = value; }
        //    get { return threadsToUse; }
        //}

        public void AddSyncJob(ManualSyncRequest item)
        {
            Debug.Assert(item != null);
            lock (locker)
            {
                if (!_queuedJobsLookup.Contains(item.TagName))
                {
                    _jobs.Insert(0, item);
                    _queuedJobsLookup.Add(item.TagName);
                }
            }
            _wh.Set();
        }


        //Change to bool instead?
        public void CancelSyncJob(CancelSyncRequest item)
        {
            lock (locker)
            {
                if (_queuedJobsLookup.Contains(item.TagName))
                {
                    for (int i = 0; i < _jobs.Count; i++)
                    {
                        if (_jobs[i].TagName == item.TagName)
                        {
                            _jobs.RemoveAt(i);
                            _queuedJobsLookup.Remove(item.TagName);
                            ServiceLocator.UINotificationQueue().Enqueue(new CancelSyncNotification(item.TagName, true));
                            break;
                        }
                    }
                }
                else if (_currJob!= null && _currJob.TagName == item.TagName)
                {
                    switch (_currJobProgress.State)
                    {
                        case SyncState.Started:
                        case SyncState.Analyzing:
                            _currJobProgress.State = SyncState.Cancelled;
                            ServiceLocator.UINotificationQueue().Enqueue(new CancelSyncNotification(item.TagName, true));
                            break;
                        default:
                            ServiceLocator.UINotificationQueue().Enqueue(new CancelSyncNotification(item.TagName, false));
                            break;
                    }
                }
            }
        }

        private void Work()
        {
            while (true)
            {
                lock (locker)
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
                        _currJobProgress.State = SyncState.Started;
                        ServiceLocator.UINotificationQueue().Enqueue(notification);
                        ManualSyncer.Sync(_currJob, _currJobProgress);

                        //Set both to null
                        _currJob = null;
                        _currJobProgress = null;
                    }
                }
                else
                {
                    _wh.WaitOne();
                }
            }
        }

        public bool IsEmpty
        {
            get { return _jobs.Count == 0; }
        }

        public bool IsQueued(string tagName)
        {
            return _queuedJobsLookup.Contains(tagName);
        }

        public bool IsSyncing(string tagName)
        {
            return _currJob == null ? false : tagName.Equals(_currJob.TagName);
        }

        public bool IsQueuedOrSyncing(string tagName)
        {
            return IsQueued(tagName) || IsSyncing(tagName);
        }

        public bool PrepareForTermination()
        {
            if (IsEmpty && _currJob == null)
                return true;
            return false;
        }

        public void Terminate()
        {
            if (IsEmpty && _currJob == null)
                Dispose();
            else
            {
                // Clear all jobs
                lock (locker)
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
                            _currJobProgress.State = SyncState.Cancelled;
                            break;
                    }
                }
                //while (_currJobProgress != null)
                //{
                //    //Busy waiting
                //}
                Dispose();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _threads.Count; i++)
            {
                lock (locker)
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
