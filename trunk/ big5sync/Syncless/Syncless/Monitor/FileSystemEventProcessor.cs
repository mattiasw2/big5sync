using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Syncless.Core;
using Syncless.Monitor.DTO;

namespace Syncless.Monitor
{
    public class FileSystemEventProcessor
    {
        private static FileSystemEventProcessor _instance;
        public static FileSystemEventProcessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileSystemEventProcessor();
                }
                return _instance;
            }
        }

        private Queue<Queue<FileSystemEvent>> queue;
        private List<string> createList;
        private List<FileSystemEvent> processList;
        private List<FileSystemEvent> waitingList;
        private Thread processorThread;
        private EventWaitHandle waitHandle;

        private FileSystemEventProcessor()
        {
            queue = new Queue<Queue<FileSystemEvent>>();
            createList = new List<string>();
            processList = new List<FileSystemEvent>();
            waitingList = new List<FileSystemEvent>();
            waitHandle = new AutoResetEvent(true);
        }

        public void Terminate()
        {
            waitHandle.Close();
            if (processorThread != null)
            {
                processorThread.Abort();
            }
        }

        public void Enqueue(Queue<FileSystemEvent> eventList)
        {
            lock (queue)
            {
                queue.Enqueue(eventList);
            }
            if (processorThread == null)
            {
                processorThread = new Thread(ProcessEvent);
                processorThread.Start();
            }
            else if (processorThread.ThreadState == ThreadState.WaitSleepJoin)
            {
                waitHandle.Set();
            }
        }

        private Queue<FileSystemEvent> Dequeue()
        {
            Queue<FileSystemEvent> eventList = null;
            lock (queue)
            {
                if (queue.Count != 0)
                {
                    eventList = queue.Dequeue();
                }
            }
            return eventList;
        }

        private void ProcessEvent()
        {
            while (true)
            {
                List<FileSystemEvent> eventList = new List<FileSystemEvent>(waitingList);
                waitingList.Clear();
                Queue<FileSystemEvent> dequeue = Dequeue();
                if (dequeue != null)
                {
                    eventList.AddRange(dequeue);
                    while (eventList.Count != 0)
                    {
                        FileSystemEvent fse = eventList[0];
                        eventList.RemoveAt(0);
                        switch (fse.FileSystemType)
                        {
                            case FileSystemType.FILE:
                                ProcessFile(fse, eventList);
                                break;
                            case FileSystemType.FOLDER:
                                ProcessFolder(fse, eventList);
                                break;
                            case FileSystemType.UNKNOWN:
                                ProcessUnknown(fse, eventList);
                                break;
                            default:
                                break;
                        }
                    }
                    ExecuteEvent();
                }
                else
                {
                    waitHandle.WaitOne();
                }
            }
        }

        private void ProcessFile(FileSystemEvent fse, List<FileSystemEvent> eventList)
        {
            switch (fse.EventType)
            {
                case EventChangeType.CREATING:
                    createList.Add(fse.Path);
                    break;
                case EventChangeType.CREATED:
                    for (int i = 0; i < createList.Count; i++)
                    {
                        string path = createList[i];
                        if (path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            createList.RemoveAt(i);
                            processList.Add(fse);
                            break;
                        }
                    }
                    break;
                case EventChangeType.MODIFIED:
                    foreach (string path in createList)
                    {
                        if (path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            return;
                        }
                    }
                    bool addModified = true;
                    for (int i = 0; i < processList.Count; i++)
                    {
                        FileSystemEvent pEvent = processList[i];
                        if (pEvent.Path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            addModified = false;
                            break;
                        }
                        else if (pEvent.EventType == EventChangeType.RENAMED)
                        {
                            if (pEvent.OldPath.ToLower().Equals(fse.Path.ToLower()) && pEvent.FileSystemType == fse.FileSystemType)
                            {
                                addModified = false;
                                break;
                            }
                        }
                    }
                    if (addModified)
                    {
                        processList.Add(fse);
                    }
                    break;
                case EventChangeType.DELETED:
                    bool addDeleted = true;
                    for (int i = 0; i < createList.Count; i++)
                    {
                        string path = createList[i];
                        if (path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            createList.RemoveAt(i);
                            addDeleted = false;
                            break;
                        }
                    }
                    for (int i = 0; i < processList.Count; i++)
                    {
                        FileSystemEvent pEvent = processList[i];
                        if (pEvent.Path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            processList.RemoveAt(i);
                            if (pEvent.EventType == EventChangeType.CREATED || pEvent.EventType == EventChangeType.RENAMED)
                            {
                                addDeleted = false;
                            }
                        }
                    }
                    if (addDeleted)
                    {
                        processList.Add(fse);
                    }
                    break;
                case EventChangeType.RENAMED:
                    bool hasCreating = false;
                    bool foundCreated = false;
                    for (int i = 0; i < createList.Count; i++)
                    {
                        hasCreating = true;
                        string path = createList[i];
                        if (path.ToLower().Equals(fse.OldPath.ToLower())) // Still Creating
                        {
                            for (int j = 0; j < eventList.Count; j++)
                            {
                                FileSystemEvent e = eventList[j];
                                if (e.Path.ToLower().Equals(fse.OldPath.ToLower()) && e.EventType == EventChangeType.CREATED && e.FileSystemType == fse.FileSystemType)
                                {
                                    ProcessFile(e, eventList);
                                    eventList.RemoveAt(j);
                                    foundCreated = true;
                                    break;
                                }
                            }
                        }
                    }
                    bool addRenamed = true;
                    for (int i = 0; i < processList.Count; i++)
                    {
                        FileSystemEvent pEvent = processList[i];
                        if (pEvent.Path.ToLower().Equals(fse.OldPath.ToLower()))
                        {
                            processList.RemoveAt(i);
                            if (pEvent.EventType == EventChangeType.CREATED)
                            {
                                addRenamed = false;
                                processList.Add(new FileSystemEvent(fse.Path, EventChangeType.CREATED, fse.FileSystemType));
                            }
                            else if (pEvent.EventType == EventChangeType.RENAMED)
                            {
                                addRenamed = false;
                                processList.Add(new FileSystemEvent(pEvent.OldPath, fse.Path, fse.FileSystemType));
                            }
                        }
                    }
                    if (addRenamed)
                    {
                        processList.Add(fse);
                    }
                    if (hasCreating && !foundCreated)
                    {
                        waitingList = new List<FileSystemEvent>(eventList);
                        eventList.Clear();
                    }
                    break;
                default:
                    break;
            }
        }

        private void ProcessFolder(FileSystemEvent fse, List<FileSystemEvent> eventList)
        {
            switch (fse.EventType)
            {
                case EventChangeType.CREATED:
                    processList.Add(fse);
                    break;
                case EventChangeType.DELETED:
                    bool addDeleted = true;
                    for (int i = 0; i < processList.Count; i++)
                    {
                        FileSystemEvent pEvent = processList[i];
                        if (pEvent.Path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            processList.RemoveAt(i);
                            if (pEvent.EventType == EventChangeType.CREATED || pEvent.EventType == EventChangeType.RENAMED)
                            {
                                addDeleted = false;
                            }
                        }
                    }
                    if (addDeleted)
                    {
                        processList.Add(fse);
                    }
                    break;
                case EventChangeType.RENAMED:
                    bool addRenamed = true;
                    for (int i = 0; i < processList.Count; i++)
                    {
                        FileSystemEvent pEvent = processList[i];
                        if (pEvent.Path.ToLower().Equals(fse.OldPath.ToLower()))
                        {
                            processList.RemoveAt(i);
                            if (pEvent.EventType == EventChangeType.CREATED)
                            {
                                addRenamed = false;
                                processList.Add(new FileSystemEvent(fse.Path, EventChangeType.CREATED, fse.FileSystemType));
                            }
                            else if (pEvent.EventType == EventChangeType.RENAMED)
                            {
                                addRenamed = false;
                                processList.Add(new FileSystemEvent(pEvent.OldPath, fse.Path, fse.FileSystemType));
                            }
                        }
                    }
                    if (addRenamed)
                    {
                        processList.Add(fse);
                    }
                    break;
                default:
                    break;
            }
        }

        private void ProcessUnknown(FileSystemEvent fse, List<FileSystemEvent> eventList)
        {
            switch (fse.EventType)
            {
                case EventChangeType.CREATED:
                    processList.Add(fse);
                    break;
                case EventChangeType.DELETED:
                    bool addDeleted = true;
                    for (int i = 0; i < createList.Count; i++)
                    {
                        string path = createList[i];
                        if (path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            createList.RemoveAt(i);
                            addDeleted = false;
                            break;
                        }
                    }
                    for (int i = 0; i < processList.Count; i++)
                    {
                        FileSystemEvent pEvent = processList[i];
                        if (pEvent.Path.ToLower().Equals(fse.Path.ToLower()))
                        {
                            processList.RemoveAt(i);
                            if (pEvent.EventType == EventChangeType.CREATED || pEvent.EventType == EventChangeType.RENAMED)
                            {
                                addDeleted = false;
                            }
                        }
                    }
                    if (addDeleted)
                    {
                        processList.Add(fse);
                    }
                    break;
                case EventChangeType.RENAMED:
                    bool hasCreating = false;
                    bool foundCreated = false;
                    for (int i = 0; i < createList.Count; i++)
                    {
                        hasCreating = true;
                        string path = createList[i];
                        if (path.ToLower().Equals(fse.OldPath.ToLower())) // Still Creating
                        {
                            for (int j = 0; j < eventList.Count; j++)
                            {
                                FileSystemEvent e = eventList[j];
                                if (e.Path.ToLower().Equals(fse.OldPath.ToLower()) && e.EventType == EventChangeType.CREATED)
                                {
                                    ProcessFile(e, eventList);
                                    eventList.RemoveAt(j);
                                    foundCreated = true;
                                    break;
                                }
                            }
                        }
                    }
                    bool addRenamed = true;
                    for (int i = 0; i < processList.Count; i++)
                    {
                        FileSystemEvent pEvent = processList[i];
                        if (pEvent.Path.ToLower().Equals(fse.OldPath.ToLower()))
                        {
                            processList.RemoveAt(i);
                            if (pEvent.EventType == EventChangeType.CREATED)
                            {
                                addRenamed = false;
                                processList.Add(new FileSystemEvent(fse.Path, EventChangeType.CREATED, fse.FileSystemType));
                            }
                            else if (pEvent.EventType == EventChangeType.RENAMED)
                            {
                                addRenamed = false;
                                processList.Add(new FileSystemEvent(pEvent.OldPath, fse.Path, fse.FileSystemType));
                            }
                        }
                    }
                    if (addRenamed)
                    {
                        processList.Add(fse);
                    }
                    if (hasCreating && !foundCreated)
                    {
                        waitingList = new List<FileSystemEvent>(eventList);
                        eventList.Clear();
                    }
                    break;
                default:
                    break;
            }
        }

        private void ExecuteEvent()
        {
            foreach (FileSystemEvent fse in processList)
            {
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
                        break;
                }
            }
            processList.Clear();
        }

        private void ExecuteFile(FileSystemEvent fse)
        {
            FileChangeEvent fileEvent;
            switch (fse.EventType)
            {
                case EventChangeType.CREATED:
                    Console.WriteLine("File Created: " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.Path), EventChangeType.CREATED);
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                case EventChangeType.MODIFIED:
                    Console.WriteLine("File Modified: " + fse.Path);
                    fileEvent = new FileChangeEvent(new FileInfo(fse.Path), EventChangeType.MODIFIED);
                    ServiceLocator.MonitorI.HandleFileChange(fileEvent);
                    break;
                case EventChangeType.DELETED:
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

        private void ExecuteFolder(FileSystemEvent fse)
        {
            FolderChangeEvent folderEvent;
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

        private void ExecuteUnknown(FileSystemEvent fse)
        {
            switch (fse.EventType)
            {
                case EventChangeType.DELETED:
                    Console.WriteLine("File/Folder Deleted: " + fse.Path);
                    DeleteChangeEvent deleteEvent = new DeleteChangeEvent(new DirectoryInfo(fse.Path), new DirectoryInfo(fse.WatchPath));
                    ServiceLocator.MonitorI.HandleDeleteChange(deleteEvent);
                    break;
                default:
                    break;
            }
        }
    }
}
