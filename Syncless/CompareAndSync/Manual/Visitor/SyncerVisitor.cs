/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core;
using Syncless.Notification;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    /// <summary>
    /// <c>SyncerVisitor</c> is in charge of visiting the tree and synchronizing files after <see cref="ComparerVisitor"/> has updated the state of the tree.
    /// </summary>
    public class SyncerVisitor : IVisitor
    {
        private readonly SyncConfig _syncConfig;
        private readonly Progress _syncProgress;
        private int _nodesCount;

        /// <summary>
        /// Instantiates an instance of <c>SyncerVisitor</c> with the sync configuration and progress object passed in.
        /// </summary>
        /// <param name="syncConfig">The sync configuration to pass in.</param>
        /// <param name="progress">The progress object to pass in.</param>
        public SyncerVisitor(SyncConfig syncConfig, Progress progress)
        {
            _syncConfig = syncConfig;
            _syncProgress = progress;
        }

        #region IVisitor Members

        /// <summary>
        /// Visit implementaton for <see cref="FileCompareObject"/>.
        /// </summary>
        /// <param name="file">The <see cref="FileCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        public void Visit(FileCompareObject file, int numOfPaths)
        {
            _nodesCount++;
            _syncProgress.Message = file.Name;
            if (file.Invalid)
            {
                _syncProgress.Fail();
                return;
            }

            int maxPriorityPos = file.SourcePosition; // Get the position of the source file.

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
            _syncProgress.Complete();
        }

        /// <summary>
        /// Visit implementaton for <see cref="FolderCompareObject"/>.
        /// </summary>
        /// <param name="folder">The <see cref="FolderCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            _nodesCount++;
            _syncProgress.Message = folder.Name;

            if (folder.Invalid)
            {
                _syncProgress.Fail();
                return;
            }

            int maxPriorityPos = folder.SourcePosition; // Get the position of the source file.

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
            _syncProgress.Complete();
        }

        /// <summary>
        /// The <see cref="RootCompareObject"/> to visit.
        /// </summary>
        /// <param name="root">The <see cref="RootCompareObject"/> to process.</param>
        public void Visit(RootCompareObject root)
        {
            _nodesCount++;
            _syncProgress.Complete(); //Do nothing
        }

        #endregion

        #region File Methods

        /// <summary>
        /// Gets the total number of nodes.
        /// </summary>
        public int NodesCount
        {
            get { return _nodesCount; }
        }

        private void CopyFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            string src = Path.Combine(fco.GetSmartParentPath(srcFilePos), fco.Name);

            // Loop through all the files under this node.
            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos && fco.Parent.FinalState[i] != FinalState.Deleted)
                {
                    // Only process if the priority is not equal (implies lower priority actually).
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        string destFile = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                        bool fileExists = fco.Exists[i];

                        try
                        {
                            if (fileExists)
                            {
                                if (_syncConfig.ArchiveLimit > 0)
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
                            CommonMethods.CopyFile(src, destFile);

                            fco.CreationTimeUtc[i] = File.GetCreationTimeUtc(destFile).Ticks; // Creation time will be different since it's a new file, thus we get it from the actual file.
                            fco.LastWriteTimeUtc[i] = File.GetLastWriteTimeUtc(destFile).Ticks; // Last modified/write time may be different, thus we get it from the actual file.

                            // Duplicate source file information to the destination
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
                        // Set FinalState to Unchanged if the hash in the metadata is the same as the actual file.
                        // Otherwise, it is possible that it is the exact same file as the source file, but was not saved to
                        // meta previously, thus we set the FinalState to created.
                        fco.FinalState[i] = (fco.MetaHash[i] == fco.Hash[i]) ? FinalState.Unchanged : FinalState.Created;
                    }
                }
            }
            // Set the FinalState to Unchanged if it already exists in metadata, and the hash in the metadata is the same as that of the actual file.
            // Otherwise set it to Created.
            fco.FinalState[srcFilePos] = (fco.MetaExists[srcFilePos] && fco.MetaHash[srcFilePos] == fco.Hash[srcFilePos]) ? FinalState.Unchanged : FinalState.Created;
        }

        private void DeleteFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            bool changed = false;

            // Loop through all the files under this node.
            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos && fco.Priority[i] != fco.Priority[srcFilePos])
                {
                    string destFile = Path.Combine(fco.GetSmartParentPath(i), fco.Name);

                    try
                    {
                        if (_syncConfig.ArchiveLimit > 0)
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

                        fco.Exists[i] = false; // Update the state of the node after it has been deleted.
                        fco.FinalState[i] = FinalState.Deleted; //Set the FinalState to deleted.
                        changed = true; // Since at least one file has changed, set changed to true.

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
                else // If the priority is equal, it will also mean that the file at position i has been deleted.
                {
                    fco.FinalState[i] = fco.MetaExists[i] ? FinalState.Deleted : FinalState.Unchanged; // If meta exists, set the change type to deleted. Otherwise set it as unchanged.
                    changed = true;
                }
            }
            fco.FinalState[srcFilePos] = changed ? FinalState.Deleted : FinalState.Unchanged; // If at least one file has been deleted (changed), set the finalstate of the source file to deleted. Otherwise set it as unchanged.
        }

        private void MoveFile(FileCompareObject fco, int numOfPaths, int srcFilePos)
        {
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFilePos && fco.Priority[i] != fco.Priority[srcFilePos])
                {
                    string oldName = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                    string newName = Path.Combine(fco.GetSmartParentPath(i), fco.NewName);
                    string srcName = Path.Combine(fco.GetSmartParentPath(srcFilePos), fco.NewName);

                    try
                    {
                        if (File.Exists(oldName)) // If the old file exists, simply rename it.
                        {
                            CommonMethods.MoveFile(oldName, newName);
                            fco.FinalState[i] = FinalState.Renamed;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_RENAMED, "File renamed from " + oldName + " to " + newName));
                        }
                        else // If the old file does not exists, copy the source file over instead.
                        {
                            CommonMethods.CopyFile(srcName, newName);
                            FileCompareObject srcFile = fco.Parent.GetChild(fco.NewName) as FileCompareObject;
                            fco.CreationTimeUtc[i] = File.GetCreationTimeUtc(newName).Ticks;
                            fco.LastWriteTimeUtc[i] = File.GetLastAccessTimeUtc(newName).Ticks;
                            fco.Exists[i] = true;
                            fco.Hash[i] = srcFile.Hash[srcFilePos];
                            fco.Length[i] = srcFile.Length[srcFilePos];
                            fco.FinalState[i] = FinalState.CreatedRenamed;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "File copied from " + srcName + " to " + newName));
                        }

                        changed = true; // Set changed to true since the change has propagated to at least one file
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
                    fco.FinalState[i] = FinalState.Renamed; // Set the FinalState to renamed if the file has the same priority, implying it has the same name as the new file name
                    changed = true;
                }
            }
            fco.FinalState[srcFilePos] = changed ? FinalState.Renamed : FinalState.Unchanged; // Set the FinalState to renamed if at least one file has changed. Otherwise, set it as unchanged.
        }

        #endregion

        #region Folder Methods

        private void CreateFolder(FolderCompareObject folder, int numOfPaths, int srcFolderPos)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFolderPos && folder.Priority[i] != folder.Priority[srcFolderPos])
                {
                    string folderToCreate = Path.Combine(folder.GetSmartParentPath(i), folder.Name);

                    if (!Directory.Exists(folderToCreate))
                    {
                        try
                        {
                            CommonMethods.CreateFolder(folderToCreate); // Create the folder since it does not exist
                            folder.Exists[i] = true; // Update the state of the folder
                            folder.CreationTimeUtc[i] = Directory.GetCreationTimeUtc(folderToCreate).Ticks;
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
                    folder.FinalState[i] = folder.MetaExists[i] ? FinalState.Unchanged : FinalState.Created; // Set the FinalState to unchanged if the metadata already exists, else set it to created.
                }
            }
            folder.FinalState[srcFolderPos] = folder.MetaExists[srcFolderPos] ? FinalState.Unchanged : FinalState.Created; // Set the FinalState to unchanged if the metadata exists, else set it to created.
        }

        private void DeleteFolder(FolderCompareObject folder, int numOfPaths, int srcFolderPos)
        {
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFolderPos && folder.Priority[i] != folder.Priority[srcFolderPos])
                {
                    string destFolder = Path.Combine(folder.GetSmartParentPath(i), folder.Name);

                    try
                    {
                        if (_syncConfig.ArchiveLimit > 0)
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
                            CommonMethods.DeleteFolder(destFolder);

                        folder.Exists[i] = false;
                        folder.FinalState[i] = FinalState.Deleted;
                        changed = true; // Set it to true since the change has propaged to at least one folder

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
                    changed = true; // Set it to true since the change has propaged to at least one folder
                }

            }
            folder.FinalState[srcFolderPos] = changed ? FinalState.Deleted : FinalState.Unchanged; // Set the FinalState to deleted if changed is true, otherwise set it as Unchanged.
        }

        private void MoveFolder(FolderCompareObject folder, int numOfPaths, int srcFolderPos)
        {
            bool changed = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (i != srcFolderPos && folder.Priority[i] != folder.Priority[srcFolderPos])
                {
                    string oldFolderName = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                    string newFolderName = Path.Combine(folder.GetSmartParentPath(i), folder.NewName);
                    string srcFolderName = Path.Combine(folder.GetSmartParentPath(srcFolderPos), folder.NewName);

                    try
                    {
                        if (Directory.Exists(oldFolderName))
                        {
                            CommonMethods.MoveFolder(oldFolderName, newFolderName); // Rename the old folder to the new name if it exists.
                            folder.FinalState[i] = FinalState.Renamed;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_RENAMED, "Folder renamed from " + oldFolderName + " to " + newFolderName));
                        }
                        else
                        {
                            CommonMethods.CopyDirectory(srcFolderName, newFolderName); // Copy the directory from the source folder if the old folder does not exist.
                            folder.Exists[i] = true;
                            folder.CreationTimeUtc[i] = Directory.GetCreationTimeUtc(newFolderName).Ticks;
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
                    folder.FinalState[i] = FinalState.Renamed;
                    changed = true;
                }
            }
            folder.FinalState[srcFolderPos] = changed ? FinalState.Renamed : FinalState.Unchanged; // If changed is true, that means at least one file has been affected, set the FinalState to renamed. Otherwise, leave it as Unchanged.
        }

        #endregion

    }
}