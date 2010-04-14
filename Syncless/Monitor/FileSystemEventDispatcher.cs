/*
 * 
 * Author: Koh Cher Guan
 * 
 */

using System.Collections.Generic;
using System.Threading;
using Syncless.Monitor.DTO;

namespace Syncless.Monitor
{
    /// <summary>
    /// This class is the holding place for all the events from the <see cref="System.IO.FileSystemWatcher" /> and <see cref="Syncless.Monitor.ExtendedFileSystemWatcher" />.
    /// This will help to keep the code for handling an event fired from the 2 Watcher short, so as to prevent their internal buffer from overflowing.
    /// The events will then be dispatched to <see cref="Syncless.Monitor.FileSystemEventProcessor" /> to process after an idle time of 1000 milliseconds.
    /// Reference from http://csharp-codesamples.com/2009/02/file-system-watcher-and-large-file-volumes/
    /// </summary>
    public class FileSystemEventDispatcher
    {
        private const int IDLE_TIME = 1000; // idle time required to dispatch events out to process

        private static FileSystemEventDispatcher _instance;
        /// <summary>
        /// Get the instance of the <see cref="Syncless.Monitor.FileSystemEventDispatcher"/> Component
        /// </summary>
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

        /// <summary>
        /// Stop the thread of this component
        /// </summary>
        public void Terminate()
        {
            waitHandle.Close();
            if (dispatcherThread != null)
            {
                dispatcherThread.Abort();
            }
        }

        /// <summary>
        /// Enqueue event and wait to be dispatched.
        /// </summary>
        /// <param name="e">A <see cref="Syncless.Monitor.DTO.FileSystemEvent"/> object containing the information needed to handle a request.</param>
        public void Enqueue(FileSystemEvent e)
        {
            lock (queue)
            {
                queue.Enqueue(e);
            }
            if (dispatcherThread == null) // start the thread if not started
            {
                dispatcherThread = new Thread(DispatchEvent);
                dispatcherThread.Start();
            }
            else if (dispatcherThread.ThreadState == ThreadState.WaitSleepJoin) // wake the thread if sleeped
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
                while (!isIdle) // check if the queue is idle
                {
                    Thread.Sleep(IDLE_TIME);
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
                    FileSystemEventProcessor.Instance.Enqueue(eventList); // send for processing
                }
                else
                {
                    waitHandle.WaitOne();
                }
            }
        }
    }
}
