/*
 * 
 * Author: Koh Cher Guan
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Syncless.Monitor
{
    /// <summary>
    /// An Extension to <see cref="System.IO.FileSystemWatcher" />.
    /// Added a CreateComplete event to inform the user when a file has completed creating.
    /// Reference from http://geekswithblogs.net/thibbard/articles/ExtendingFileSystemWatcher.aspx
    /// </summary>
    public class ExtendedFileSystemWatcher : FileSystemWatcher
    {
        private const int SLEEP_TIME = 1000; // sleep time for checking if a file complete creating

        private List<FileSystemEventArgs> inQueue;
        private Thread watcherThread;
        private bool runWatcher;
        
        /// <summary>
        /// Occurs when a file in the specified Path has completed creating.
        /// </summary>
        public event FileSystemEventHandler CreateComplete;

        /// <summary>
        /// Gets or sets a value indicating whether the component is enabled.
        /// </summary>
        public new bool EnableRaisingEvents
        {
            get
            {
                return base.EnableRaisingEvents;
            }
            set
            {
                if (value == true)
                {
                    runWatcher = true;
                    watcherThread = new Thread(WatchForCompletedFiles);
                    watcherThread.IsBackground = true;
                    watcherThread.Start();
                }
                else
                {
                    runWatcher = false;
                    watcherThread.Abort();
                }
                base.EnableRaisingEvents = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.ExtendedFileSystemWatcher" /> class.
        /// </summary>
        public ExtendedFileSystemWatcher()
            : base()
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.ExtendedFileSystemWatcher" /> class, given the specified directory to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <exception cref="System.ArgumentNullException">The path parameter is null.</exception>
        /// <exception cref="System.ArgumentException">The path parameter is an empty string (""). OR The path specified through the path parameter does not exist.</exception>
        public ExtendedFileSystemWatcher(string path)
            : base(path)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.ExtendedFileSystemWatcher" /> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, "*.txt" watches for changes to all text files.</param>
        /// <exception cref="System.ArgumentNullException">The path parameter is null.</exception>
        /// <exception cref="System.ArgumentException">The path parameter is an empty string (""). OR The path specified through the path parameter does not exist.</exception>
        public ExtendedFileSystemWatcher(string path, string filter)
            : base(path, filter)
        {
            Init();
        }

        // execute by the constructors to initialize this class
        private void Init()
        {
            inQueue = new List<FileSystemEventArgs>();
            runWatcher = false;
            Created += new FileSystemEventHandler(ExtendedFileSystemWatcher_Created);
        }

        // execute when a create event is fired
        // add FileSystemEventArgs to the internal queue
        private void ExtendedFileSystemWatcher_Created(Object source, FileSystemEventArgs e)
        {
            try
            {
                inQueue.Add(e);
            }
            catch (Exception ex)
            {
                OnError(new ErrorEventArgs(ex));
            }
        }

        // execute when the watcherThread start.
        // process the queue to check if a file has completed creating
        private void WatchForCompletedFiles()
        {
            while (runWatcher)
            {
                if (inQueue.Count > 0)
                {
                    FileSystemEventArgs e;
                    for (int i = 0; i < inQueue.Count; i++)
                    {
                        try
                        {
                            e = inQueue[i];
                            if (File.Exists(e.FullPath)) // Try opening a file. No exception thrown means it has completed creating.
                            {
                                FileStream fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.None);
                                fs.Close();
                            }
                            inQueue.RemoveAt(i);
                            CreateComplete(this, e);
                        }
                        catch (Exception)
                        { }
                    }
                }
                Thread.Sleep(SLEEP_TIME);
            }
        }
        
    }
}
