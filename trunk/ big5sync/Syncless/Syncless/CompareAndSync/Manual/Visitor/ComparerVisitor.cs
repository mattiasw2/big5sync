using System.Collections.Generic;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    public class ComparerVisitor : IVisitor
    {
        #region IVisitor Members
        private int _totalNodes;

        public int TotalNodes
        {
            get { return _totalNodes; }
            set { _totalNodes = value; }
        }
        public ComparerVisitor()
        {
            _totalNodes = 0;

        }
        public void Visit(FileCompareObject file, int numOfPaths)
        {
            if (file.Invalid)
                return;

            DetectFileRename(file, numOfPaths);
            //DetectFileRenameAndUpdate(file, numOfPaths);
            CompareFiles(file, numOfPaths);
            _totalNodes++;
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            if (folder.Invalid)
                return;

            CompareFolders(folder, numOfPaths);
            _totalNodes++;
        }

        public void Visit(RootCompareObject root)
        {
            // Do nothing
            _totalNodes++;
        }

        #endregion

        #region Files

        private static void DetectFileRenameAndUpdate(FileCompareObject file, int numOfPaths)
        {
            //Get a Delete type
            //1. Find something that is New and has the same creation time
            //2. Check that the New is null for all other indexes
            //3. If above is verified, check that Delete is NoChange or null for all other indexes
            //4. If all is verified, set the ChangeType to New.

            FileCompareObject f = null;
            List<int> indexes = new List<int>();

            for (int i = 0; i < numOfPaths; i++)
            {
                // ReSharper disable PossibleNullReferenceException
                if (file.ChangeType[i].HasValue && file.ChangeType[i] == MetaChangeType.Delete)
                    // ReSharper restore PossibleNullReferenceException
                    indexes.Add(i);
                else if (file.ChangeType[i] != MetaChangeType.NoChange && file.ChangeType != null)
                    return;
            }

            if (indexes.Count < 1)
                return;

            for (int i = 0; i < indexes.Count; i++)
            {
                f = file.Parent.GetSameCreationTime(file.MetaCreationTime[indexes[i]], indexes[i]);

                if (f != null && f.ChangeType[indexes[i]] == MetaChangeType.New)
                {
                    bool found = true;
                    for (int j = 0; j < f.ChangeType.Length; j++)
                    {
                        if (j != indexes[i] && f.ChangeType[j] != null)
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        // ReSharper disable PossibleNullReferenceException
                        file.ChangeType[indexes[i]] = null; //TODO: Unchanged?
                    // ReSharper restore PossibleNullReferenceException

                }
            }

        }

        private static void DetectFileRename(FileCompareObject file, int numOfPaths)
        {
            FileCompareObject result = null;
            List<int> indexes = new List<int>();

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.New || file.ChangeType[i] == MetaChangeType.Update)
                    return;
            }

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Delete)
                {
                    FileCompareObject f = file.Parent.GetIdenticalFile(file.Name, file.MetaHash[i], file.MetaCreationTime[i], i);

                    if (f != null)
                    {
                        indexes.Add(i);
                        result = f;
                    }
                }
            }

            if (indexes.Count == 1)
            {
                // ReSharper disable PossibleNullReferenceException
                file.NewName = result.Name;
                // ReSharper restore PossibleNullReferenceException
                file.ChangeType[indexes[0]] = MetaChangeType.Rename;
                result.Invalid = true;
                file.Parent.Dirty = true; //Experimental
            }
            else if (indexes.Count > 1) //More than 2 renames detected
            {
                foreach (int i in indexes)
                    file.ChangeType[i] = null;
            }

        }

        private static void CompareFiles(FileCompareObject file, int numOfPaths)
        {
            //Rename will only occur if all other changes are MetaChangeType.NoChange or null
            int renamePos = -1;
            int count = 0;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Rename)
                {
                    renamePos = i;
                    count++;
                }
                else if (file.ChangeType[i] != MetaChangeType.NoChange && file.ChangeType[i] != null && file.ChangeType[i] != MetaChangeType.Delete)
                {
                    renamePos = -1;
                    break;
                }
            }

            if (renamePos > -1 && count == 1)
            {
                file.Priority[renamePos] = 1;
                file.SourcePosition = renamePos;
                return;
            }

            //Delete will only occur if all other changes are MetaChangeType.NoChange or null
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
                return;
            }

            //Update/Create handled in a similar way
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
                    if (file.LastWriteTime[i] > file.LastWriteTime[mostUpdatedPos])
                    {
                        file.Priority[i] = file.Priority[mostUpdatedPos] + 1;
                        mostUpdatedPos = i;
                    }
                    else if (file.LastWriteTime[i] == file.LastWriteTime[mostUpdatedPos])
                    {
                        //Conflict
                        file.Priority[i] = file.Priority[mostUpdatedPos] - 1;
                        file.FinalState[i] = FinalState.Conflict;
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

            //When to set parent to dirty? When the priority is not the same for all files?
            int priority = -1;
            int numExists = 0;
            int posExists = -1;
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
            //Rename will only occur if all other changes are MetaChangeType.NoChange or null
            int renamePos = -1;
            int count = 0;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (folder.ChangeType[i] == MetaChangeType.Rename)
                {
                    renamePos = i;
                    count++;
                }
                else if (folder.ChangeType[i] != MetaChangeType.NoChange && folder.ChangeType[i] != MetaChangeType.Delete && folder.ChangeType[i] != null)
                {
                    renamePos = -1;
                    break;
                }
            }

            if (renamePos > -1 && count == 1)
            {
                folder.Priority[renamePos] = 1;
                folder.SourcePosition = renamePos;
                return;
            }

            //Delete will only occur if none of the folders are marked as dirty
            List<int> deletePos = new List<int>();

            if (!folder.Dirty)
            {
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
            }

            if (deletePos.Count > 0)
            {
                folder.SourcePosition = deletePos[0];
                foreach (int i in deletePos)
                    folder.Priority[i] = 1;
                return;
            }

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

        #endregion

    }
}