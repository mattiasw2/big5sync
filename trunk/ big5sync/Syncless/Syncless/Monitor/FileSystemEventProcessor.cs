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
                                Debug.Assert(false);
                                break;
                        }
                    }
                    FileSystemEventExecutor.Instance.Enqueue(processList);
                    processList.Clear();
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
                    FileCreated(fse);
                    break;
                case EventChangeType.MODIFIED:
                    if (!FileModified(fse)) return;
                    break;
                case EventChangeType.DELETED:
                    GenericDeleted(fse);
                    break;
                case EventChangeType.RENAMED:
                    GenericRenamed(fse, eventList);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void FileCreated(FileSystemEvent fse)
        {
            for (int i = 0; i < createList.Count; i++)
            {
                string path = createList[i];
                if (path.ToLower().Equals(fse.Path.ToLower()))
                {
                    createList.RemoveAt(i);
                    processList.Add(fse);
                    return;
                }
            }
        }

        private bool FileModified(FileSystemEvent fse)
        {
            foreach (string path in createList)
            {
                if (path.ToLower().Equals(fse.Path.ToLower()))
                {
                    return false;
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
            return true;
        }

        private void GenericDeleted(FileSystemEvent fse)
        {
            bool addDeleted = true;
            if (fse.FileSystemType != FileSystemType.FOLDER)
            {
                addDeleted = DeletedHasCreating(fse);
            }
            for (int i = 0; i < processList.Count; i++)
            {
                FileSystemEvent pEvent = processList[i];
                if (pEvent.Path.ToLower().Equals(fse.Path.ToLower()))
                {
                    processList.RemoveAt(i);
                    i--;
                    if (pEvent.EventType == EventChangeType.CREATED || pEvent.EventType == EventChangeType.RENAMED)
                    {
                        addDeleted = false;
                    }
                }
                if (pEvent.EventType == EventChangeType.DELETED) // Remove All Child Deleted Event
                {
                    FileInfo child = new FileInfo(pEvent.Path);
                    DirectoryInfo parent = new DirectoryInfo(fse.Path);
                    if (child.Directory.FullName.ToLower().Equals(parent.FullName.ToLower()))
                    {
                        processList.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (addDeleted)
            {
                processList.Add(fse);
            }
        }

        private bool DeletedHasCreating(FileSystemEvent fse)
        {
            for (int i = 0; i < createList.Count; i++)
            {
                string path = createList[i];
                if (path.ToLower().Equals(fse.Path.ToLower()))
                {
                    createList.RemoveAt(i);
                    return false;
                }
            }
            return true;
        }

        private void GenericRenamed(FileSystemEvent fse, List<FileSystemEvent> eventList)
        {
            bool hasCreating = false;
            bool foundCreated = false;

            if (fse.FileSystemType != FileSystemType.FOLDER)
            {
                RenamedHasCreating(fse, eventList, ref hasCreating, ref foundCreated);
            }

            bool addRenamed = true;
            for (int i = 0; i < processList.Count; i++)
            {
                FileSystemEvent pEvent = processList[i];
                if (pEvent.Path.ToLower().Equals(fse.OldPath.ToLower()))
                {
                    processList.RemoveAt(i);
                    i--;
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

            if (fse.FileSystemType != FileSystemType.FOLDER)
            {
                if (hasCreating && !foundCreated)
                {
                    waitingList = new List<FileSystemEvent>(eventList);
                    eventList.Clear();
                }
            }
        }

        private void RenamedHasCreating(FileSystemEvent fse, List<FileSystemEvent> eventList, ref bool hasCreating, ref bool foundCreated)
        {
            for (int i = 0; i < createList.Count; i++)
            {
                string path = createList[i];
                if (path.ToLower().Equals(fse.OldPath.ToLower())) // Still Creating
                {
                    hasCreating = true;
                    for (int j = 0; j < eventList.Count; j++)
                    {
                        FileSystemEvent e = eventList[j];
                        if (e.Path.ToLower().Equals(fse.OldPath.ToLower()) && e.EventType == EventChangeType.CREATED && e.FileSystemType == FileSystemType.FILE)
                        {
                            ProcessFile(e, eventList);
                            eventList.RemoveAt(j);
                            foundCreated = true;
                            break;
                        }
                    }
                }
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
                    GenericDeleted(fse);
                    break;
                case EventChangeType.RENAMED:
                    GenericRenamed(fse, eventList);
                    break;
                default:
                    Debug.Assert(false);
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
                    GenericDeleted(fse);
                    break;
                case EventChangeType.RENAMED:
                    GenericRenamed(fse, eventList);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }
    }
}
