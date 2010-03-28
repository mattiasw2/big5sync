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
        private readonly Queue<ManualSyncRequest> _jobs = new Queue<ManualSyncRequest>();
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private static ManualQueueControl _instance;

        private string _currJobName;
        private readonly HashSet<string> _queuedJobs = new HashSet<string>();

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

        public string CurrJob
        {
            get { return _currJobName; }
        }

        public void AddSyncJob(ManualSyncRequest item)
        {
            lock (locker)
            {
                _jobs.Enqueue(item);
                if (item != null)
                    _queuedJobs.Add(item.TagName);
            }
            _wh.Set();
        }

        private void Work()
        {
            while (true)
            {
                ManualSyncRequest item = null;
                lock (locker)
                {
                    if (_jobs.Count > 0)
                    {
                        try
                        {
                            item = _jobs.Dequeue();

                            if (item == null)
                                return;

                            _queuedJobs.Remove(item.TagName);
                            _currJobName = item.TagName;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
                if (item != null)
                {
                    ManualSyncer.Sync(item);
                    _currJobName = null;
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
            for (int i = 0; i < _threads.Count; i++)
                AddSyncJob(null);
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Join();
        }
    }
}
