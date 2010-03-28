using System;
using System.Threading;
using System.Collections.Generic;
using Syncless.CompareAndSync.Request;

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
            lock (locker)
            {
                if (item == null || !_queuedJobsLookup.Contains(item.TagName))
                {
                    _jobs.Insert(0, item);
                    if (item != null)
                        _queuedJobsLookup.Add(item.TagName);
                }
            }
            _wh.Set();
        }

        public void CancelSyncJob(CancelSyncRequest item)
        {
            lock (locker)
            {
                if (!_queuedJobsLookup.Contains(item.TagName))
                {
                    for (int i = 0; i < _jobs.Count; i++)
                    {
                        if (_jobs[i].TagName == item.TagName)
                        {
                            _jobs.RemoveAt(i);
                            break;
                        }
                    }
                }
                else if (_currJob.TagName == item.TagName)
                {
                    _currJob.IsCancelled = true;
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
                    ManualSyncer.Sync(_currJob);
                    _currJob = null;
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
            if (_currJob == null || string.IsNullOrEmpty(_currJob.TagName))
                return false;

            return _queuedJobsLookup.Contains(tagName);
        }

        public bool IsSyncing(string tagName)
        {
            return _currJob == null? false : tagName.Equals(_currJob.TagName);
        }

        public bool IsQueuedOrSyncing(string tagName)
        {
            return IsQueued(tagName) || IsSyncing(tagName);
        }

        public void Dispose()
        {
            for (int i = 0; i < _threads.Count; i++)
                AddSyncJob(null);
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Join();
        }
    }
}
