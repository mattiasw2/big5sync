using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;

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
                    case MetaChangeType.NoChange:
                        CreateFolder(folder, currentPaths, maxPriorityPos);
                        break;
                    //case MetaChangeType.Rename:
                    //    MoveFolder(folder, currentPaths, maxPriorityPos);
                    //    break;
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
            /*
            //Probable folder rename
            if (fco.Parent.FinalState[srcFilePos] == FinalState.Renamed)
            {
                currentPaths[srcFilePos] = fco.Parent.GetFullParentPath(srcFilePos);
            }*/

            string src = Path.Combine(currentPaths[srcFilePos], fco.Name);
            bool fileExists = false;
            string destFile = null;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos && fco.Parent.FinalState[i] != FinalState.Deleted)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        try
                        {
                            //if (fco.Parent.FinalState[srcFilePos] != FinalState.Renamed)
                                destFile = Path.Combine(currentPaths[i], fco.Name);
                            //else
                            //    destFile = fco.GetFullParentPath(i);
                            
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
                        }
                        catch (CopyFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
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

        private void DeleteFile(FileCompareObject fco, string[] currentPaths, int srcFilePos)
        {
            string destFile = null;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        try
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
                        catch (ArchiveFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
                        }
                        catch (DeleteFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
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

        private void MoveFile(FileCompareObject fco, string[] currentPaths, int srcFilePos)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        try
                        {
                            if (File.Exists(Path.Combine(currentPaths[i], fco.Name)))
                            {
                                CommonMethods.MoveFile(Path.Combine(currentPaths[i], fco.Name), Path.Combine(currentPaths[i], fco.NewName));
                                fco.FinalState[i] = FinalState.Renamed;
                            }
                            else
                            {
                                CommonMethods.CopyFile(Path.Combine(currentPaths[srcFilePos], fco.NewName), Path.Combine(currentPaths[i], fco.NewName), true);
                                fco.FinalState[i] = FinalState.Created;
                            }

                        }
                        catch (MoveFileException e)
                        {
                            fco.FinalState[i] = FinalState.Error;
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
                            try
                            {
                                CommonMethods.CreateFolder(Path.Combine(currentPaths[i], folder.Name));
                                folder.Exists[i] = true;
                                folder.FinalState[i] = FinalState.Created;
                            }
                            catch (CreateFolderException e)
                            {
                                folder.FinalState[i] = FinalState.Error;
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

        private void DeleteFolder(FolderCompareObject folder, string[] currentPaths, int srcFilePos)
        {
            string destFolder = null;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFilePos])
                    {
                        try
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
                            folder.Contents.Clear(); //Experimental
                        }
                        catch (ArchiveFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
                        }
                        catch (DeleteFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
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

        private void MoveFolder(FolderCompareObject folder, string[] currentPaths, int srcFolderPos)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFolderPos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFolderPos])
                    {
                        try
                        {
                            if (Directory.Exists(Path.Combine(currentPaths[i], folder.Name)))
                            {
                                CommonMethods.MoveFolder(Path.Combine(currentPaths[i], folder.Name), Path.Combine(currentPaths[i], folder.NewName));
                                folder.FinalState[i] = FinalState.Renamed;
                            }
                            else
                            {
                                CommonMethods.CopyDirectory(Path.Combine(currentPaths[srcFolderPos], folder.NewName), Path.Combine(currentPaths[i], folder.NewName));
                                folder.FinalState[i] = FinalState.Created;
                            }

                        }
                        catch (MoveFolderException e)
                        {
                            folder.FinalState[i] = FinalState.Error;
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
