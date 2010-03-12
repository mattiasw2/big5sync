using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    public class SyncerVisitor : IVisitor
    {
        #region IVisitor Members
        private SyncConfig _syncConfig;

        public SyncerVisitor(SyncConfig syncConfig)
        {
            _syncConfig = syncConfig;
        }

        public void Visit(FileCompareObject file, string[] currentPaths)
        {
            if (file.Invalid)
                return;

            int maxPriorityPos = 0;
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.Priority[i] > file.Priority[maxPriorityPos])
                    maxPriorityPos = i;
            }

            if (file.Priority[maxPriorityPos] > 0)
            {
                switch (file.ChangeType[maxPriorityPos])
                {
                    case MetaChangeType.Delete:
                        DeleteFile(file, currentPaths, maxPriorityPos);
                        break;
                    case MetaChangeType.New:
                    case MetaChangeType.Update:
                    case MetaChangeType.NoChange:
                        CopyFile(file, currentPaths, maxPriorityPos);
                        break;

                    case MetaChangeType.Rename:
                        MoveFile(file, currentPaths, maxPriorityPos);
                        break;
                }
            }

            //Basic logic: Look for highest priority and propagate it.

        }

        public void Visit(FolderCompareObject folder, string[] currentPaths)
        {
            if (folder.Invalid)
                return;

            int maxPriorityPos = 0;
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (folder.Priority[i] > folder.Priority[maxPriorityPos])
                    maxPriorityPos = i;
            }

            if (folder.Priority[maxPriorityPos] > 0)
            {
                switch (folder.ChangeType[maxPriorityPos])
                {
                    case MetaChangeType.Delete:
                        DeleteFolder(folder, currentPaths, maxPriorityPos);
                        break;
                    case MetaChangeType.New:
                        //case MetaChangeType.NoChange:
                        CreateFolder(folder, currentPaths, maxPriorityPos);
                        break;
                }
            }
        }

        public void Visit(RootCompareObject root)
        {
            //Do nothing
        }

        #endregion

        #region File Methods

        private void CopyFile(FileCompareObject fco, string[] currentPaths, int srcFilePos)
        {
            string src = Path.Combine(currentPaths[srcFilePos], fco.Name);
            bool fileExists = false;
            string destFile = null;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos && fco.Parent.FinalState[i] != FinalState.Deleted)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        destFile = Path.Combine(currentPaths[i], fco.Name);
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
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = FinalState.Propagated;
        }

        private void DeleteFile(FileCompareObject fco, string[] currentPaths, int srcFilePos)
        {
            string destFile = null;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        destFile = Path.Combine(currentPaths[i], fco.Name);

                        if (_syncConfig.ArchiveLimit >= 0)
                            CommonMethods.ArchiveFile(destFile, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                        if (_syncConfig.Recycle)
                            CommonMethods.DeleteFileToRecycleBin(destFile);
                        else
                            CommonMethods.DeleteFile(destFile);

                        fco.Exists[i] = false;
                        fco.FinalState[i] = FinalState.Deleted;

                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = FinalState.Propagated;
        }

        private void MoveFile(FileCompareObject fco, string[] currentPaths, int srcFilePos)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        CommonMethods.MoveFile(Path.Combine(currentPaths[i], fco.Name), Path.Combine(currentPaths[i], fco.NewName));
                        fco.FinalState[i] = FinalState.Renamed;
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

        private void CreateFolder(FolderCompareObject folder, string[] currentPaths, int srcFilePos)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFilePos])
                    {
                        if (!Directory.Exists(Path.Combine(currentPaths[i], folder.Name)))
                        {
                            CommonMethods.CreateFolder(Path.Combine(currentPaths[i], folder.Name));
                            folder.Exists[i] = true;
                            folder.FinalState[i] = FinalState.Created;
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

        private void DeleteFolder(FolderCompareObject folder, string[] currentPaths, int srcFilePos)
        {
            string destFolder = null;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFilePos])
                    {
                        destFolder = Path.Combine(currentPaths[i], folder.Name);

                        if (_syncConfig.ArchiveLimit >= 0)
                            CommonMethods.ArchiveFolder(destFolder, _syncConfig.ArchiveName, _syncConfig.ArchiveLimit);
                        if (_syncConfig.Recycle)
                            CommonMethods.DeleteFolderToRecycleBin(destFolder);
                        else
                            CommonMethods.DeleteFolder(destFolder, true);

                        folder.Exists[i] = false;
                        folder.FinalState[i] = FinalState.Deleted;
                        folder.Contents.Clear(); //Experimentald
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFilePos] = FinalState.Propagated;
        }

        #endregion

    }


}
