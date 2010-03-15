using System;
using System.Collections.Generic;
using System.Threading;
using Syncless.Monitor.DTO;

namespace Syncless.Monitor
{
    public class FileSystemEventDispatcher
    {
        private const int SLEEP_TIME = 1000;

        private static FileSystemEventDispatcher _instance;
        public static FileSystemEventDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileSystemEventDispatcher();
                }
                return _instance;
            }
        }

        private List<FileSystemEvent> queue;
        private Thread dispatcherThread;
        private EventWaitHandle waitHandle;

        private FileSystemEventDispatcher()
        {
            queue = new List<FileSystemEvent>();
            waitHandle = new AutoResetEvent(true);
        }

        public void Terminate()
        {
            waitHandle.Close();
            if (dispatcherThread != null)
            {
                dispatcherThread.Abort();
            }
        }

        public void Enqueue(FileSystemEvent e)
        {
            lock (queue)
            {
                queue.Add(e);
            }
            if (dispatcherThread == null)
            {
                dispatcherThread = new Thread(DispatchEvent);
                dispatcherThread.Start();
            }
            else if (dispatcherThread.ThreadState == ThreadState.WaitSleepJoin)
            {
                waitHandle.Set();
            }
        }

        private List<FileSystemEvent> BatchDequeue()
        {
            List<FileSystemEvent> eventList = null;
            lock (queue)
            {
                if (queue.Count != 0)
                {
                    eventList = new List<FileSystemEvent>(queue);
                    queue.Clear();
                }
            }
            return eventList;
        }

        private void DispatchEvent()
        {
            while (true)
            {
                int count = queue.Count;
                bool isIdle = false;
                while (!isIdle)
                {
                    Thread.Sleep(SLEEP_TIME);
                    if (count == queue.Count)
                    {
                        isIdle = true;
                    }
                    else
                    {
                        count = queue.Count;
                    }
                }
                List<FileSystemEvent> eventList = BatchDequeue();
                if (eventList != null)
                {
                    FileSystemEventProcessor.Instance.Enqueue(eventList);
                }
                else
                {
                    waitHandle.WaitOne();
                }
            }
        }

        
    }
}
