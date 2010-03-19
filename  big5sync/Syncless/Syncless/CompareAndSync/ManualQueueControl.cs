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
        private Queue<ManualRequest> jobs = new Queue<ManualRequest>();
        private EventWaitHandle wh = new AutoResetEvent(false);
        private static ManualQueueControl _instance;

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

        public void AddSyncJob(ManualRequest item)
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
                ManualRequest item = null;
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
                    if (item is ManualSyncRequest)
                        ManualSyncer.Sync(item as ManualSyncRequest);
                    else
                        ManualSyncer.Compare(item as ManualCompareRequest);
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
