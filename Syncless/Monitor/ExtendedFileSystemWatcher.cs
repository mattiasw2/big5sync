using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Syncless.Monitor
{
    /// <summary>
    /// Reference from http://geekswithblogs.net/thibbard/articles/ExtendingFileSystemWatcher.aspx
    /// </summary>
    public class ExtendedFileSystemWatcher : FileSystemWatcher
    {
        private const int SLEEP_TIME = 1000;

        private List<FileSystemEventArgs> inQueue;
        private Thread watcherThread;
        private bool runWatcher;
        
        public event FileSystemEventHandler CreateComplete;

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

        public FileSystemEventArgs[] FileInQueue
        {
            get
            {
                try
                {
                    FileSystemEventArgs[] rv = new FileSystemEventArgs[inQueue.Count];
                    inQueue.CopyTo(rv);
                    return rv;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public ExtendedFileSystemWatcher()
            : base()
        {
            inQueue = new List<FileSystemEventArgs>();
            runWatcher = false;
            Created += new FileSystemEventHandler(ExtendedFileSystemWatcher_Created);
        }

        public ExtendedFileSystemWatcher(string path)
            : base(path)
        {
            inQueue = new List<FileSystemEventArgs>();
            runWatcher = false;
            Created += new FileSystemEventHandler(ExtendedFileSystemWatcher_Created);
        }

        public ExtendedFileSystemWatcher(string path, string filter)
            : base(path, filter)
        {
            inQueue = new List<FileSystemEventArgs>();
            runWatcher = false;
            Created += new FileSystemEventHandler(ExtendedFileSystemWatcher_Created);
        }

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
                            if (File.Exists(e.FullPath))
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
