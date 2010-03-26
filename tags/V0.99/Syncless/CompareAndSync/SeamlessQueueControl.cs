using System;
using System.Threading;
using System.Collections.Generic;
using Syncless.CompareAndSync.Request;

namespace Syncless.CompareAndSync
{
    /// <summary>
    /// Class for queuing auto-sync jobs. Based off http://moazzam-khan.com/blog/?p=418
    /// </summary>
    public class SeamlessQueueControl : IDisposable
    {
        private readonly List<Thread> _threads = new List<Thread>();
        private int threadsToUse = 1;
        private static readonly object locker = new object();
        private readonly Queue<AutoSyncRequest> _jobs = new Queue<AutoSyncRequest>();
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private static SeamlessQueueControl _instance;

        private SeamlessQueueControl()
        {
            for (int i = 0; i < threadsToUse; i++)
            {
                Thread t = new Thread(Work);
                _threads.Add(t);
                t.Start();
            }
        }

        public static SeamlessQueueControl Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SeamlessQueueControl();
                }
                return _instance;
            }
        }

        //public int ThreadsToUse
        //{
        //    set { threadsToUse = value; }
        //    get { return threadsToUse; }
        //}

        public void AddSyncJob(AutoSyncRequest item)
        {
            lock (locker)
            {
                _jobs.Enqueue(item);
            }
            _wh.Set();
        }

        private void Work()
        {
            while (true)
            {
                AutoSyncRequest item = null;
                lock (locker)
                {
                    if (_jobs.Count > 0)
                    {
                        item = _jobs.Dequeue();
                        if (item == null)
                            return;
                    }
                }
                if (item != null)
                {
                    SeamlessSyncer.Sync(item);
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

        public void Dispose()
        {
            for (int i = 0; i < _threads.Count; i++)
                AddSyncJob(null);
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Join();
        }
    }
}
