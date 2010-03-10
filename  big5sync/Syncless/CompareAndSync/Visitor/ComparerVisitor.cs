using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompareAndSync.CompareObject;

namespace CompareAndSync.Visitor
{
    public class ComparerVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, string[] currentPaths)
        {
            ProcessFileMetaData(file, currentPaths);
            DetectFileRename(file, currentPaths);
            CompareFiles(file, currentPaths);
        }

        public void Visit(FolderCompareObject folder, string[] currentPaths)
        {
            //Delete, New, Copy, Rename
            ProcessFolderMetaData(folder, currentPaths);
            CompareFolders(folder, currentPaths);
        }

        public void Visit(RootCompareObject root)
        {
            // Do nothing
        }

        #endregion

        #region Files

        private void ProcessFileMetaData(FileCompareObject file, string[] currentPaths)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.Exists[i] && !file.MetaExists[i])
                {
                    file.ChangeType[i] = MetaChangeType.New; //Possible rename/move
                    file.Parent.ChangeType[i] = MetaChangeType.Dirty; //Experimental
                }
                else if (!file.Exists[i] && file.MetaExists[i])
                    file.ChangeType[i] = MetaChangeType.Delete; //Possible rename/move
                else if (file.Exists[i] && file.MetaExists[i])
                {
                    if (file.Length[i] != file.MetaLength[i] || file.Hash[i] != file.MetaHash[i])
                    {
                        file.ChangeType[i] = MetaChangeType.Update;
                        file.Parent.ChangeType[i] = MetaChangeType.Dirty; //Experimental
                    }
                    else
                        file.ChangeType[i] = MetaChangeType.NoChange;
                }
                else
                {
                    file.ChangeType[i] = null;
                    file.Parent.ChangeType[i] = MetaChangeType.Dirty; //Experimental
                }
            }
        }

        private void DetectFileRename(FileCompareObject file, string[] currentPaths)
        {
            FileCompareObject f = null;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Delete)
                {
                    f = file.Parent.GetIdenticalFile(file.MetaHash[i], file.MetaCreationTime[i]);
                    
                    if (f != null)
                    {
                        int counter = 0;

                        //Check that f is MetaChangeType.New for exactly one i
                        for (int j = 0; j < f.ChangeType.Length; j++)
                        {
                            if (f.ChangeType[i] == MetaChangeType.New)
                                counter++;
                        }

                        if (counter != 1)
                            return;

                        file.NewName = f.Name;
                        file.ChangeType[i] = MetaChangeType.Rename;
                        file.Parent.Contents.Remove(f.Name);                        
                    }
                }
            }
        }

        private void CompareFiles(FileCompareObject file, string[] currentPaths)
        {
            //Delete will only occur if all other changes are MetaChangeType.NoChange
            List<int> deletePos = new List<int>();

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Delete)
                    deletePos.Add(i);
                else if (file.ChangeType[i] != MetaChangeType.NoChange)
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

            //Rename will only occur if all other changes are MetaChangeType.NoChange
            int renamePos = -1;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Rename)
                    renamePos = i;
                else if (file.ChangeType[i] != MetaChangeType.NoChange || file.ChangeType[i] != null)
                {
                    renamePos = -1;
                    break;
                }
            }

            if (renamePos > 0)
            {
                file.Priority[renamePos] = 1;
                return;
            }

            //Update/Create handled in a similar way
            int mostUpdatedPos = 0;           

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.Exists[i])
                {
                    mostUpdatedPos = i;
                    break;
                }
            }

            file.Priority[mostUpdatedPos] = 1;

            for (int i = mostUpdatedPos + 1; i < currentPaths.Length; i++)
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
        }

        #endregion

        #region Folders

        private void ProcessFolderMetaData(FolderCompareObject folder, string[] currentPaths)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (folder.Exists[i] && !folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.New; //Possible rename/move
                else if (!folder.Exists[i] && folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.Delete; //Possible rename/move
                else if (folder.Exists[i] && folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.NoChange;
                else
                    folder.ChangeType[i] = null;
            }
        }

        private void CompareFolders(FolderCompareObject folder, string[] currentPaths)
        {
            //Delete will only occur if none of the folders are marked as dirty
            List<int> deletePos = new List<int>();

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (folder.ChangeType[i] == MetaChangeType.Delete)
                    deletePos.Add(i);
                else if (folder.ChangeType[i] == MetaChangeType.Dirty) //Old code: != MetaChangeType.NoChange
                {
                    deletePos.Clear();
                    break;
                }
            }

            if (deletePos.Count > 0)
            {
                foreach (int i in deletePos)
                    folder.Priority[i] = 1;
                return;
            }

            int mostUpdatedPos = 0;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (folder.Exists[i])
                {
                    mostUpdatedPos = i;
                    break;
                }
            }

            folder.Priority[mostUpdatedPos] = 1;

            for (int i = mostUpdatedPos + 1; i < currentPaths.Length; i++)
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
