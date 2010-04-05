using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    public class ProcessMetadataVisitor : IVisitor
    {

        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                PopulateHash(file, i);
                ProcessFileMetaData(file, i);
            }
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
                ProcessFolderMetaData(folder, i);
        }

        public void Visit(RootCompareObject root) { }

        #endregion

        #region File Operations

        private void PopulateHash(FileCompareObject file, int index)
        {
            if (file.Exists[index])
            {
                if (file.MetaExists[index] && file.CreationTime[index] == file.MetaCreationTime[index] && file.LastWriteTime[index] == file.MetaLastWriteTime[index] && file.Length[index] == file.MetaLength[index])
                {
                    file.Hash[index] = file.MetaHash[index];
                }
                else
                {
                    try
                    {
                        file.Hash[index] = CommonMethods.CalculateMD5Hash(Path.Combine(file.GetSmartParentPath(index), file.Name));
                    }
                    catch (HashFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error hashing " + Path.Combine(file.GetSmartParentPath(index), file.Name + ".")));
                        file.FinalState[index] = FinalState.Error;
                        file.Invalid = true; //EXP
                    }
                }
            }
        }

        private void ProcessFileMetaData(FileCompareObject file, int index)
        {
            if (file.Exists[index] && !file.MetaExists[index])
                file.ChangeType[index] = MetaChangeType.New; //Possible rename/move
            else if (!file.Exists[index] && file.MetaExists[index])
                file.ChangeType[index] = MetaChangeType.Delete; //Possible rename/move
            else if (file.Exists[index] && file.MetaExists[index])
            {
                if (file.Length[index] != file.MetaLength[index] || file.Hash[index] != file.MetaHash[index])
                    file.ChangeType[index] = MetaChangeType.Update;
                else
                    file.ChangeType[index] = MetaChangeType.NoChange;
            }
            else if (file.ToDoAction[index].HasValue)
            {
                if (file.ToDoAction[index] == LastKnownState.Deleted)
                    file.ChangeType[index] = MetaChangeType.Delete;
            }
        }

        #endregion

        #region Folder Operations

        private void ProcessFolderMetaData(FolderCompareObject folder, int index)
        {
            if (folder.Exists[index] && !folder.MetaExists[index])
                folder.ChangeType[index] = MetaChangeType.New; //Possible rename/move
            else if (!folder.Exists[index] && folder.MetaExists[index])
                folder.ChangeType[index] = MetaChangeType.Delete; //Possible rename/move
            else if (folder.Exists[index] && folder.MetaExists[index])
                folder.ChangeType[index] = MetaChangeType.NoChange;
            else if (folder.ToDoAction[index].HasValue)
            {
                if (folder.ToDoAction[index] == LastKnownState.Deleted)
                    folder.ChangeType[index] = MetaChangeType.Delete;
            }
        }

        #endregion

    }
}