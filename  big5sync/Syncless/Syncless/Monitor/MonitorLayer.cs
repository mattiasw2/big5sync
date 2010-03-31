using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Syncless.Core;
using Syncless.Filters;
using Syncless.Helper;
using Syncless.Monitor.DTO;
using Syncless.Monitor.Exceptions;
namespace Syncless.Monitor
{
    public class MonitorLayer
    {

        private const int BUFFER_SIZE = 16384;
        private static MonitorLayer _instance;
        public static MonitorLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MonitorLayer();
                }
                return _instance;
            }
        }

        private List<ExtendedFileSystemWatcher> watchers;
        private List<string> monitoredPaths;
        private List<FileSystemWatcher> rootWatchers;
        private Dictionary<string, List<string>> rootsAndParent;
        private FilterChain configFilter;
        
        private MonitorLayer()
        {
            watchers = new List<ExtendedFileSystemWatcher>();
            monitoredPaths = new List<string>();
            rootWatchers = new List<FileSystemWatcher>();
            rootsAndParent = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            configFilter = new FilterChain();
        }

        public void Terminate()
        {
            FileSystemEventDispatcher.Instance.Terminate();
            FileSystemEventProcessor.Instance.Terminate();
            FileSystemEventExecutor.Instance.Terminate();
        }

        /// <summary>
        /// Start monitoring a path. If the path is already monitored, raise an exception.
        ///    001:/Lectures
        ///    001:/Lectures/lecture1.pdf require only 1 monitor (*important) 
        ///      however, if 001:/Lectures is unmonitored, 001:/Lectures/lecture1.pdf have to be monitored. 
        ///      this is to ensure that if a change is made to lecture1.pdf , it will not be notified twice.
        /// </summary>
        /// <param name="path">The Path to be monitored</param>
        /// <returns>Boolean stating if the monitor can be started</returns>
        /// <exception cref="Syncless.Monitor.Exception.MonitorPathNotFoundException">Throw when the path is not found.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool MonitorPath(string path)
        {
            if (Directory.Exists(path))
            {
                return MonitorDirectory(path);
            }
            else
            {
                throw new MonitorPathNotFoundException(ErrorMessage.PATH_NOT_FOUND, path);
            }
        }

        private bool MonitorDirectory(string path)
        {
            bool addToWatcher = true;
            for (int i = 0; i < watchers.Count; i++)
            {
                ExtendedFileSystemWatcher watcher = watchers[i];
                string watchPath = watcher.Path.ToLower();
                if (watchPath.Equals(path.ToLower())) // Duplicate directory
                {
                    return false;
                }
                else if (watchPath.StartsWith(path.ToLower())) // Adding a parent directory or Adding another directory to the same directory
                {
                    DirectoryInfo newDirectory = new DirectoryInfo(path);
                    DirectoryInfo existingDirectory = new DirectoryInfo(watchPath);
                    if (!newDirectory.Parent.FullName.ToLower().Equals(existingDirectory.Parent.FullName.ToLower())) // Adding a parent directory
                    {
                        watcher.Dispose();
                        watchers.RemoveAt(i);
                        i--;
                    }
                }
                else if (path.ToLower().StartsWith(watchPath)) // Adding a child directory or Adding another directory to the same directory
                {
                    DirectoryInfo newDirectory = new DirectoryInfo(path);
                    DirectoryInfo existingDirectory = new DirectoryInfo(watchPath);
                    if (!newDirectory.Parent.FullName.ToLower().Equals(existingDirectory.Parent.FullName.ToLower())) // Adding a child directory
                    {
                        addToWatcher = false;
                    }
                }
            }
            if (addToWatcher)
            {
                ExtendedFileSystemWatcher watcher = CreateWatcher(path, "*.*");
                watchers.Add(watcher);
                MonitorRootDirectory(path);
            }
            foreach (string mPath in monitoredPaths)
            {
                if (mPath.ToLower().Equals(path.ToLower()))
                {
                    return false;
                }
            }
            monitoredPaths.Add(path);
            return true;
        }

        private void MonitorRootDirectory(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            if(directory.Root.FullName.ToLower().Equals(path.ToLower())) // Root is excluded
            {
                for (int i = 0; i < rootWatchers.Count; i++)
                {
                    FileSystemWatcher rootWatcher = rootWatchers[i];
                    string rootWatchPath = rootWatcher.Path.ToLower();
                    if (rootWatchPath.StartsWith(path.ToLower()))
                    {
                        rootWatcher.Dispose();
                        rootWatchers.RemoveAt(i);
                    }
                }
                rootsAndParent.Remove(path);
                return ;
            }

            List<string> transferedPath = null;
            for (int i = 0; i < rootWatchers.Count; i++)
            {
                FileSystemWatcher rootWatcher = rootWatchers[i];
                string rootWatchPath = rootWatcher.Path.ToLower();
                if (rootWatchPath.Equals(path.ToLower())) // A parent directory is now being watched
                {
                    rootWatcher.Dispose();
                    rootWatchers.RemoveAt(i);
                    transferedPath = (List<string>)rootsAndParent[rootWatcher.Path];
                    rootsAndParent.Remove(rootWatcher.Path);
                }
            }

            bool noRootWatcher = true;
            string parent = directory.Parent.FullName;
            foreach (KeyValuePair<string, List<string>> kvp in rootsAndParent)
            {
                if (kvp.Key.ToLower().Equals(parent.ToLower())) // Same Root
                {
                    kvp.Value.Add(path);
                    noRootWatcher = false;
                    break;
                }
            }
            if (noRootWatcher)
            {
                FileSystemWatcher newRootWatcher = CreateRootWatcher(parent, "*.*");
                rootWatchers.Add(newRootWatcher);
                List<string> folders = new List<string>();
                if (transferedPath != null)
                {
                    folders.AddRange(transferedPath);
                }
                folders.Add(path);
                rootsAndParent.Add(parent, folders);
            }
        }

        /// <summary>
        /// Unmonitor a path. If the Path does not exist, raise an exception
        ///    if 001:/Lectures is monitored, and i try to unmonitor 001:/Lectures/lecture1.pdf , it should fail.
        ///    if 
        ///     001:/Lectures and 001:/Lectures/lecture1.pdf is being monitored , if i remove 001/Lectures/lecture1.pdf, the next time i remove 001:/Lectures, then 001:/Lectures/lecture1.pdf should not be monitored.
        /// </summary>
        /// <param name="path">The Path to be monitored</param>
        /// <returns>Boolean stating if the monitor can be stopped</returns>
        public bool UnMonitorPath(string path)
        {
            if (Directory.Exists(path))
            {
                return UnMonitorDirectory(path);
            }
            else
            {
                return false;
            }
        }

        private bool UnMonitorDirectory(string path)
        {
            bool isMonitored = false;
            for (int i = 0; i < monitoredPaths.Count; i++)
            {
                if (monitoredPaths[i].ToLower().Equals(path.ToLower()))
                {
                    monitoredPaths.RemoveAt(i);
                    isMonitored = true;
                    break;
                }
            }
            if (!isMonitored)
            {
                return false;
            }
            bool unMonitored = false;
            for (int i = 0; i < watchers.Count; i++)
            {
                ExtendedFileSystemWatcher watcher = watchers[i];
                string watchPath = watcher.Path.ToLower();
                if (watchPath.Equals(path.ToLower()))
                {
                    watcher.Dispose();
                    watchers.RemoveAt(i);
                    UnMonitorRootDirectory(path);
                    unMonitored = true;
                    break;
                }
            }
            if (!unMonitored)
            {
                return true;
            }
            foreach (string mPath in monitoredPaths)
            {
                if (mPath.ToLower().StartsWith(path.ToLower()))
                {
                    MonitorPath(mPath);
                }
            }
            return true;
        }

        private void UnMonitorRootDirectory(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            string parent = directory.Parent.FullName;
            bool remove = false;
            foreach (KeyValuePair<string, List<string>> kvp in rootsAndParent)
            {
                if (kvp.Key.ToLower().Equals(parent))
                {
                    List<string> folders = kvp.Value;
                    if (folders.Count == 1)
                    {
                        remove = true;
                    }
                    else
                    {
                        folders.Remove(path);
                        return;
                    }
                }
            }
            if (remove)
            {
                rootsAndParent.Remove(parent);
                for (int i = 0; i < rootWatchers.Count; i++)
                {
                    FileSystemWatcher rootWatcher = rootWatchers[i];
                    string rootWatchPath = rootWatcher.Path.ToLower();
                    if (rootWatchPath.Equals(parent))
                    {
                        rootWatcher.Dispose();
                        rootWatchers.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Unmonitor all files that is contained in a physical drive 
        /// </summary>
        /// <param name="driveLetter">The drive letter (i.e 'C') </param>
        /// <returns>The number of paths unmonitored</returns>
        /// <exception cref="Syncless.Monitor.Exception.MonitorDriveNotFoundException">Throw when the drive is not found.</exception>
        public int UnMonitorDrive(string driveLetter)
        {
            for (int i = 0; i < watchers.Count; i++)
            {
                ExtendedFileSystemWatcher watcher = watchers[i];
                if (watcher.Path.ToLower().StartsWith(driveLetter.ToLower()))
                {
                    watcher.Dispose();
                    watchers.RemoveAt(i);
                    i--;
                }
            }
            int count = 0;
            for (int i = 0; i < monitoredPaths.Count; i++)
            {
                if (monitoredPaths[i].ToLower().StartsWith(driveLetter.ToLower()))
                {
                    monitoredPaths.RemoveAt(i);
                    i--;
                    count++;
                }
            }
            for (int i = 0; i < rootWatchers.Count; i++)
            {
                if (rootWatchers[i].Path.ToLower().StartsWith(driveLetter.ToLower()))
                {
                    rootWatchers[i].Dispose();
                    rootsAndParent.Remove(rootWatchers[i].Path);
                    rootWatchers.RemoveAt(i);
                    i--;
                    
                }
            }
            return count;
        }

        private ExtendedFileSystemWatcher CreateWatcher(string path, string filter)
        {
            ExtendedFileSystemWatcher watcher = new ExtendedFileSystemWatcher(path, filter);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = true;
            watcher.InternalBufferSize = BUFFER_SIZE;
            watcher.Changed += new FileSystemEventHandler(OnModified);
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.CreateComplete += new FileSystemEventHandler(OnCreateComplete);
            watcher.Error += new ErrorEventHandler(OnError);
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private void OnModified(object source, FileSystemEventArgs e)
        {
            if (!configFilter.ApplyFilter(null, e.FullPath))
            {
                return;
            }
            if (File.Exists(e.FullPath))
            {
                FileSystemEvent fse = new FileSystemEvent(e.FullPath, EventChangeType.MODIFIED, FileSystemType.FILE);
                FileSystemEventDispatcher.Instance.Enqueue(fse);
            }
        }
        
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (!configFilter.ApplyFilter(null, e.FullPath))
            {
                return;
            }
            FileSystemEvent fse;
            if (File.Exists(e.FullPath))
            {
                fse = new FileSystemEvent(e.FullPath, EventChangeType.CREATING, FileSystemType.FILE);
            }
            else if (Directory.Exists(e.FullPath))
            {
                fse = new FileSystemEvent(e.FullPath, EventChangeType.CREATED, FileSystemType.FOLDER);
            }
            else
            {
                fse = new FileSystemEvent(e.FullPath, EventChangeType.CREATED, FileSystemType.UNKNOWN);
            }
            FileSystemEventDispatcher.Instance.Enqueue(fse);
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            if (!configFilter.ApplyFilter(null, e.FullPath))
            {
                return;
            }
            ExtendedFileSystemWatcher watcher = (ExtendedFileSystemWatcher)source;
            FileSystemEvent fse = new FileSystemEvent(e.FullPath, watcher.Path);
            FileSystemEventDispatcher.Instance.Enqueue(fse);
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (!configFilter.ApplyFilter(null, e.OldFullPath))
            {
                return;
            }
            FileSystemEvent fse;
            if (File.Exists(e.FullPath))
            {
                fse = new FileSystemEvent(e.OldFullPath, e.FullPath, FileSystemType.FILE);
            }
            else if (Directory.Exists(e.FullPath))
            {
                fse = new FileSystemEvent(e.OldFullPath, e.FullPath, FileSystemType.FOLDER);
            }
            else
            {
                fse = new FileSystemEvent(e.OldFullPath, e.FullPath, FileSystemType.UNKNOWN);
            }
            FileSystemEventDispatcher.Instance.Enqueue(fse);
        }

        private void OnCreateComplete(object source, FileSystemEventArgs e)
        {
            if (!configFilter.ApplyFilter(null, e.FullPath))
            {
                return;
            }
            FileSystemEvent fse = new FileSystemEvent(e.FullPath, EventChangeType.CREATED, FileSystemType.FILE);
            FileSystemEventDispatcher.Instance.Enqueue(fse);
        }

        private void OnError(object source, ErrorEventArgs e)
        {
            ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(e.GetException().ToString());
            //Console.WriteLine(e.GetException().ToString());
        }
        
        private FileSystemWatcher CreateRootWatcher(string path, string filter)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(path, filter);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = false;
            watcher.Deleted += new FileSystemEventHandler(OnRootDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRootRenamed);
            watcher.Error += new ErrorEventHandler(OnRootError);
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private void OnRootDeleted(object source, FileSystemEventArgs e)
        {
            if (!configFilter.ApplyFilter(null, e.FullPath))
            {
                return;
            }
            FileSystemWatcher watcher = (FileSystemWatcher)source;
            List<string> folders = rootsAndParent[watcher.Path];
            foreach (string folder in folders)
            {
                if (e.FullPath.Equals(folder))
                {
                    FileSystemEvent fse = new FileSystemEvent(e.FullPath, EventChangeType.DELETED, FileSystemType.FOLDER);
                    FileSystemEventDispatcher.Instance.Enqueue(fse);
                    return;
                }
            }
        }

        private void OnRootRenamed(object source, RenamedEventArgs e)
        {
            if (!configFilter.ApplyFilter(null, e.OldFullPath))
            {
                return;
            }
            FileSystemWatcher watcher = (FileSystemWatcher)source;
            List<string> folders = rootsAndParent[watcher.Path];
            foreach (string folder in folders)
            {
                if (e.OldFullPath.Equals(folder))
                {
                    FileSystemEvent fse = new FileSystemEvent(e.OldFullPath, e.FullPath, FileSystemType.FOLDER);
                    FileSystemEventDispatcher.Instance.Enqueue(fse);
                    return;
                }
            }
        }

        private void OnRootError(object source, ErrorEventArgs e)
        {
            ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(e.GetException().ToString());
            //Console.WriteLine(e.GetException().ToString());
        }
    }
}
