using System.Collections.Generic;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Manual.CompareObject;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    /// <summary>
    /// <c>FolderRenameVisitor</c> is responsible for handling folder renames, and merging the contents of the renamed folders with their previous names.
    /// </summary>
    public class FolderRenameVisitor : IVisitor
    {

        #region IVisitor Members

        // Do nothing for file.
        public void Visit(FileCompareObject file, int numOfPaths)
        {
        }

        /// <summary>
        /// Visit implementation for <see cref="FolderCompareObject"/>.
        /// </summary>
        /// <param name="folder">The <see cref="FolderCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            DetectFolderRename(folder, numOfPaths);
        }

        // Do nothing for root.
        public void Visit(RootCompareObject root)
        {
        }

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

            // 1. If there exists a folder for which meta exists is true and exists is false, it is (aka changeType.delete)
            //    highly probable that it is a folder rename
            // 2. We check all folders which has the same meta name but different name as the non-existent folder
            // 3. If the count is 1, we shall proceed to rename
            FolderCompareObject folderObject;

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

                if (folderObject != null)
                {

                    for (int i = 0; i < numOfPaths; i++)
                    {
                        if (!folderObject.Exists[i])
                            deleteIndexes.Remove(i);
                    }

                    MergeRenamedFolder(folder, folderObject, deleteIndexes);
                }

            }
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

            actualFolder.Parent.Dirty = true;
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
                actualFileObj.LastWriteTimeUtc[i] = renamedFileObj.LastWriteTimeUtc[i];
                actualFileObj.MetaHash[i] = renamedFileObj.MetaHash[i];
                actualFileObj.MetaLastWriteTimeUtc[i] = renamedFileObj.MetaLastWriteTimeUtc[i];
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
                ChangeParent(actualFldrObj);
            }
            else
                MergeOneLevelDown(actualFldrObj, renamedFolderObj, deleteIndexes);
        }

        private void MergeFileSystemObject(BaseCompareObject actualObj, BaseCompareObject renameObj, List<int> deleteIndexes)
        {
            foreach (int i in deleteIndexes)
            {
                actualObj.ChangeType[i] = renameObj.ChangeType[i];
                actualObj.CreationTimeUtc[i] = renameObj.CreationTimeUtc[i];
                actualObj.Exists[i] = renameObj.Exists[i];
                actualObj.FinalState[i] = renameObj.FinalState[i];
                actualObj.Invalid = renameObj.Invalid;
                actualObj.MetaCreationTimeUtc[i] = renameObj.MetaCreationTimeUtc[i];
                actualObj.MetaExists[i] = renameObj.MetaExists[i];
                actualObj.MetaUpdated[i] = renameObj.MetaUpdated[i];
                actualObj.LastKnownState[i] = renameObj.LastKnownState[i];
            }
        }

        private void ChangeParent(FolderCompareObject actualFldrObj)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection values = actualFldrObj.Contents.Values;

            foreach (BaseCompareObject bco in values)
                bco.Parent = actualFldrObj;
        }
    }
}