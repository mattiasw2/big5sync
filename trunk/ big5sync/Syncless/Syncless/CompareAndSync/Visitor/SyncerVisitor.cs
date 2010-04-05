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
            _syncProgress.Message = file.Name;
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
            _syncProgress.Message = folder.Name;
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
                                {
                                    CommonMethods.ArchiveFile(destFile, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "File archived " + destFile));
                                }
                                if (_syncConfig.Recycle)
                                {
                                    CommonMethods.DeleteFileToRecycleBin(destFile);
                                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "File deleted to recycle bin " + destFile));
                                }
                            }

                        }
                        catch (ArchiveFileException)
                        {
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error archiving file " + destFile));
                        }
                        catch (DeleteFileException)
                        {
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error delete file to recycle bin " + destFile));
                        }

                        try
                        {
                            CommonMethods.CopyFile(src, destFile, true);
                            fco.CreationTime[i] = File.GetCreationTime(destFile).Ticks;
                            fco.LastWriteTime[i] = File.GetLastWriteTime(destFile).Ticks;
                            fco.Exists[i] = true;
                            fco.FinalState[i] = fileExists ? FinalState.Updated : FinalState.Created;
                            fco.Hash[i] = fco.Hash[srcFilePos];
                            fco.Length[i] = fco.Length[srcFilePos];

                            if (fileExists)
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_MODIFIED, "File updated from " + src + " to " + destFile));
                            else
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "File copied from " + src + " to " + destFile));
                        }
                        catch (CopyFileException)
                        {
                            fco.FinalState[i] = FinalState.Error;

                            if (fileExists)
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error updating file from " + src + " to " + destFile));
                            else
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying file from " + src + " to " + destFile));
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = fco.MetaHash[i] == fco.Hash[i] ? FinalState.Unchanged : FinalState.Created;
                    }
                }
            }
            fco.FinalState[srcFilePos] = (fco.MetaExists[srcFilePos] && fco.MetaHash[srcFilePos] == fco.Hash[srcFilePos]) ? FinalState.Unchanged : FinalState.Created;
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
                            {
                                CommonMethods.ArchiveFile(destFile, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "File archived " + destFile));
                            }
                        }
                        catch (ArchiveFileException)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error archiving file " + destFile));
                        }

                        try
                        {
                            if (_syncConfig.Recycle)
                                CommonMethods.DeleteFileToRecycleBin(destFile);
                            else
                                CommonMethods.DeleteFile(destFile);

                            fco.Exists[i] = false;
                            fco.FinalState[i] = FinalState.Deleted;
                            changed = true;

                            if (_syncConfig.Recycle)
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "File deleted to recycle bin " + destFile));
                            else
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "File deleted " + destFile));
                        }
                        catch (DeleteFileException)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error deleting file " + destFile));
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = fco.MetaExists[i] ? FinalState.Deleted : FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = changed ? FinalState.Deleted : FinalState.Unchanged;
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
                                fco.FinalState[i] = FinalState.Renamed;
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_RENAMED, "File renamed from " + oldName + " to " + newName));
                            }
                            else
                            {
                                CommonMethods.CopyFile(srcName, newName, true);
                                FileCompareObject srcFile = fco.Parent.GetChild(fco.NewName) as FileCompareObject;
                                fco.CreationTime[i] = File.GetCreationTime(newName).Ticks;
                                fco.LastWriteTime[i] = File.GetLastWriteTime(newName).Ticks;
                                fco.Exists[i] = true;
                                fco.Hash[i] = srcFile.Hash[srcFilePos];
                                fco.Length[i] = srcFile.Length[srcFilePos];
                                fco.FinalState[i] = FinalState.CreatedRenamed;
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "File copied from " + srcName + " to " + newName));
                            }
                            changed = true;
                        }
                        catch (MoveFileException)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error renaming file from " + oldName + " to " + newName));
                        }
                        catch (CopyFileException)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying file from " + srcName + " to " + newName));
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged; //Should not be able to hit here
                    }
                }
            }
            fco.FinalState[srcFilePos] = changed ? FinalState.Renamed : FinalState.Unchanged;
        }

        #endregion

        #region Folder Methods

        private void CreateFolder(FolderCompareObject folder, int numOfPaths, int srcFolderPos)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFolderPos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFolderPos])
                    {
                        string folderToCreate = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                        if (!Directory.Exists(folderToCreate))
                        {
                            try
                            {
                                CommonMethods.CreateFolder(folderToCreate);
                                folder.Exists[i] = true;
                                folder.CreationTime[i] = Directory.GetCreationTime(folderToCreate).Ticks;
                                folder.FinalState[i] = FinalState.Created;
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Folder created " + folderToCreate));
                            }
                            catch (CreateFolderException)
                            {
                                folder.FinalState[i] = FinalState.Error;
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error creating folder " + folderToCreate));
                            }
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = folder.MetaExists[i] ? FinalState.Unchanged : FinalState.Created;
                    }
                }
            }
            folder.FinalState[srcFolderPos] = folder.MetaExists[srcFolderPos] ? FinalState.Unchanged : FinalState.Created;
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
                            {
                                CommonMethods.ArchiveFolder(destFolder, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "Folder archived " + destFolder));
                            }
                        }
                        catch (ArchiveFolderException)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error archiving folder " + destFolder));
                        }

                        try
                        {
                            if (_syncConfig.Recycle)
                                CommonMethods.DeleteFolderToRecycleBin(destFolder);
                            else
                                CommonMethods.DeleteFolder(destFolder, true);

                            folder.Exists[i] = false;
                            folder.FinalState[i] = FinalState.Deleted;
                            folder.Contents.Clear(); //Experimental
                            changed = true;

                            if (_syncConfig.Recycle)
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "Folder deleted to recycle bin " + destFolder));
                            else
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "Folder deleted " + destFolder));
                        }
                        catch (DeleteFolderException)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error deleting folder " + destFolder));
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = folder.MetaExists[i] ? FinalState.Deleted : FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFolderPos] = changed ? FinalState.Deleted : FinalState.Unchanged;
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
                                folder.FinalState[i] = FinalState.Renamed;
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_RENAMED, "Folder renamed from " + oldFolderName + " to " + newFolderName));
                            }
                            else
                            {
                                CommonMethods.CopyDirectory(srcFolderName, newFolderName);
                                folder.Exists[i] = true;
                                folder.CreationTime[i] = Directory.GetCreationTime(newFolderName).Ticks;
                                folder.FinalState[i] = FinalState.CreatedRenamed;
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Folder copied from " + srcFolderName + " to " + newFolderName));
                            }
                            changed = true;
                        }
                        catch (MoveFolderException)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error renaming folder from " + oldFolderName + " to " + newFolderName));
                        }
                        catch (CopyFolderException)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying folder: " + srcFolderName + " to " + newFolderName));
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged; //Should not be able to reach here
                    }
                }
            }
            folder.FinalState[srcFolderPos] = changed ? FinalState.Renamed : FinalState.Unchanged;
        }

        #endregion

    }


}
