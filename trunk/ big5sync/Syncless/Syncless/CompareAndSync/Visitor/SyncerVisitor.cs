using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.Core;
using Syncless.Notification;

namespace Syncless.CompareAndSync.Visitor
{
    public class SyncerVisitor : IVisitor
    {
        #region IVisitor Members
        private SyncConfig _syncConfig;
        private SyncProgress _syncProgress;
        public SyncerVisitor(SyncConfig syncConfig,SyncProgress progress)
        {
            _syncConfig = syncConfig;
            _syncProgress = progress;
        }

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            _syncProgress.Message = "Synchronzing "+file.Name;
            if (file.Invalid)
            {
                _syncProgress.fail();
                return;
            }
                
            int maxPriorityPos = 0;
            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.Priority[i] > file.Priority[maxPriorityPos])
                    maxPriorityPos = i;
            }

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
            _syncProgress.Message = "Synchronzing " + folder.Name;
            if (folder.Invalid)
            {
                _syncProgress.fail();
                return;
            }

            int maxPriorityPos = 0;
            for (int i = 0; i < numOfPaths; i++)
            {
                if (folder.Priority[i] > folder.Priority[maxPriorityPos])
                    maxPriorityPos = i;
            }

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
            _syncProgress.complete();//Do nothing
        }

        #endregion

        #region File Methods

        private void CopyFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            string src = Path.Combine(fco.GetSmartParentPath(srcFilePos), fco.Name);

            bool fileExists = false;
            string destFile = null;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos && fco.Parent.FinalState[i] != FinalState.Deleted)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        try
                        {
                            destFile = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                            fileExists = File.Exists(destFile);

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
                            if (fileExists)
                                fco.FinalState[i] = FinalState.Updated;
                            else
                                fco.FinalState[i] = FinalState.Created;
                            fco.Hash[i] = fco.Hash[srcFilePos];
                            fco.LastWriteTime[i] = fco.LastWriteTime[srcFilePos];
                            fco.Length[i] = fco.LastWriteTime[srcFilePos];
                        }
                        catch (ArchiveFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                        catch (CopyFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = FinalState.Propagated;
        }

        private void DeleteFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            string destFile = null;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        try
                        {
                            destFile = Path.Combine(fco.GetSmartParentPath(i), fco.Name);

                            if (_syncConfig.ArchiveLimit >= 0)
                                CommonMethods.ArchiveFile(destFile, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                            if (_syncConfig.Recycle)
                                CommonMethods.DeleteFileToRecycleBin(destFile);
                            else
                                CommonMethods.DeleteFile(destFile);

                            fco.Exists[i] = false;
                            fco.FinalState[i] = FinalState.Deleted;
                        }
                        catch (ArchiveFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);

                        }
                        catch (DeleteFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }

                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = FinalState.Propagated;
        }

        private void MoveFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        try
                        {
                            if (File.Exists(Path.Combine(fco.GetSmartParentPath(i), fco.Name)))
                            {
                                CommonMethods.MoveFile(Path.Combine(fco.GetSmartParentPath(i), fco.Name), Path.Combine(fco.GetSmartParentPath(i), fco.NewName));
                                fco.FinalState[i] = FinalState.Renamed;
                            }
                            else
                            {
                                CommonMethods.CopyFile(Path.Combine(fco.GetSmartParentPath(srcFilePos), fco.NewName), Path.Combine(fco.GetSmartParentPath(i), fco.NewName), true);
                                fco.FinalState[i] = FinalState.Created;
                            }

                        }
                        catch (MoveFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = FinalState.Propagated;
        }

        #endregion

        #region Folder Methods

        private void CreateFolder(FolderCompareObject folder, int numOfPaths, int srcFilePos)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFilePos])
                    {
                        if (!Directory.Exists(Path.Combine(folder.GetSmartParentPath(i), folder.Name)))
                        {
                            try
                            {
                                CommonMethods.CreateFolder(Path.Combine(folder.GetSmartParentPath(i), folder.Name));
                                folder.Exists[i] = true;
                                folder.FinalState[i] = FinalState.Created;
                            }
                            catch (CreateFolderException e)
                            {
                                folder.FinalState[i] = FinalState.Error;
                                //TODO: Throw to notification queue in future
                                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                            }
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFilePos] = FinalState.Propagated;

        }

        private void DeleteFolder(FolderCompareObject folder, int numOfPaths, int srcFilePos)
        {
            string destFolder = null;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFilePos])
                    {
                        try
                        {
                            destFolder = Path.Combine(folder.GetSmartParentPath(i), folder.Name);

                            if (_syncConfig.ArchiveLimit >= 0)
                                CommonMethods.ArchiveFolder(destFolder, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                            if (_syncConfig.Recycle)
                                CommonMethods.DeleteFolderToRecycleBin(destFolder);
                            else
                                CommonMethods.DeleteFolder(destFolder, true);

                            folder.Exists[i] = false;
                            folder.FinalState[i] = FinalState.Deleted;
                            folder.Contents.Clear(); //Experimental
                        }
                        catch (ArchiveFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                        catch (DeleteFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFilePos] = FinalState.Propagated;
        }

        private void MoveFolder(FolderCompareObject folder, int numOfPaths, int srcFolderPos)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFolderPos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFolderPos])
                    {
                        try
                        {
                            string oldFolderName = Path.Combine(folder.GetSmartParentPath(i), folder.Name);

                            if (Directory.Exists(oldFolderName))
                            {
                                CommonMethods.MoveFolder(oldFolderName, Path.Combine(folder.GetSmartParentPath(i), folder.NewName));
                                folder.FinalState[i] = FinalState.Renamed;
                            }
                            else
                            {
                                CommonMethods.CopyDirectory(Path.Combine(folder.GetSmartParentPath(srcFolderPos), folder.NewName), Path.Combine(folder.GetSmartParentPath(i), folder.NewName));
                                folder.FinalState[i] = FinalState.Created;
                            }

                        }
                        catch (MoveFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFolderPos] = FinalState.Propagated;
        }

        #endregion

    }


}
