using System.Collections.Generic;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Manual.CompareObject;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    public class FolderRenameVisitor : IVisitor
    {

        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfPaths) { }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            DetectFolderRename(folder, numOfPaths);
        }

        public void Visit(RootCompareObject root) { }

        #endregion

        private void DetectFolderRename(FolderCompareObject folder, int numOfPaths)
        {
            List<int> deleteIndexes = new List<int>();
            List<int> unchangedIndexes = new List<int>();

            for (int i = 0; i < numOfPaths; i++)
            {
                switch (folder.ChangeType[i])
                {
                    case MetaChangeType.Delete:
                        deleteIndexes.Add(i);
                        break;
                    case MetaChangeType.NoChange:
                        unchangedIndexes.Add(i);
                        break;
                }
            }

            //1. If there exists a folder for which meta exists is true and exists is false, it is (aka changeType.delete)
            //highly probable that it is a folder rename
            //2. We check all folders which has the same meta name but different name as the non-existent folder
            //3. If the count is 1, we shall proceed to rename
            FolderCompareObject folderObject;
            int count = 0;
            int renamePos = -1;

            if (deleteIndexes.Count > 0)
            {
                int renameCount;
                folderObject = folder.Parent.GetRenamedFolder(folder.Name, out renameCount);

                if (renameCount > 1)
                {
                    foreach (int j in unchangedIndexes)
                        folder.ChangeType[j] = MetaChangeType.New;
                    return;
                }

                for (int i = 0; i < numOfPaths; i++)
                {
                    if (!folderObject.Exists[i])
                        deleteIndexes.Remove(i);
                }

                if (folderObject != null)
                        MergeRenamedFolder(folder, folderObject, deleteIndexes);

            }

            //if (count == 1)
            //    MergeRenamedFolder(folder, folderObject, renamePos);
        }

        private void MergeRenamedFolder(FolderCompareObject actualFolder, FolderCompareObject renamedFolder, List<int> deleteIndexes)
        {
            Dictionary<string, BaseCompareObject>.KeyCollection renamedFolderContents = renamedFolder.Contents.Keys;
            BaseCompareObject o;
            FolderCompareObject actualFldrObj;
            FolderCompareObject renamedFolderObj;
            FileCompareObject actualFileObj;
            FileCompareObject renamedFileObj;

            actualFolder.NewName = renamedFolder.Name;

            foreach (int i in deleteIndexes)
                actualFolder.ChangeType[i] = MetaChangeType.Rename;

            foreach (string name in renamedFolderContents)
            {
                if (actualFolder.Contents.TryGetValue(name, out o))
                {
                    if ((actualFldrObj = o as FolderCompareObject) != null)
                    {
                        renamedFolderObj = renamedFolder.Contents[name] as FolderCompareObject;
                        MergeFolder(actualFldrObj, renamedFolderObj, deleteIndexes);
                    }
                    else
                    {
                        actualFileObj = o as FileCompareObject;
                        renamedFileObj = renamedFolder.Contents[name] as FileCompareObject;
                        MergeFile(actualFileObj, renamedFileObj, deleteIndexes);
                    }
                }
                else
                {
                    actualFolder.AddChild(renamedFolder.Contents[name]);
                    renamedFolder.Contents[name].Parent = actualFolder;
                }
            }

            actualFolder.Parent.Dirty = true; //EXP
            renamedFolder.Contents = new Dictionary<string, BaseCompareObject>();
            renamedFolder.Invalid = true;
        }

        private void MergeOneLevelDown(FolderCompareObject actualFolder, FolderCompareObject renamedFolder, List<int> deleteIndexes)
        {
            Dictionary<string, BaseCompareObject>.KeyCollection renamedFolderContents = renamedFolder.Contents.Keys;
            BaseCompareObject o;
            FolderCompareObject actualFldrObj;
            FolderCompareObject renamedFolderObj;
            FileCompareObject actualFileObj;
            FileCompareObject renamedFileObj;

            foreach (string name in renamedFolderContents)
            {
                if (actualFolder.Contents.TryGetValue(name, out o))
                {
                    if ((actualFldrObj = o as FolderCompareObject) != null)
                    {
                        renamedFolderObj = renamedFolder.Contents[name] as FolderCompareObject;
                        MergeFolder(actualFldrObj, renamedFolderObj, deleteIndexes);
                    }
                    else
                    {
                        actualFileObj = o as FileCompareObject;
                        renamedFileObj = renamedFolder.Contents[name] as FileCompareObject;
                        MergeFile(actualFileObj, renamedFileObj, deleteIndexes);
                    }
                }
                else
                {
                    actualFolder.AddChild(renamedFolder.Contents[name]);
                    renamedFolder.Contents[name].Parent = actualFolder;
                }
            }

            //actualFolder.Parent.Dirty = true; //EXP
            renamedFolder.Contents = new Dictionary<string, BaseCompareObject>();
            renamedFolder.Invalid = true;
        }

        private void MergeFile(FileCompareObject actualFileObj, FileCompareObject renamedFileObj, List<int> deleteIndexes)
        {
            MergeFileSystemObject(actualFileObj, renamedFileObj, deleteIndexes);

            foreach (int i in deleteIndexes)
            {
                actualFileObj.Hash[i] = renamedFileObj.Hash[i];
                actualFileObj.Length[i] = renamedFileObj.Length[i];
                actualFileObj.LastWriteTime[i] = renamedFileObj.LastWriteTime[i];
                actualFileObj.MetaHash[i] = renamedFileObj.MetaHash[i];
                actualFileObj.MetaLastWriteTime[i] = renamedFileObj.MetaLastWriteTime[i];
                actualFileObj.MetaLength[i] = renamedFileObj.MetaLength[i];
            }
        }

        private void MergeFolder(FolderCompareObject actualFldrObj, FolderCompareObject renamedFolderObj, List<int> deleteIndexes)
        {
            MergeFileSystemObject(actualFldrObj, renamedFolderObj, deleteIndexes);

            foreach (int i in deleteIndexes)
                actualFldrObj.LastKnownState[i] = actualFldrObj.LastKnownState[i];

            if (actualFldrObj.Contents.Count == 0)
            {
                actualFldrObj.Contents = renamedFolderObj.Contents;
                ChangeFatherMother(actualFldrObj);
            }
            else
                MergeOneLevelDown(actualFldrObj, renamedFolderObj, deleteIndexes);
        }

        private void MergeFileSystemObject(BaseCompareObject actualObj, BaseCompareObject renameObj, List<int> deleteIndexes)
        {
            foreach (int i in deleteIndexes)
            {
                actualObj.ChangeType[i] = renameObj.ChangeType[i];
                actualObj.CreationTime[i] = renameObj.CreationTime[i];
                actualObj.Exists[i] = renameObj.Exists[i];
                actualObj.FinalState[i] = renameObj.FinalState[i];
                actualObj.Invalid = renameObj.Invalid; //EXP
                actualObj.MetaCreationTime[i] = renameObj.MetaCreationTime[i];
                actualObj.MetaExists[i] = renameObj.MetaExists[i];
                actualObj.MetaUpdated[i] = renameObj.MetaUpdated[i];
                actualObj.LastKnownState[i] = renameObj.LastKnownState[i];
            }
        }

        private void ChangeFatherMother(FolderCompareObject actualFldrObj)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection values = actualFldrObj.Contents.Values;

            foreach (BaseCompareObject bco in values)
                bco.Parent = actualFldrObj;
        }
    }
}