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
            //if (folder.Invalid)
            //    return;

            DetectFolderRename(folder, numOfPaths);
        }

        public void Visit(RootCompareObject root) { }

        #endregion

        private void DetectFolderRename(FolderCompareObject folder, int numOfPaths)
        {
            List<int> deletePos = new List<int>();

            for (int i = 0; i < numOfPaths; i++)
            {
                if (folder.ChangeType[i] == MetaChangeType.Delete)
                    deletePos.Add(i);
            }

            //1. If there exists a folder for which meta exists is true and exists is false, it is (aka changeType.delete)
            //highly probable that it is a folder rename
            //2. We check all folders which has the same meta name but different name as the non-existent folder
            //3. If the count is 1, we shall proceed to rename
            FolderCompareObject folderObject = null;
            int count = 0;
            int renamePos = -1;
            for (int i = 0; i < deletePos.Count; i++)
            {
                if (folder.ChangeType[deletePos[i]] == MetaChangeType.Delete)
                {
                    int renameCount;
                    folderObject = folder.Parent.GetRenamedFolder(folder.Name, folder.CreationTime[i], deletePos[i], out renameCount);

                    if (folderObject != null)
                    {
                        count++;
                        renamePos = deletePos[i];
                    }
                    if (renameCount > 1)
                    {
                        foreach (int j in deletePos)
                            folder.ChangeType[j] = null;
                        return;
                    }
                }
            }

            if (count == 1)
                MergeRenamedFolder(folder, folderObject, renamePos);
        }

        private void MergeRenamedFolder(FolderCompareObject actualFolder, FolderCompareObject renamedFolder, int pos)
        {
            Dictionary<string, BaseCompareObject>.KeyCollection renamedFolderContents = renamedFolder.Contents.Keys;
            BaseCompareObject o;
            FolderCompareObject actualFldrObj;
            FolderCompareObject renamedFolderObj;
            FileCompareObject actualFileObj;
            FileCompareObject renamedFileObj;

            actualFolder.NewName = renamedFolder.Name;
            actualFolder.ChangeType[pos] = MetaChangeType.Rename;

            foreach (string name in renamedFolderContents)
            {
                if (actualFolder.Contents.TryGetValue(name, out o))
                {
                    if ((actualFldrObj = o as FolderCompareObject) != null)
                    {
                        renamedFolderObj = renamedFolder.Contents[name] as FolderCompareObject;
                        MergeFolder(actualFldrObj, renamedFolderObj, pos);
                    }
                    else
                    {
                        actualFileObj = o as FileCompareObject;
                        renamedFileObj = renamedFolder.Contents[name] as FileCompareObject;
                        MergeFile(actualFileObj, renamedFileObj, pos);
                    }
                }
                else
                {
                    actualFolder.AddChild(renamedFolder.Contents[name]);
                    renamedFolder.Contents[name].Parent = actualFolder;
                }
            }

            actualFolder.UpdateRename(pos);
            actualFolder.ChangeType[pos] = MetaChangeType.Rename;
            actualFolder.Parent.Dirty = true; //EXP
            renamedFolder.Contents = new Dictionary<string, BaseCompareObject>();
            renamedFolder.Invalid = true;
        }

        private void MergeOneLevelDown(FolderCompareObject actualFolder, FolderCompareObject renamedFolder, int pos)
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
                        MergeFolder(actualFldrObj, renamedFolderObj, pos);
                    }
                    else
                    {
                        actualFileObj = o as FileCompareObject;
                        renamedFileObj = renamedFolder.Contents[name] as FileCompareObject;
                        MergeFile(actualFileObj, renamedFileObj, pos);
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

        private void MergeFile(FileCompareObject actualFileObj, FileCompareObject renamedFileObj, int pos)
        {
            MergeFileSystemObject(actualFileObj, renamedFileObj, pos);
            actualFileObj.Hash[pos] = renamedFileObj.Hash[pos];
            actualFileObj.Length[pos] = renamedFileObj.Length[pos];
            actualFileObj.LastWriteTime[pos] = renamedFileObj.LastWriteTime[pos];
            actualFileObj.MetaHash[pos] = renamedFileObj.MetaHash[pos];
            actualFileObj.MetaLastWriteTime[pos] = renamedFileObj.MetaLastWriteTime[pos];
            actualFileObj.MetaLength[pos] = renamedFileObj.MetaLength[pos];
            //List of conflicts
        }

        private void MergeFolder(FolderCompareObject actualFldrObj, FolderCompareObject renamedFolderObj, int pos)
        {
            MergeFileSystemObject(actualFldrObj, renamedFolderObj, pos);
            actualFldrObj.UseNewName[pos] = renamedFolderObj.UseNewName[pos];
            actualFldrObj.LastKnownState[pos] = actualFldrObj.LastKnownState[pos];

            if (actualFldrObj.Contents.Count == 0)
            {
                actualFldrObj.Contents = renamedFolderObj.Contents;
                ChangeFatherMother(actualFldrObj);
            }
            else
                MergeOneLevelDown(actualFldrObj, renamedFolderObj, pos);
        }

        private void MergeFileSystemObject(BaseCompareObject actualObj, BaseCompareObject renameObj, int pos)
        {
            actualObj.ChangeType[pos] = renameObj.ChangeType[pos];
            actualObj.CreationTime[pos] = renameObj.CreationTime[pos];
            actualObj.Exists[pos] = renameObj.Exists[pos];
            actualObj.FinalState[pos] = renameObj.FinalState[pos];
            actualObj.Invalid = renameObj.Invalid; //EXP
            actualObj.MetaCreationTime[pos] = renameObj.MetaCreationTime[pos];
            actualObj.MetaExists[pos] = renameObj.MetaExists[pos];
            actualObj.MetaUpdated[pos] = renameObj.MetaUpdated[pos];
            actualObj.LastKnownState[pos] = renameObj.LastKnownState[pos];
        }

        private void ChangeFatherMother(FolderCompareObject actualFldrObj)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection values = actualFldrObj.Contents.Values;

            foreach (BaseCompareObject bco in values)
                bco.Parent = actualFldrObj;
        }
    }
}