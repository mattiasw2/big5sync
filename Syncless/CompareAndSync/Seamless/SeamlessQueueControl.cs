using System;
using System.Collections.Generic;
using System.Threading;
using Syncless.CompareAndSync.Request;

namespace Syncless.CompareAndSync.Seamless
{
    /// <summary>
    /// Class for queuing auto-sync jobs. Based off http://moazzam-khan.com/blog/?p=418
    /// </summary>
    public class SeamlessQueueControl : IDisposable
    {
        private readonly List<Thread> _threads = new List<Thread>();
        private const int ThreadsToUse = 1;
        private static readonly object Locker = new object();
        private readonly Queue<AutoSyncRequest> _jobs = new Queue<AutoSyncRequest>();
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private static SeamlessQueueControl _instance;
        private AutoSyncRequest _currJob;

        private SeamlessQueueControl()
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

        /// <summary>
        /// Adds a AutoSyncRequest to the queue.
        /// </summary>
        /// <param name="item">AutoSyncRequest</param>
        /// <exception cref="ArgumentNullException">Thrown when AutoSyncRequest is null</exception>
        public void AddSyncJob(AutoSyncRequest item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            lock (Locker)
            {
                _jobs.Enqueue(item);
            }
            _wh.Set();
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

        // <summary>
        /// Gets a boolean indicating if the queue is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _jobs.Count == 0; }
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
                    _jobs.Enqueue(null);
                }
                _wh.Set();
            }
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Join();
        }
    }
}