using System;
using System.Collections.Generic;
using System.Threading;
using Syncless.Monitor.DTO;

namespace Syncless.Monitor
{
    /// <summary>
    /// Reference from http://csharp-codesamples.com/2009/02/file-system-watcher-and-large-file-volumes/
    /// </summary>
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

        private Queue<FileSystemEvent> queue;
        private Thread dispatcherThread;
        private EventWaitHandle waitHandle;

        private FileSystemEventDispatcher()
        {
            queue = new Queue<FileSystemEvent>();
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
                queue.Enqueue(e);
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

        private Queue<FileSystemEvent> BatchDequeue()
        {
            Queue<FileSystemEvent> eventList = null;
            lock (queue)
            {
                if (queue.Count != 0)
                {
                    eventList = new Queue<FileSystemEvent>(queue);
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
                Queue<FileSystemEvent> eventList = BatchDequeue();
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
