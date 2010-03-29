using System;
using System.Diagnostics;
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
        private AutoSyncRequest _currJob;

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
            Debug.Assert(locker != null);
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
                lock (locker)
                {
                    if (_jobs.Count > 0)
                    {
                        _currJob = _jobs.Dequeue();
                        if (_currJob == null)
                            return;
                    }
                }
                if (_currJob != null)
                {
                    SeamlessSyncer.Sync(_currJob);
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
                _jobs.Clear();

                while (_currJob != null)
                {
                    //Busy waiting
                }
                Dispose();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _threads.Count; i++)
            {
                lock (locker)
                {
                    _jobs.Enqueue(null);
                }
                _wh.Set();
            }
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Join();
        }
    }
}
