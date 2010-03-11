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
        private List<Thread> threads = new List<Thread>();
        private int threadsToUse = 1;
        private object locker = new object();
        private Queue<AutoSyncRequest> jobs = new Queue<AutoSyncRequest>();
        private EventWaitHandle wh = new AutoResetEvent(false);
        private static SeamlessQueueControl _instance;

        private SeamlessQueueControl()
        {
            for (int i = 0; i < threadsToUse; i++)
            {
                Thread t = new Thread(work);
                threads.Add(t);
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

        public bool IsEmpty
        {
            get { return jobs.Count == 0; }
        }

        public void Dispose()
        {
            Console.WriteLine("LOL");
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
