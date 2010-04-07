using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Syncless.CompareAndSync;
using Syncless.Core;
using Syncless.Filters;
using Syncless.Helper;
using Syncless.Monitor.DTO;
using Syncless.Monitor.Exceptions;
namespace Syncless.Monitor
{
    /// <summary>
    /// The Monitor Component
    /// </summary>
    public class MonitorLayer
    {

        private const int BUFFER_SIZE = 65536; // internal buffer size for FileSystemWatcher
        private static MonitorLayer _instance; // singleton instance
        /// <summary>
        /// Get the instance of the Monitor Component
        /// </summary>
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

        // watchers for path
        private List<ExtendedFileSystemWatcher> watchers;
        private List<string> monitoredPaths;

        // root watchers for the path being monitored
        private List<FileSystemWatcher> rootWatchers;
        private Dictionary<string, List<string>> rootsAndParent;

        // filters to filter syncless related folder
        private FilterChain filtering;
        private List<Filter> filterList;
        
        private MonitorLayer()
        {
            watchers = new List<ExtendedFileSystemWatcher>();
            monitoredPaths = new List<string>();
            rootWatchers = new List<FileSystemWatcher>();
            rootsAndParent = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            filtering = new FilterChain();
            filterList = new List<Filter>();
            filterList.Add(FilterFactory.CreateArchiveFilter(SyncConfig.Instance.ArchiveName)); // filter of syncless archive folder
            filterList.Add(FilterFactory.CreateArchiveFilter(SyncConfig.Instance.ConflictDir)); // filter of syncless conflict folder
        }

        /// <summary>
        /// Stop all thread activities related to the Monitor Component
        /// </summary>
        public void Terminate()
        {
            FileSystemEventDispatcher.Instance.Terminate();
            FileSystemEventProcessor.Instance.Terminate();
            FileSystemEventExecutor.Instance.Terminate();
        }

        /// <summary>
        /// Start monitoring a path. If the path is already monitored, return false.
        ///    001:/Lectures
        ///    001:/Lectures/lecture1.pdf require only 1 monitor (*important) 
        ///      however, if 001:/Lectures is unmonitored, 001:/Lectures/lecture1.pdf have to be monitored. 
        ///      this is to ensure that if a change is made to lecture1.pdf , it will not be notified twice.
        /// </summary>
        /// <param name="path">A <see cref="string"/> specifying the path to be monitored.</param>
        /// <returns><see cref="bool"/> stating if a path will be monitored</returns>
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

        // Monitor A Directory
        private bool MonitorDirectory(string path)
        {
            bool addToWatcher = true;
            for (int i = 0; i < watchers.Count; i++)
            {
                ExtendedFileSystemWatcher watcher = watchers[i];
                string watchPath = watcher.Path.ToLower();
                if (watchPath.Equals(path.ToLower())) // if duplicate directory found
                {
                    return false;
                }
                else if (watchPath.StartsWith(path.ToLower())) // if adding a parent directory or adding another directory to the same directory
                {
                    DirectoryInfo newDirectory = new DirectoryInfo(path);
                    DirectoryInfo existingDirectory = new DirectoryInfo(watchPath);
                    if (!newDirectory.Parent.FullName.ToLower().Equals(existingDirectory.Parent.FullName.ToLower())) // if adding a parent directory
                    {
                        watcher.Dispose();      // stop monitoring
                        watchers.RemoveAt(i);
                        i--;
                    }
                }
                else if (path.ToLower().StartsWith(watchPath)) // if adding a child directory or adding another directory to the same directory
                {
                    DirectoryInfo newDirectory = new DirectoryInfo(path);
                    DirectoryInfo existingDirectory = new DirectoryInfo(watchPath);
                    if (!newDirectory.Parent.FullName.ToLower().Equals(existingDirectory.Parent.FullName.ToLower())) // if adding a child directory
                    {
                        addToWatcher = false; // do not add a watcher
                    }
                }
            }
            if (addToWatcher)
            {
                ExtendedFileSystemWatcher watcher = CreateWatcher(path, "*.*");
                watchers.Add(watcher);
                MonitorRootDirectory(path); // attemp to add a root watcher to watch the path being monitored
            }
            foreach (string mPath in monitoredPaths)
            {
                if (mPath.ToLower().Equals(path.ToLower()))
                {
                    return false; // already monitoring
                }
            }
            monitoredPaths.Add(path);
            return true; // monitoring successful
        }

        // Monitor the root path
        private void MonitorRootDirectory(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            if(directory.Root.FullName.ToLower().Equals(path.ToLower())) // if the root of the drive is assigned to be monitored
            {
                for (int i = 0; i < rootWatchers.Count; i++) // remove all existing root watcher monitoring the child of the drive
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
                if (rootWatchPath.Equals(path.ToLower())) // if a previous root path is now being monitored
                {
                    rootWatcher.Dispose(); // stop monitoring
                    rootWatchers.RemoveAt(i);
                    transferedPath = rootsAndParent[rootWatcher.Path]; // transfer all the monitored path to the new root watcher
                    rootsAndParent.Remove(rootWatcher.Path);
                }
            }

            bool noRootWatcher = true;
            string parent = directory.Parent.FullName;
            foreach (KeyValuePair<string, List<string>> kvp in rootsAndParent)
            {
                if (kvp.Key.ToLower().Equals(parent.ToLower())) // if has same root path
                {
                    kvp.Value.Add(path); // add the path to the list to be monitored
                    noRootWatcher = false; // do not add root watcher
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
                    folders.AddRange(transferedPath); // transfering the removed monitored path to the new root watcher
                }
                folders.Add(path);
                rootsAndParent.Add(parent, folders);
            }
        }

