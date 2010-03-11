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
        private List<Thread> threads;
        private int threadsToUse;
        private object locker;
        private Queue<AutoSyncRequest> jobs;
        private EventWaitHandle wh;
        private static SeamlessQueueControl _instance;

        private SeamlessQueueControl()
        {
            threads = new List<Thread>();
            threadsToUse = 1;
            locker = new object();
            jobs = new Queue<AutoSyncRequest>();
            wh = new AutoResetEvent(false);
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

        public int ThreadsToUse
        {
            set { threadsToUse = value; }
            get { return threadsToUse; }
        }

        private void AutoSyncJobQueue()
        {
            for (int i = 0; i < threadsToUse; i++)
            {
                Thread t = new Thread(work);
                threads.Add(t);
                t.Start();
            }
        }

        public void AddSyncJob(AutoSyncRequest item)
        {
            lock (locker)
            {
                jobs.Enqueue(item);
            }
            wh.Set();
        }

        private void work()
        {
            while (true)
            {
                AutoSyncRequest item = null;
                lock (locker)
                {
                    if (jobs.Count > 0)
                    {
                        item = jobs.Dequeue();
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
                    wh.WaitOne();
                }
            }
        }

        public void Dispose()
        {
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
