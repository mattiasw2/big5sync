using System;
using System.Collections.Generic;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    /// <summary>
    /// ComparerVisitor is responsible for comparing and updating the states of the files and folders.
    /// </summary>
    public class ComparerVisitor : IVisitor
    {
        private int _totalNodes;

        /// <summary>
        /// Instantiates a <c>ComparerVisitor</c>, and sets the total nodes to 0.
        /// </summary>
        public ComparerVisitor()
        {
            _totalNodes = 0;
        }

        #region IVisitor Members

        /// <summary>
        /// Visit implementation for <see cref="FileCompareObject"/>.
        /// </summary>
        /// <param name="file">The <see cref="FileCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to keep in sync.</param>
        public void Visit(FileCompareObject file, int numOfPaths)
        {
            if (file.Invalid)
                return;

            DetectFileRename(file, numOfPaths);
            CompareFiles(file, numOfPaths);
            _totalNodes++;
        }

        /// <summary>
        /// Visit implementation for <see cref="FolderCompareObject"/>.
        /// </summary>
        /// <param name="folder">The <see cref="FolderCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to keep in sync.</param>
        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            if (folder.Invalid)
                return;

            CompareFolders(folder, numOfPaths);
            _totalNodes++;
        }

        // Simply increase the node count
        public void Visit(RootCompareObject root)
        {
            _totalNodes++;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the total number of nodes.
        /// </summary>
        public int TotalNodes
        {
            get { return _totalNodes; }
        }

        #endregion

        #region Files

        /// <summary>
        /// Detects possible file renames and handles them accordingly.
        /// </summary>
        /// <param name="file">The <see cref="FileCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        private static void DetectFileRename(FileCompareObject file, int numOfPaths)
        {
            FileCompareObject result = null;
            Dictionary<string, List<int>> newNames = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            List<int> unchangedIndexes = new List<int>();

            for (int i = 0; i < numOfPaths; i++)
            {
                switch (file.ChangeType[i])
                {
                    case MetaChangeType.New:
                    case MetaChangeType.Update:
                        return; // Return as long as one of the meta change type is new or updated.
                }
            }

            for (int i = 0; i < numOfPaths; i++)
            {
                switch (file.ChangeType[i])
                {
                    case MetaChangeType.Delete:
                        // All deletes are also possible renames, thus we call GetIdenticalFile to look for an identical file with a different name, if any
                        FileCompareObject f = file.Parent.GetIdenticalFile(file.Name, file.MetaHash[i], file.MetaCreationTimeUtc[i], i);
                        
                        // If an identical file is found
                        if (f != null)
                        {
                            List<int> pos;
                            if (newNames.TryGetValue(f.Name, out pos))
                            {
                                pos.Add(i); // Add the position to the existing list of names
                            }
                            else
                            {
                                // Create a new list to add the position if the name does not currently exist
                                pos = new List<int>();
                                pos.Add(i);
                                newNames.Add(f.Name, pos);
                                result = f; // Point result to f
                            }
                        }
                        break;
                    case MetaChangeType.NoChange:
                        unchangedIndexes.Add(i);
                        break;
                }
            }

            if (newNames.Count == 1) // Meaning only one new/possible rename was detected, proceed with the rename.
            {
                file.NewName = result.Name; // Set the new name of this file to the name of the found file.

                foreach (int i in newNames[result.Name])
                    file.ChangeType[i] = MetaChangeType.Rename; // Set all the positions to change type rename.

                result.Invalid = true; // Set the found file to invalid, no need to process it.
                file.Parent.Dirty = true; // Set the current file's parent to dirty.
            }
            else if (newNames.Count > 1) // Meaning multiple renames are detected, void all the renames and treat each type as new.
            {
                foreach (int i in unchangedIndexes)
                    file.ChangeType[i] = MetaChangeType.New; // Set all the unchanged to new since they will be treated as new,
            }

        }

        /// <summary>
        /// Compares files to propagate updates and creations.
        /// </summary>
        /// <param name="file">The <see cref="FileCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        private static void CompareFiles(FileCompareObject file, int numOfPaths)
        {
            bool isRenamed = FileRenameHelper(file, numOfPaths);
            if (isRenamed)
                return; // Exit the method since a rename has been detected and handled for this file.

            bool isDeleted = FileDeleteHelper(file, numOfPaths);
            if (isDeleted)
                return; // Exit the method since a delete has been detected and handled for this file.

            FileCreateUpdateHelper(file, numOfPaths);
            SetFileParentDirty(file, numOfPaths);
        }

        #endregion

        #region File Handlers

        // Helper method to handle file renames.
        private static bool FileRenameHelper(FileCompareObject file, int numOfPaths)
        {
            int renamePos = -1;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Rename)
                {
                    if (renamePos == -1)
                    {
                        renamePos = i; // Set rename position to i.
                        file.SourcePosition = renamePos; // Set the source position of the file to be the rename position.
                    }
                    file.Priority[i] = 1; // Set the file priority to 1 if change type is rename.
                }
            }

            return renamePos > -1 ? true : false; // Return true only if renamePos has been set, i.e., more than -1.
        }

        // Helper method to handle file deletes.
        private static bool FileDeleteHelper(FileCompareObject file, int numOfPaths)
        {
            List<int> deletePos = new List<int>();
            bool stop = false;
            for (int i = 0; i < numOfPaths; i++)
            {
                if (stop)
                    break;

                if (file.ChangeType[i] == MetaChangeType.Delete)
                {
                    deletePos.Add(i);
                    for (int j = 0; j < numOfPaths; j++)
                    {
                        if (file.Exists[j])
                        {
                            if (file.MetaUpdated[j] > file.MetaUpdated[i])
                            {
                                deletePos.Clear();
                                stop = true;
                                break;
                            }
                        }
                    }
                }
                else if (file.ChangeType[i] != MetaChangeType.NoChange && file.ChangeType[i] != null)
                {
                    deletePos.Clear();
                    break;
                }
            }

            if (deletePos.Count > 0)
            {
                file.SourcePosition = deletePos[0];
                foreach (int i in deletePos)
                    file.Priority[i] = 1;
                return true;
            }
            return false;
        }

        private static void FileCreateUpdateHelper(FileCompareObject file, int numOfPaths)
        {
            int mostUpdatedPos = 0;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.Exists[i])
                {
                    mostUpdatedPos = i;
                    break;
                }
            }

            file.Priority[mostUpdatedPos] = 1;

            for (int i = mostUpdatedPos + 1; i < numOfPaths; i++)
            {
                if (!file.Exists[i])
                {
                    file.Priority[i] = -1;
                    continue;
                }

                if (file.Length[mostUpdatedPos] != file.Length[i] || file.Hash[mostUpdatedPos] != file.Hash[i])
                {
                    if (file.LastWriteTimeUtc[i] > file.LastWriteTimeUtc[mostUpdatedPos])
                    {
                        file.Priority[i] = file.Priority[mostUpdatedPos] + 1;
                        mostUpdatedPos = i;
                    }
                    else if (file.LastWriteTimeUtc[i] == file.LastWriteTimeUtc[mostUpdatedPos])
                    {
                        //Conflict
                        file.Priority[i] = file.Priority[mostUpdatedPos] - 1;
                        file.ConflictPositions.Add(i);
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CONFLICT, "Conflicted file detected " + Path.Combine(file.GetSmartParentPath(i), file.Name)));
                    }
                }
                else
                {
                    file.Priority[i] = file.Priority[mostUpdatedPos];
                }
            }

            file.SourcePosition = mostUpdatedPos;
        }

        private static void SetFileParentDirty(FileCompareObject file, int numOfPaths)
        {
            int numExists = 0;
            int posExists = -1;
            SetFileSystemObjectDirty(file, numOfPaths, ref numExists, ref posExists);

            if (numExists == 1)
            {
                if (file.ChangeType[posExists] == MetaChangeType.New || file.ChangeType[posExists] == MetaChangeType.Update)
                    file.Parent.Dirty = true;
            }
        }

        #endregion

        #region Folders

        private static void CompareFolders(FolderCompareObject folder, int numOfPaths)
        {
            bool isRenamed = FolderRenameHelper(folder, numOfPaths);
            if (isRenamed)
                return;

            bool isDeleted = FolderDeleteHelper(folder, numOfPaths);
            if (isDeleted)
                return;

            FolderCreateUpdateHelper(folder, numOfPaths);
            SetFolderParentDirty(folder, numOfPaths);
        }

        #endregion

        #region Folder Handlers

        private static bool FolderRenameHelper(FolderCompareObject folder, int numOfPaths)
        {
            //Rename will only occur if all other changes are MetaChangeType.NoChange or null
            int renamePos = -1;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (folder.ChangeType[i] == MetaChangeType.Rename)
                {
                    if (renamePos == -1)
                    {
                        renamePos = i;
                        folder.SourcePosition = renamePos;
                    }
                    folder.Priority[i] = 1;
                }
            }

            return renamePos > -1 ? true : false;
        }

        private static bool FolderDeleteHelper(FolderCompareObject folder, int numOfPaths)
        {
            //Delete will only occur if none of the folders are marked as dirty
            if (!folder.Dirty)
            {
                List<int> deletePos = new List<int>();
                bool stop = false;
                for (int i = 0; i < numOfPaths; i++)
                {
                    if (stop)
                        break;

                    if (folder.ChangeType[i] == MetaChangeType.Delete)
                    {
                        deletePos.Add(i);

                        for (int j = 0; j < numOfPaths; j++)
                        {
                            if (folder.Exists[j])
                            {
                                if (folder.MetaUpdated[j] > folder.MetaUpdated[i])
                                {
                                    deletePos.Clear();
                                    stop = true;
                                    break;
                                }
                            }
                        }
                    }
                    else if (folder.ChangeType[i] != MetaChangeType.NoChange && folder.ChangeType[i] != null)
                    {
                        deletePos.Clear();
                        break;
                    }
                }

                if (deletePos.Count > 0)
                {
                    folder.SourcePosition = deletePos[0];
                    folder.Contents.Clear(); //EXP
                    foreach (int i in deletePos)
                        folder.Priority[i] = 1;
                    return true;
                }
            }
            return false;
        }

        private static void FolderCreateUpdateHelper(FolderCompareObject folder, int numOfPaths)
        {
            int mostUpdatedPos = 0;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (folder.Exists[i])
                {
                    mostUpdatedPos = i;
                    break;
                }
            }

            folder.Priority[mostUpdatedPos] = 1;

            for (int i = mostUpdatedPos + 1; i < numOfPaths; i++)
            {
                if (!folder.Exists[i])
                {
                    folder.Priority[i] = -1;
                    continue;
                }
                folder.Priority[i] = folder.Priority[mostUpdatedPos];
            }

            folder.SourcePosition = mostUpdatedPos;
        }

        private static void SetFolderParentDirty(FolderCompareObject folder, int numOfPaths)
        {
            int numExists = 0;
            int posExists = -1;
            SetFileSystemObjectDirty(folder, numOfPaths, ref numExists, ref posExists);

            if (numExists == 1)
            {
                if (folder.ChangeType[posExists] == MetaChangeType.New)
                    folder.Parent.Dirty = true;
            }
        }

        #endregion

        private static void SetFileSystemObjectDirty(BaseCompareObject file, int numOfPaths, ref int numExists, ref int posExists)
        {
            int priority = -1;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.Exists[i])
                {
                    numExists++;
                    posExists = i;
                    if (priority < 0)
                        priority = file.Priority[i];
                    else
                    {
                        if (priority != file.Priority[i])
                        {
                            file.Parent.Dirty = true;
                            break;
                        }
                    }
                }
            }
        }

    }
}