        /// <summary>
        /// Unmonitor a path. If the Path does not exist, return false.
        ///    if 001:/Lectures is monitored, and i try to unmonitor 001:/Lectures/lecture1.pdf , it should fail.
        ///    if 
        ///     001:/Lectures and 001:/Lectures/lecture1.pdf is being monitored , if i remove 001/Lectures/lecture1.pdf, the next time i remove 001:/Lectures, then 001:/Lectures/lecture1.pdf should not be monitored.
        /// </summary>
        /// <param name="path">A <see cref="string"/> specifying the path to be unmonitored</param>
        /// <returns><see cref="bool"/> stating if the monitor can be stopped.</returns>
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

        // Stop Monitoring a Directory
        private bool UnMonitorDirectory(string path)
        {
            bool isMonitored = false;
            for (int i = 0; i < monitoredPaths.Count; i++) // check if the path is being monitored
            {
                if (monitoredPaths[i].ToLower().Equals(path.ToLower()))
                {
                    monitoredPaths.RemoveAt(i);
                    isMonitored = true;
                    break;
                }
            }
            if (!isMonitored) // if not being monitored
            {
                return false;
            }
            bool unMonitored = false;
            for (int i = 0; i < watchers.Count; i++) // check if watcher is assigned
            {
                ExtendedFileSystemWatcher watcher = watchers[i];
                string watchPath = watcher.Path.ToLower();
                if (watchPath.Equals(path.ToLower())) // if watcher is assigned
                {
                    watcher.Dispose(); // stop monitoring
                    watchers.RemoveAt(i);
                    UnMonitorRootDirectory(path); // attempt to stop monitoring the root path of this path
                    unMonitored = true;
                    break;
                }
            }
            if (!unMonitored)
            {
                return true;
            }
            foreach (string mPath in monitoredPaths) // start monitoring the child path of this path since it has stopped monitoring
            {
                if (mPath.ToLower().StartsWith(path.ToLower()))
                {
                    MonitorPath(mPath);
                }
            }
            return true;
        }

        // Stop monitoring root watcher
        private void UnMonitorRootDirectory(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            string parent = directory.Parent.FullName;
            bool remove = false;
            foreach (KeyValuePair<string, List<string>> kvp in rootsAndParent) // check if the root path is being monitored
            {
                if (kvp.Key.ToLower().Equals(parent)) // if is monitored
                {
                    List<string> folders = kvp.Value;
                    if (folders.Count == 1) // if root path is for only one path, then remove the whole root path
                    {
                        remove = true;
                    }
                    else // else, remove the unmonitored path from its root path
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
        /// <param name="driveLetter">A <see cref="string"/> specifying the drive letter (i.e 'C') </param>
        /// <returns>An <see cref="int"/> specifying the number of paths unmonitored</returns>
        public int UnMonitorDrive(string driveLetter)
        {
            for (int i = 0; i < watchers.Count; i++) // remove all watcher with path under the specified drive
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
            for (int i = 0; i < monitoredPaths.Count; i++) // remove all path under the specified drive
            {
                if (monitoredPaths[i].ToLower().StartsWith(driveLetter.ToLower()))
                {
                    monitoredPaths.RemoveAt(i);
                    i--;
                    count++;
                }
            }
            for (int i = 0; i < rootWatchers.Count; i++) // remove all root watcher with path under the specified drive
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

        // Builder for ExtendedFileSystemWatcher
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

        // execute when a modified event is fired
        private void OnModified(object source, FileSystemEventArgs e)
        {
            if (!filtering.ApplyFilter(filterList, e.FullPath))
            {
                return;
            }
            if (File.Exists(e.FullPath))
            {
                FileSystemEvent fse = new FileSystemEvent(e.FullPath, EventChangeType.MODIFIED, FileSystemType.FILE);
                FileSystemEventDispatcher.Instance.Enqueue(fse);
            }
        }

        // execute when a created event is fired
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (!filtering.ApplyFilter(filterList, e.FullPath))
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

        // execute when a deleted event is fired
        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            if (!filtering.ApplyFilter(filterList, e.FullPath))
            {
                return;
            }
            ExtendedFileSystemWatcher watcher = (ExtendedFileSystemWatcher)source;
            FileSystemEvent fse = new FileSystemEvent(e.FullPath, watcher.Path);
            FileSystemEventDispatcher.Instance.Enqueue(fse);
        }

        // execute when a renamed event is fired
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (!filtering.ApplyFilter(filterList, e.OldFullPath))
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

        // execute when a create completed event is fired
        private void OnCreateComplete(object source, FileSystemEventArgs e)
        {
            if (!filtering.ApplyFilter(filterList, e.FullPath))
            {
                return;
            }
            FileSystemEvent fse = new FileSystemEvent(e.FullPath, EventChangeType.CREATED, FileSystemType.FILE);
            FileSystemEventDispatcher.Instance.Enqueue(fse);
        }

        // execute when a error event is fired
        private void OnError(object source, ErrorEventArgs e)
        {
            ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(e.GetException().ToString());
            //Console.WriteLine(e.GetException().ToString());
            
        }
        
        // Builder for FileSystemWatcher, the root watcher
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

        // execute when a deleted event for root watcher is fired
        private void OnRootDeleted(object source, FileSystemEventArgs e)
        {
            if (!filtering.ApplyFilter(filterList, e.FullPath))
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

        // execute when a renamed event for root watcher is fired
        private void OnRootRenamed(object source, RenamedEventArgs e)
        {
            if (!filtering.ApplyFilter(filterList, e.OldFullPath))
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

        // execute when a error event for root watcher is fired
        private void OnRootError(object source, ErrorEventArgs e)
        {
            ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(e.GetException().ToString());
            //Console.WriteLine(e.GetException().ToString());
        }
    }
}
