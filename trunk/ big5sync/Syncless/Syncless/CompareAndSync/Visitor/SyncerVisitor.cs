using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.Core;
using Syncless.Notification;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Visitor
{
    public class SyncerVisitor : IVisitor
    {

        #region IVisitor Members
        private readonly SyncConfig _syncConfig;
        private readonly SyncProgress _syncProgress;

        public SyncerVisitor(SyncConfig syncConfig, SyncProgress progress)
        {
            _syncConfig = syncConfig;
            _syncProgress = progress;
        }

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            _nodesCount++;
            _syncProgress.Message = "Synchronizing " + file.Name;
            if (file.Invalid)
            {
                _syncProgress.fail();
                return;
            }

            //int maxPriorityPos = 0;
            //for (int i = 0; i < numOfPaths; i++)
            //{
            //    if (file.Priority[i] > file.Priority[maxPriorityPos])
            //        maxPriorityPos = i;
            //}

            int maxPriorityPos = file.SourcePosition;

            if (file.Priority[maxPriorityPos] > 0)
            {
                switch (file.ChangeType[maxPriorityPos])
                {
                    case MetaChangeType.Delete:
                        DeleteFile(file, numOfPaths, maxPriorityPos);
                        break;
                    case MetaChangeType.New:
                    case MetaChangeType.Update:
                    case MetaChangeType.NoChange:
                        CopyFile(file, numOfPaths, maxPriorityPos);
                        break;

                    case MetaChangeType.Rename:
                        MoveFile(file, numOfPaths, maxPriorityPos);
                        break;
                }
            }
            _syncProgress.complete();
            //Basic logic: Look for highest priority and propagate it.
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            _nodesCount++;
            _syncProgress.Message = "Synchronizing " + folder.Name;
            if (folder.Invalid)
            {
                _syncProgress.fail();
                return;
            }

            //int maxPriorityPos = 0;
            //for (int i = 0; i < numOfPaths; i++)
            //{
            //    if (folder.Priority[i] > folder.Priority[maxPriorityPos])
            //        maxPriorityPos = i;
            //}

            int maxPriorityPos = folder.SourcePosition;

            if (folder.Priority[maxPriorityPos] > 0)
            {
                switch (folder.ChangeType[maxPriorityPos])
                {
                    case MetaChangeType.Delete:
                        DeleteFolder(folder, numOfPaths, maxPriorityPos);
                        break;
                    case MetaChangeType.New:
                    case MetaChangeType.NoChange:
                        CreateFolder(folder, numOfPaths, maxPriorityPos);
                        break;
                    case MetaChangeType.Rename:
                        MoveFolder(folder, numOfPaths, maxPriorityPos);
                        break;
                }
            }
            _syncProgress.complete();
        }

        public void Visit(RootCompareObject root)
        {
            _nodesCount++;
            _syncProgress.complete();//Do nothing
        }

        #endregion

        #region File Methods

        private int _nodesCount;

        public int NodesCount
        {
            get { return _nodesCount; }
            set { _nodesCount = value; }
        }

        private void CopyFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            string src = Path.Combine(fco.GetSmartParentPath(srcFilePos), fco.Name);
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos && fco.Parent.FinalState[i] != FinalState.Deleted)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        string destFile = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                        bool fileExists = File.Exists(destFile);
                        try
                        {
                            if (fileExists)
                            {
                                if (_syncConfig.ArchiveLimit >= 0)
                                    CommonMethods.ArchiveFile(destFile, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                                if (_syncConfig.Recycle)
                                    CommonMethods.DeleteFileToRecycleBin(destFile);
                            }

                            CommonMethods.CopyFile(src, destFile, true);
                            fco.CreationTime[i] = new FileInfo(destFile).CreationTime.Ticks;
                            fco.Exists[i] = true;
                            fco.FinalState[i] = fileExists ? FinalState.Updated : FinalState.Created;
                            fco.Hash[i] = fco.Hash[srcFilePos];
                            fco.LastWriteTime[i] = fco.LastWriteTime[srcFilePos];
                            fco.Length[i] = fco.LastWriteTime[srcFilePos];
                            changed = true;
                        }
                        catch (ArchiveFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "Error archiving file " + destFile));
                        }
                        catch (CopyFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            if (fileExists)
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_MODIFIED, "Error updating file from " + src + " to " + destFile));
                            else
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Error copying file from " + src + " to " + destFile));
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = changed ? FinalState.Propagated : FinalState.Unchanged;
        }

        private void DeleteFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        string destFile = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                        try
                        {
                            if (_syncConfig.ArchiveLimit >= 0)
                                CommonMethods.ArchiveFile(destFile, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                            if (_syncConfig.Recycle)
                                CommonMethods.DeleteFileToRecycleBin(destFile);
                            else
                                CommonMethods.DeleteFile(destFile);

                            fco.Exists[i] = false;
                            fco.FinalState[i] = FinalState.Deleted;
                            changed = true;
                        }
                        catch (ArchiveFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "Error archiving file " + destFile));
                        }
                        catch (DeleteFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "Error deleting file " + destFile));
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = changed ? FinalState.Propagated : FinalState.Unchanged;
        }

        private void MoveFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        string oldName = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                        string newName = Path.Combine(fco.GetSmartParentPath(i), fco.NewName);
                        string srcName = Path.Combine(fco.GetSmartParentPath(srcFilePos), fco.NewName);

                        try
                        {
                            if (File.Exists(oldName))
                            {
                                CommonMethods.MoveFile(oldName, newName);
                                //fco.FinalState[i] = FinalState.Renamed;
                            }
                            else
                            {
                                CommonMethods.CopyFile(srcName, newName, true);
                                //fco.FinalState[i] = FinalState.Created;
                            }
                            fco.FinalState[i] = FinalState.Renamed;
                            changed = true;
                        }
                        catch (MoveFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_RENAMED, "Error renaming file from " + oldName + " to " + newName));
                        }
                        catch (CopyFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Error copying file from " + srcName + " to " + newName));
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = changed ? FinalState.Propagated : FinalState.Unchanged;
        }

        #endregion

        #region Folder Methods

        private void CreateFolder(FolderCompareObject folder, int numOfPaths, int srcFilePos)
        {
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFilePos])
                    {
                        string folderToCreate = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                        if (!Directory.Exists(folderToCreate))
                        {
                            try
                            {
                                CommonMethods.CreateFolder(folderToCreate);
                                folder.Exists[i] = true;
                                folder.FinalState[i] = FinalState.Created;
                            }
                            catch (CreateFolderException e)
                            {
                                folder.FinalState[i] = FinalState.Error;
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Error creating folder " + folderToCreate));
                            }
                        }
                        changed = true;
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFilePos] = changed ? FinalState.Propagated : FinalState.Unchanged;
        }

        private void DeleteFolder(FolderCompareObject folder, int numOfPaths, int srcFolderPos)
        {
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFolderPos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFolderPos])
                    {
                        string destFolder = Path.Combine(folder.GetSmartParentPath(i), folder.Name);

                        try
                        {
                            if (_syncConfig.ArchiveLimit >= 0)
                                CommonMethods.ArchiveFolder(destFolder, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                            if (_syncConfig.Recycle)
                                CommonMethods.DeleteFolderToRecycleBin(destFolder);
                            else
                                CommonMethods.DeleteFolder(destFolder, true);

                            folder.Exists[i] = false;
                            folder.FinalState[i] = FinalState.Deleted;
                            folder.Contents.Clear(); //Experimental
                            changed = true;
                        }
                        catch (ArchiveFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "Error archiving folder " + destFolder));
                        }
                        catch (DeleteFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "Error deleting folder " + destFolder));
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFolderPos] = changed ? FinalState.Propagated : FinalState.Unchanged;
        }

        private void MoveFolder(FolderCompareObject folder, int numOfPaths, int srcFolderPos)
        {
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFolderPos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFolderPos])
                    {
                        string oldFolderName = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                        string newFolderName = Path.Combine(folder.GetSmartParentPath(i), folder.NewName);
                        string srcFolderName = Path.Combine(folder.GetSmartParentPath(srcFolderPos), folder.NewName);

                        try
                        {
                            if (Directory.Exists(oldFolderName))
                            {
                                CommonMethods.MoveFolder(oldFolderName, newFolderName);
                                //folder.FinalState[i] = FinalState.Renamed;
                            }
                            else
                            {
                                CommonMethods.CopyDirectory(srcFolderName, newFolderName);
                                //folder.FinalState[i] = FinalState.Created;
                            }
                            folder.FinalState[i] = FinalState.Renamed;
                            changed = true;
                        }
                        catch (MoveFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_RENAMED, "Error renaming folder from " + oldFolderName + " to " + newFolderName));
                        }
                        catch (CopyFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Error copying folder: " + srcFolderName + " to " + newFolderName));
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFolderPos] = changed ? FinalState.Propagated : FinalState.Unchanged;
        }

        #endregion

    }


}
