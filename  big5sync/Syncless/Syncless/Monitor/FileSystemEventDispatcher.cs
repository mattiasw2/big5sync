using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Syncless.Core;
using Syncless.Monitor.DTO;

namespace Syncless.Monitor
{
    public class FileSystemEventDispatcher
    {
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
        private List<string> createList;
        private Thread dispatcherThread;
        private EventWaitHandle waitHandle;

        private FileSystemEventDispatcher()
        {
            queue = new List<FileSystemEvent>();
            createList = new List<string>();
            waitHandle = new AutoResetEvent(true);
        }

        public void AddToQueue(FileSystemEvent e)
        {
            queue.Add(e);
            if (dispatcherThread == null)
            {
                dispatcherThread = new Thread(DispatchEvent);
                //dispatcherThread.IsBackground = true;
                dispatcherThread.Start();
            }
            else if (dispatcherThread.ThreadState == ThreadState.WaitSleepJoin)
            {
                waitHandle.Set();
            }
        }

        private void DispatchEvent()
        {
            while (true)
            {
                if (queue.Count != 0)
                {
                    FileSystemEvent fse = queue[0];
                    switch (fse.FileSystemType)
                    {
                        case FileSystemType.FILE:
                            DispatchFile(fse);
                            break;
                        case FileSystemType.FOLDER:
                            DispatchFolder(fse);
                            break;
                        case FileSystemType.UNKNOWN:
                            DispatchUnknown(fse);
                            break;
                        default:
                            break;
                    }
                    queue.RemoveAt(0);
                }
                else
                {
                    waitHandle.WaitOne();
                }
            }
        }

        private void DispatchFile(FileSystemEvent fse)
        {
            FileChangeEvent fileEvent = null;
            switch (fse.EventType)
            {
                case EventChangeType.CREATING:
                    Console.WriteLine("File Started Creating: " + fse.Path);
                    createList.Add(fse.Path);
                    break;
                case EventChangeType.CREATED:
                    bool execute = false;
                    for (int i = 0; i < createList.Count; i++)
                    {
                        string path = createList[i];
                        if (path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            Console.WriteLine("File Finished Creating: " + fse.Path);
                            createList.RemoveAt(i);
                            execute = true;
                            break;
                        }
                    }
                    if (execute)
                    {
                        fileEvent = new FileChangeEvent(new FileInfo(fse.Path), EventChangeType.CREATED);
                        ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    }
                    break;
                case EventChangeType.MODIFIED:
                    foreach (string path in createList)
                    {
                        if (path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            Console.WriteLine("File Still Creating: " + fse.Path);
                            return;
                        }
                    }
                    Console.WriteLine("File Modified: " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.Path), EventChangeType.MODIFIED);
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                case EventChangeType.DELETED:
                    for (int i = 0; i < createList.Count; i++)
                    {
                        string path = createList[i];
                        if (path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            Console.WriteLine("File Deleted: " + fse.Path);
                            createList.RemoveAt(i);
                            return;
                        }
                    }
                    Console.WriteLine("File Deleted: " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.Path), EventChangeType.DELETED);
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                case EventChangeType.RENAMED:
                    
                    Console.WriteLine("File Renamed: " + fse.OldPath + " " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.OldPath), new FileInfo(fse.Path));
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                default:
                    break;
            }
        }

        private void DispatchFolder(FileSystemEvent fse)
        {
            FolderChangeEvent folderEvent = null;
            switch (fse.EventType)
            {
                case EventChangeType.CREATED:
                    Console.WriteLine("Folder Created: " + fse.Path);
                    folderEvent = new FolderChangeEvent(new DirectoryInfo(fse.Path), EventChangeType.CREATED);
                    ServiceLocator.MonitorI.HandleFolderChange(folderEvent);
                    break;
                case EventChangeType.DELETED:
                    Console.WriteLine("Folder Deleted: " + fse.Path);
                    folderEvent = new FolderChangeEvent(new DirectoryInfo(fse.Path), EventChangeType.DELETED);
                    ServiceLocator.MonitorI.HandleFolderChange(folderEvent);
                    break;
                case EventChangeType.RENAMED:
                    Console.WriteLine("Folder Renamed: " + fse.OldPath + " " + fse.Path);
                    folderEvent = new FolderChangeEvent(new DirectoryInfo(fse.OldPath), new DirectoryInfo(fse.Path));
                    ServiceLocator.MonitorI.HandleFolderChange(folderEvent);
                    break;
                default:
                    break;
            }
        }

        private void DispatchUnknown(FileSystemEvent fse)
        {
            switch (fse.EventType)
            {
                case EventChangeType.CREATED:
                    Console.WriteLine("Created File not exist! " + fse.Path);
                    break;
                case EventChangeType.DELETED:
                    for (int i = 0; i < createList.Count; i++)
                    {
                        string path = createList[i];
                        if (path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            Console.WriteLine("File Deleted: " + fse.Path);
                            createList.RemoveAt(i);
                            return;
                        }
                    }
                    Console.WriteLine("File/Folder Deleted: " + fse.Path);
                    DeleteChangeEvent deleteEvent = new DeleteChangeEvent(new DirectoryInfo(fse.Path), new DirectoryInfo(fse.WatchPath));
                    ServiceLocator.MonitorI.HandleDeleteChange(deleteEvent);
                    break;
                case EventChangeType.RENAMED:
                    Console.WriteLine("Renamed File not exist! " + fse.OldPath + " " + fse.Path);
                    break;
                default:
                    break;
            }
        }
    }
}
