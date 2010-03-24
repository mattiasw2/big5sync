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
        private List<Thread> threads = new List<Thread>();
        private int threadsToUse = 1;
        private object locker = new object();
        private Queue<ManualSyncRequest> jobs = new Queue<ManualSyncRequest>();
        private EventWaitHandle wh = new AutoResetEvent(false);
        private static ManualQueueControl _instance;

        private string _currJobName;
        private HashSet<string> _queuedJobs = new HashSet<string>();

        private ManualQueueControl()
        {
            for (int i = 0; i < threadsToUse; i++)
            {
                Thread t = new Thread(work);
                threads.Add(t);
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

        public string CurrJob
        {
            get { return _currJobName; }
        }

        public void AddSyncJob(ManualSyncRequest item)
        {
            lock (locker)
            {
                jobs.Enqueue(item);
                _queuedJobs.Add(item.TagName);
            }
            wh.Set();
        }

        private void work()
        {
            while (true)
            {
                ManualSyncRequest item = null;
                lock (locker)
                {
                    if (jobs.Count > 0)
                    {
                        item = jobs.Dequeue();
                        _queuedJobs.Remove(item.TagName);
                        _currJobName = item.TagName;

                        if (item == null)
                            return;
                    }
                }
                if (item != null)
                {
                    ManualSyncer.Sync(item);
                    _currJobName = null;
                }
                else
                {
                    wh.WaitOne();
                }
            }
        }

        public bool IsEmpty
        {
            get { return jobs.Count == 0; }
        }

        public bool IsQueued(string tagName)
        {
            if (string.IsNullOrEmpty(_currJobName))
                return false;

            return _queuedJobs.Contains(tagName);
        }

        public bool IsSyncing(string tagName)
        {
            return tagName.Equals(_currJobName);
        }

        public bool IsQueuedOrSyncing(string tagName)
        {
            return IsQueued(tagName) || IsSyncing(tagName);
        }

        public void Dispose()
        {
            jobs.Clear();
            for (int i = 0; i < threads.Count; i++)
            {
                AddSyncJob(null);
            }
            for (int i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
        }
    }
}
