/*
 * 
 * Author: Koh Cher Guan
 * 
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Syncless.Monitor.DTO;
using ThreadState = System.Threading.ThreadState;

namespace Syncless.Monitor
{
    /// <summary>
    /// As the event fired by <see cref="System.IO.FileSystemWatcher" /> and <see cref="Syncless.Monitor.ExtendedFileSystemWatcher" /> are based on how the Operating System and the various Application handle an operation,
    /// a simple operation like updating a file may fires a series of different events.
    /// This class will attempt to combine a series of events into a single event as far as possible.
    /// The events will then be dispatched to <see cref="Syncless.Monitor.FileSystemEventExecutor" /> to request for execution of the events thru <see cref="Syncless.Core.IMonitorControllerInterface" />.
    /// </summary>
    public class FileSystemEventProcessor
    {
        private static FileSystemEventProcessor _instance;
        /// <summary>
        /// Get the instance of the <see cref="Syncless.Monitor.FileSystemEventDispatcher"/> Component
        /// </summary>
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
        private List<string> creatingList;
        private List<FileSystemEvent> processList;
        private List<FileSystemEvent> waitingList;
        private Thread processorThread;
        private EventWaitHandle waitHandle;

        private FileSystemEventProcessor()
        {
            queue = new Queue<Queue<FileSystemEvent>>();
            creatingList = new List<string>();
            processList = new List<FileSystemEvent>();
            waitingList = new List<FileSystemEvent>();
            waitHandle = new AutoResetEvent(true);
        }

        /// <summary>
        /// Stop the thread of this component
        /// </summary>
        public void Terminate()
        {
            waitHandle.Close();
            if (processorThread != null)
            {
                processorThread.Abort();
            }
        }

        /// <summary>
        /// Enqueue events and wait to be processed.
        /// </summary>
        /// <param name="eventList">A <see cref="Queue{T}"/> object containing the information needed to handle a request.</param>
        public void Enqueue(Queue<FileSystemEvent> eventList)
        {
            lock (queue)
            {
                queue.Enqueue(eventList);
            }
            if (processorThread == null)
            {
                processorThread = new Thread(ProcessEvent); // start the thread if not started
                processorThread.Start();
            }
            else if (processorThread.ThreadState == ThreadState.WaitSleepJoin) // wake the thread if sleeped
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
                List<FileSystemEvent> eventList = new List<FileSystemEvent>(waitingList); // include all events in the waiting list
                waitingList.Clear();
                Queue<FileSystemEvent> dequeue = Dequeue();
                if (dequeue != null)
                {
                    eventList.AddRange(dequeue); // events from waiting list and the new received events will be processed
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
                    FileSystemEventExecutor.Instance.Enqueue(processList); // send for executing
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
                    creatingList.Add(fse.Path);
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
            for (int i = 0; i < creatingList.Count; i++) // remove from the creating list since it is created.
            {
                string path = creatingList[i];
                if (path.ToLower().Equals(fse.Path.ToLower()))
                {
                    creatingList.RemoveAt(i);
                    processList.Add(fse);
                    return;
                }
            }
        }

        private bool FileModified(FileSystemEvent fse)
        {
            foreach (string path in creatingList) // ignore this event if it is still creating
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
                if (pEvent.Path.ToLower().Equals(fse.Path.ToLower())) // ignore this event if there exist any event for this path
                {
                    addModified = false;
                    break;
                }
                else if (pEvent.EventType == EventChangeType.RENAMED) // ignore this event if a path is renamed
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
            if (fse.FileSystemType != FileSystemType.FOLDER) // does not apply if is a folder
            {
                addDeleted = DeletedHasCreating(fse); // ignore this event if it is still creating
            }
            for (int i = 0; i < processList.Count; i++)
            {
                FileSystemEvent pEvent = processList[i];
                if (pEvent.Path.ToLower().Equals(fse.Path.ToLower())) // remove any existing event if exist
                {
                    processList.RemoveAt(i);
                    i--;
                    // "creation" (renamed is also a form of creation) is found so there is not a need to execute this event
                    if (pEvent.EventType == EventChangeType.CREATED || pEvent.EventType == EventChangeType.RENAMED)
                    {
                        addDeleted = false; 
                    }
                }
                if (pEvent.EventType == EventChangeType.DELETED) // remove all child deleted event so only the root path will be executed
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
            for (int i = 0; i < creatingList.Count; i++) // ignore this event if it is still creating
            {
                string path = creatingList[i];
                if (path.ToLower().Equals(fse.Path.ToLower()))
                {
                    creatingList.RemoveAt(i); // file is deleted so no need to wait for the created event
                    return false;
                }
            }
            return true;
        }

        private void GenericRenamed(FileSystemEvent fse, List<FileSystemEvent> eventList)
        {
            bool hasCreating = false;
            bool foundCreated = false;

            if (fse.FileSystemType != FileSystemType.FOLDER) // does not apply if is a folder
            {
                RenamedHasCreating(fse, eventList, ref hasCreating, ref foundCreated); // wait for the created event if a creating event exist
            }

            bool addRenamed = true;
            for (int i = 0; i < processList.Count; i++)
            {
                FileSystemEvent pEvent = processList[i];
                if (pEvent.Path.ToLower().Equals(fse.OldPath.ToLower())) // remove any existing event if exist
                {
                    processList.RemoveAt(i);
                    i--;
                    if (pEvent.EventType == EventChangeType.CREATED) // if is created event, transform it to a create event with the new path name
                    {
                        addRenamed = false;
                        processList.Add(new FileSystemEvent(fse.Path, EventChangeType.CREATED, fse.FileSystemType));
                    }
                    else if (pEvent.EventType == EventChangeType.RENAMED) // if is renamed event, transform it to a renamed event with the new old path name and new path name
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

            if (fse.FileSystemType != FileSystemType.FOLDER) // does not apply if is a folder
            {
                if (hasCreating && !foundCreated) // put all remainding events into waiting list if the created event is not found
                {
                    waitingList = new List<FileSystemEvent>(eventList);
                    eventList.Clear();
                }
            }
        }

        private void RenamedHasCreating(FileSystemEvent fse, List<FileSystemEvent> eventList, ref bool hasCreating, ref bool foundCreated)
        {
            for (int i = 0; i < creatingList.Count; i++) // wait for the created event if a creating event exist
            {
                string path = creatingList[i];
                if (path.ToLower().Equals(fse.OldPath.ToLower())) // still creating
                {
                    hasCreating = true;
                    for (int j = 0; j < eventList.Count; j++) // look for the created event
                    {
                        FileSystemEvent e = eventList[j];
                        if (e.Path.ToLower().Equals(fse.OldPath.ToLower()) && e.EventType == EventChangeType.CREATED && e.FileSystemType == FileSystemType.FILE)
                        {
                            ProcessFile(e, eventList); // process the created event first
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
