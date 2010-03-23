using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    public class ComparerVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            if (file.Invalid)
                return;

            DetectFileRename(file, numOfPaths);
            DetectFileRenameAndUpdate(file, numOfPaths);
            CompareFiles(file, numOfPaths);
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            if (folder.Invalid)
                return;

            CompareFolders(folder, numOfPaths);
        }

        public void Visit(RootCompareObject root)
        {
            // Do nothing
        }

        #endregion

        #region Files

        private void DetectFileRenameAndUpdate(FileCompareObject file, int numOfPaths)
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
                if (file.ChangeType[i] == MetaChangeType.Delete)
                    indexes.Add(i);
                else if (file.ChangeType[i] != MetaChangeType.NoChange && file.ChangeType != null)
                    return;
            }

            if (indexes.Count < 1)
                return;

            bool found = true;

            for (int i = 0; i < indexes.Count; i++)
            {
                f = file.Parent.GetSameCreationTime(file.MetaCreationTime[indexes[i]], indexes[i]);

                if (f != null && f.ChangeType[indexes[i]] == MetaChangeType.New)
                {
                    found = true;
                    for (int j = 0; j < f.ChangeType.Length; j++)
                    {
                        if (j != indexes[i] && f.ChangeType[j] != null)
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        file.ChangeType[indexes[i]] = null; //TODO: Unchanged?

                }
            }

        }

        private void DetectFileRename(FileCompareObject file, int numOfPaths)
        {
            FileCompareObject f = null;
            FileCompareObject result = null;
            int resultPos = -1;
            int counter = 0;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.New || file.ChangeType[i] == MetaChangeType.Update)
                    return;
            }

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Delete)
                {
                    if (file.Todo[i].HasValue && file.Todo[i] == ToDo.Rename)
                    {
                        counter++;
                        resultPos = i;
                    }
                    else
                    {
                        f = file.Parent.GetIdenticalFile(file.Name, file.MetaHash[i], file.MetaCreationTime[i], i);

                        if (f != null)
                        {
                            counter++;
                            result = f;
                            resultPos = i;
                        }
                    }
                }
            }

            if (counter == 1)
            {
                if (result != null)
                {
                    file.NewName = result.Name;
                    file.ChangeType[resultPos] = MetaChangeType.Rename;
                    result.Invalid = true;
                }
                else
                {
                    file.NewName = file.TodoNewName[resultPos];
                    file.ChangeType[resultPos] = MetaChangeType.Rename;
                }
            }

        }

        private void CompareFiles(FileCompareObject file, int numOfPaths)
        {
            //Delete will only occur if all other changes are MetaChangeType.NoChange or null
            List<int> deletePos = new List<int>();

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Delete)
                    deletePos.Add(i);
                else if (file.ChangeType[i] != MetaChangeType.NoChange && file.ChangeType[i] != null)
                {
                    deletePos.Clear();
                    break;
                }
            }

            if (deletePos.Count > 0)
            {
                foreach (int i in deletePos)
                    file.Priority[i] = 1;
                return;
            }

            //Rename will only occur if all other changes are MetaChangeType.NoChange or null
            int renamePos = -1;

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Rename)
                    renamePos = i;
                else if (file.ChangeType[i] != MetaChangeType.NoChange && file.ChangeType[i] != null)
                {
                    renamePos = -1;
                    break;
                }
            }

            if (renamePos > -1)
            {
                file.Priority[renamePos] = 1;
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
                }
                else
                {
                    file.Priority[i] = file.Priority[mostUpdatedPos];
                }
            }

            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.Exists[i] && file.Priority[i] != file.Priority[mostUpdatedPos])
                {
                    file.Parent.Dirty = true;
                    break;
                }
            }
        }

        #endregion

        #region Folders

        private void CompareFolders(FolderCompareObject folder, int numOfFiles)
        {
            //Delete will only occur if none of the folders are marked as dirty
            List<int> deletePos = new List<int>();

            if (!folder.Dirty)
            {
                for (int i = 0; i < numOfFiles; i++)
                {
                    if (folder.ChangeType[i] == MetaChangeType.Delete)
                        deletePos.Add(i);
                }
            }

            if (deletePos.Count > 0)
            {
                foreach (int i in deletePos)
                    folder.Priority[i] = 1;
                return;
            }
            
            //Rename will only occur if all other changes are MetaChangeType.NoChange or null
            int renamePos = -1;

            for (int i = 0; i < numOfFiles; i++)
            {
                if (folder.ChangeType[i] == MetaChangeType.Rename)
                    renamePos = i;
                else if (folder.ChangeType[i] != MetaChangeType.NoChange && folder.ChangeType[i] != null)
                {
                    renamePos = -1;
                    break;
                }
            }

            if (renamePos > -1)
            {
                folder.Priority[renamePos] = 1;
                return;
            }

            int mostUpdatedPos = 0;

            for (int i = 0; i < numOfFiles; i++)
            {
                if (folder.Exists[i])
                {
                    mostUpdatedPos = i;
                    break;
                }
            }

            folder.Priority[mostUpdatedPos] = 1;

            for (int i = mostUpdatedPos + 1; i < numOfFiles; i++)
            {
                if (!folder.Exists[i])
                {
                    folder.Priority[i] = -1;
                    continue;
                }
                else
                {
                    folder.Priority[i] = folder.Priority[mostUpdatedPos];
                }
            }

        }

        #endregion

    }
}
