using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Syncless.Core;
using Syncless.Monitor.DTO;
using ThreadState = System.Threading.ThreadState;

namespace Syncless.Monitor
{
    public class FileSystemEventExecutor
    {
        private const int CLEAR_PATH_TABLE_TIME = 30;
        private const int FILE_BUSY_TIME = 1000;

        private static FileSystemEventExecutor _instance;
        public static FileSystemEventExecutor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileSystemEventExecutor();
                }
                return _instance;
            }
        }

        private List<FileSystemEvent> queue;
        private DateTime timer;
        private Thread executorThread;
        private EventWaitHandle waitHandle;

        private FileSystemEventExecutor()
        {
            queue = new List<FileSystemEvent>();
            timer = DateTime.Now;
            waitHandle = new AutoResetEvent(true);
        }

        public void Terminate()
        {
            waitHandle.Close();
            if (executorThread != null)
            {
                executorThread.Abort();
            }
        }

        public void Enqueue(List<FileSystemEvent> eventList)
        {
            lock (queue)
            {
                queue.AddRange(eventList);
            }
            if (executorThread == null)
            {
                executorThread = new Thread(ExecuteEvent);
                executorThread.Start();
            }
            else if (executorThread.ThreadState == ThreadState.WaitSleepJoin)
            {
                ClearPathTable();
                waitHandle.Set();
            }
        }

        private FileSystemEvent Dequeue(int index)
        {
            FileSystemEvent fse = null;
            lock (queue)
            {
                if (queue.Count > index)
                {
                    fse = queue[index];
                    queue.RemoveAt(index);
                }
            }
            return fse;
        }

        private FileSystemEvent Peek(int index)
        {
            FileSystemEvent fse = null;
            lock (queue)
            {
                if (queue.Count > index)
                {
                    fse = queue[index];
                }
            }
            return fse;
        }

        private void ExecuteEvent()
        {
            int index = 0;
            while (true)
            {
                if (queue.Count != 0)
                {
                    FileSystemEvent fse = Peek(index);
                    if (fse == null)
                    {
                        Thread.Sleep(FILE_BUSY_TIME);
                        index = 0;
                        continue;
                    }
                    try
                    {
                        if (fse.FileSystemType == FileSystemType.FILE)
                        {
                            if (File.Exists(fse.Path))
                            {
                                FileStream fs = new FileStream(fse.Path, FileMode.Open, FileAccess.Read, FileShare.None);
                                fs.Close();
                            }
                            else
                            {
                                Dequeue(index);
                                index = 0;
                                continue;
                            }
                        }
                        else if (fse.FileSystemType == FileSystemType.FOLDER && fse.EventType != EventChangeType.DELETED)
                        {
                            if (!Directory.Exists(fse.Path))
                            {
                                Dequeue(index);
                                index = 0;
                                continue;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        index++;
                        continue;
                    }

                    switch (fse.FileSystemType)
                    {
                        case FileSystemType.FILE:
                            ExecuteFile(fse);
                            break;
                        case FileSystemType.FOLDER:
                            ExecuteFolder(fse);
                            break;
                        case FileSystemType.UNKNOWN:
                            ExecuteUnknown(fse);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                    Dequeue(index);
                    index = 0;
                }
                else
                {
                    timer = DateTime.Now;
                    waitHandle.WaitOne();
                }
            }
        }

        private void ExecuteFile(FileSystemEvent fse)
        {
            FileChangeEvent fileEvent;
            switch (fse.EventType)
            {
                case EventChangeType.CREATED:
                    //ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("File Created: " + fse.Path);
                    //Console.WriteLine("File Created: " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.Path), EventChangeType.CREATED);
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                case EventChangeType.MODIFIED:
                    //ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("File Modified: " + fse.Path);
                    //Console.WriteLine("File Modified: " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.Path), EventChangeType.MODIFIED);
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                case EventChangeType.DELETED:
                    //ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("File Deleted: " + fse.Path);
                    //Console.WriteLine("File Deleted: " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.Path), EventChangeType.DELETED);
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                case EventChangeType.RENAMED:
                    //ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("File Renamed: " + fse.OldPath + " " + fse.Path);
                    //Console.WriteLine("File Renamed: " + fse.OldPath + " " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.OldPath), new FileInfo(fse.Path));
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void ExecuteFolder(FileSystemEvent fse)
        {
            FolderChangeEvent folderEvent;
            switch (fse.EventType)
            {
                case EventChangeType.CREATED:
                    //ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Folder Created: " + fse.Path);
                    //Console.WriteLine("Folder Created: " + fse.Path);
                    folderEvent = new FolderChangeEvent(new DirectoryInfo(fse.Path), EventChangeType.CREATED);
                    ServiceLocator.MonitorI.HandleFolderChange(folderEvent);
                    break;
                case EventChangeType.DELETED:
                    //ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Folder Deleted: " + fse.Path);
                    //Console.WriteLine("Folder Deleted: " + fse.Path);
                    folderEvent = new FolderChangeEvent(new DirectoryInfo(fse.Path), EventChangeType.DELETED);
                    ServiceLocator.MonitorI.HandleFolderChange(folderEvent);
                    break;
                case EventChangeType.RENAMED:
                    //ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Folder Renamed: " + fse.OldPath + " " + fse.Path);
                    //Console.WriteLine("Folder Renamed: " + fse.OldPath + " " + fse.Path);
                    folderEvent = new FolderChangeEvent(new DirectoryInfo(fse.OldPath), new DirectoryInfo(fse.Path));
                    ServiceLocator.MonitorI.HandleFolderChange(folderEvent);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void ExecuteUnknown(FileSystemEvent fse)
        {
            switch (fse.EventType)
            {
                case EventChangeType.DELETED:
                    //ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("File/Folder Deleted: " + fse.Path);
                    //Console.WriteLine("File/Folder Deleted: " + fse.Path);
                    DeleteChangeEvent deleteEvent = new DeleteChangeEvent(new DirectoryInfo(fse.Path), new DirectoryInfo(fse.WatchPath));
                    ServiceLocator.MonitorI.HandleDeleteChange(deleteEvent);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void ClearPathTable()
        {
            TimeSpan idleTime = new TimeSpan(DateTime.Now.ToBinary() - timer.ToBinary());
            if (idleTime.TotalSeconds >= CLEAR_PATH_TABLE_TIME)
            {
                ServiceLocator.MonitorI.ClearPathHash();
            }
        }
    }
}
