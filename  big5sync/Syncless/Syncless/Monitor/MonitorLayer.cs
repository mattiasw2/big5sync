using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Syncless.Core;
using Syncless.Helper;
using Syncless.Monitor.Exceptions;
namespace Syncless.Monitor
{
    public class MonitorLayer
    {
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

        private List<FileSystemWatcher> watchers;
        private List<string> monitoredPaths;
        // new code
        //private List<FileSystemWatcher> rootWatcher;
        //private Hashtable rootAndParentPair; 

        private MonitorLayer()
        {
            watchers = new List<FileSystemWatcher>();
            monitoredPaths = new List<string>();
            // new code
            //rootWatcher = new List<FileSystemWatcher>();
            //rootAndParentPair = new Hashtable();
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
        /// <exception cref="Syncless.Monitor.MonitorPathNotFoundException">Throw when the path is not found.</exception>
        public bool MonitorPath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new MonitorPathNotFoundException(ErrorMessage.PATH_NOT_FOUND, path);
            }
            if (File.Exists(path))
            {
                return MonitorFile(path);
            }
            else
            {
                return MonitorDirectory(path);
            }
        }

        private bool MonitorDirectory(string path)
        {
            bool addToWatcher = true;
            for (int i = 0; i < watchers.Count; i++)
            {
                FileSystemWatcher watcher = watchers[i];
                string watchPath = watcher.Path.ToLower();
                if (!watcher.Filter.Equals("*.*"))
                {
                    watchPath = watchPath + "\\" + watcher.Filter.ToLower();
                }
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
                FileSystemWatcher watcher = CreateWatcher(path, "*.*");
                watchers.Add(watcher);
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

        private bool MonitorFile(string path)
        {
            FileInfo file = new FileInfo(path);
            string pathDirectory = file.DirectoryName.ToLower();
            bool addToWatcher = true;
            for (int i = 0; i < watchers.Count; i++)
            {
                FileSystemWatcher watcher = watchers[i];
                string watchPath = watcher.Path.ToLower();
                if (!watcher.Filter.Equals("*.*"))
                {
                    watchPath = watchPath + "\\" + watcher.Filter.ToLower();
                }
                if (watchPath.Equals(path.ToLower())) // Duplicate file
                {
                    return false;
                }
                else if (watchPath.StartsWith(pathDirectory)) // Adding a file to a parent or monitored directory or Adding a different file
                {
                    if (watchPath.Equals(pathDirectory)) // Adding to a monitored directory
                    {
                        addToWatcher = false;
                        break;
                    }
                }
            }
            if (addToWatcher)
            {
                FileSystemWatcher watcher = CreateWatcher(file.DirectoryName, file.Name);
                watchers.Add(watcher);
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

        /// <summary>
        /// Unmonitor a path. If the Path does not exist, raise an exception
        ///    if 001:/Lectures is monitored, and i try to unmonitor 001:/Lectures/lecture1.pdf , it should fail.
        ///    if 
        ///     001:/Lectures and 001:/Lectures/lecture1.pdf is being monitored , if i remove 001/Lectures/lecture1.pdf, the next time i remove 001:/Lectures, then 001:/Lectures/lecture1.pdf should not be monitored.
        /// </summary>
        /// <param name="path">The Path to be monitored</param>
        /// <returns>Boolean stating if the monitor can be stopped</returns>
        /// <exception cref="Syncless.Monitor.MonitorPathNotFoundException">Throw when the path is not found.</exception>
        public bool UnMonitorPath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new MonitorPathNotFoundException(ErrorMessage.PATH_NOT_FOUND, path);
            }
            if (File.Exists(path))
            {
                return UnMonitorFile(path);
            }
            else
            {
                return UnMonitorDirectory(path);
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
                FileSystemWatcher watcher = watchers[i];
                string watchPath = watcher.Path.ToLower();
                if (!watcher.Filter.Equals("*.*"))
                {
                    watchPath = watchPath + "\\" + watcher.Filter.ToLower();
                }
                if (watchPath.Equals(path.ToLower()))
                {
                    watcher.Dispose();
                    watchers.RemoveAt(i);
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

        private bool UnMonitorFile(string path)
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
            for (int i = 0; i < watchers.Count; i++)
            {
                FileSystemWatcher watcher = watchers[i];
                string watchPath = watcher.Path.ToLower();
                if (!watcher.Filter.Equals("*.*"))
                {
                    watchPath = watchPath + "\\" + watcher.Filter.ToLower();
                }
                if (watchPath.Equals(path.ToLower()))
                {
                    watcher.Dispose();
                    watchers.RemoveAt(i);
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// Unmonitor all files that is contained in a physical drive 
        /// </summary>
        /// <param name="driveLetter">The drive letter (i.e 'C') </param>
        /// <returns>The number of paths unmonitored</returns>
        /// <exception cref="Syncless.Monitor.MonitorDriveNotFoundException">Throw when the drive is not found.</exception>
        public int UnMonitorDrive(string driveLetter)
        {
            DriveInfo drive = new DriveInfo(driveLetter);
            if (!drive.IsReady)
            {
                throw new MonitorDriveNotFoundException(ErrorMessage.DRIVE_NOT_FOUND, driveLetter);
            }
            for (int i = 0; i < watchers.Count; i++)
            {
                FileSystemWatcher watcher = watchers[i];
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
            return count;
        }

        private FileSystemWatcher CreateWatcher(string path, string filter)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(path, filter);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += new FileSystemEventHandler(OnModified);
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private static void OnModified(object source, FileSystemEventArgs e)
        {
            IMonitorControllerInterface monitor = ServiceLocator.MonitorI;
            if (File.Exists(e.FullPath))
            {
                Console.WriteLine("File Modified: " + e.FullPath);
                FileChangeEvent fileEvent = new FileChangeEvent(new FileInfo(e.FullPath), EventChangeType.MODIFIED);
                monitor.HandleFileChange(fileEvent);
            }
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            IMonitorControllerInterface monitor = ServiceLocator.MonitorI;
            if (File.Exists(e.FullPath))
            {
                Console.WriteLine("File Created: " + e.FullPath);
                FileChangeEvent fileEvent = new FileChangeEvent(new FileInfo(e.FullPath), EventChangeType.CREATED);
                monitor.HandleFileChange(fileEvent);
            }
            else
            {
                Console.WriteLine("Folder Created: " + e.FullPath);
                FolderChangeEvent folderEvent = new FolderChangeEvent(new DirectoryInfo(e.FullPath), EventChangeType.CREATED);
                monitor.HandleFolderChange(folderEvent);
            }
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            IMonitorControllerInterface monitor = ServiceLocator.MonitorI;
            FileSystemWatcher watcher = (FileSystemWatcher)source;
            if (!watcher.Filter.Equals("*.*"))
            {
                Console.WriteLine("File Deleted: " + e.FullPath);
                FileChangeEvent fileEvent = new FileChangeEvent(new FileInfo(e.FullPath), EventChangeType.DELETED);
                monitor.HandleFileChange(fileEvent);
            }
            else if (watcher.Path.ToLower().Equals(e.FullPath)) // Strangely this will never happen as watcher cannot detect any event from the root folder
            {
                Console.WriteLine("Folder Deleted: " + e.FullPath);
                FolderChangeEvent folderEvent = new FolderChangeEvent(new DirectoryInfo(e.FullPath), EventChangeType.DELETED);
                monitor.HandleFolderChange(folderEvent);
            }
            else
            {
                Console.WriteLine("File/Folder Deleted: " + e.FullPath);
                DeleteChangeEvent deleteEvent = new DeleteChangeEvent(new DirectoryInfo(e.FullPath), new DirectoryInfo(watcher.Path));
                monitor.HandleDeleteChange(deleteEvent);
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            IMonitorControllerInterface monitor = ServiceLocator.MonitorI;
            if (File.Exists(e.OldFullPath))
            {
                Console.WriteLine("File Renamed: " + e.OldFullPath + " " + e.FullPath);
                FileChangeEvent fileEvent = new FileChangeEvent(new FileInfo(e.OldFullPath), new FileInfo(e.FullPath));
                monitor.HandleFileChange(fileEvent);
            }
            else
            {
                Console.WriteLine("Folder Renamed: " + e.OldFullPath + " " + e.FullPath);
                FolderChangeEvent folderEvent = new FolderChangeEvent(new DirectoryInfo(e.OldFullPath), new DirectoryInfo(e.FullPath));
                monitor.HandleFolderChange(folderEvent);
            }
        }

    }
}
