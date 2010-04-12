using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    /// <summary>
    /// <c>ProcessMetadataVisitor</c> is in charge of processing the metadata and comparing it against the actual file or folder, as well as handle whether or not to rehash a file.
    /// </summary>
    public class ProcessMetadataVisitor : IVisitor
    {

        #region IVisitor Members

        /// <summary>
        /// Visit implementation for <see cref="FileCompareObject"/>.
        /// </summary>
        /// <param name="file">The <see cref="FileCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        public void Visit(FileCompareObject file, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                PopulateHash(file, i);
                ProcessFileMetaData(file, i);
            }
        }

        /// <summary>
        /// Visit implementation for <see cref="FolderCompareObject"/>.
        /// </summary>
        /// <param name="folder">The <see cref="FolderCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
                ProcessFolderMetaData(folder, i);
        }

        // Do nothing
        public void Visit(RootCompareObject root)
        {
        }

        #endregion

        #region File Operations

        private void PopulateHash(FileCompareObject file, int index)
        {
            if (file.Exists[index])
            {
                // If the metadata exists, and the creation time of the file is equals to that of the metadata, and the last write time is also the same,
                // as well as the length, we assume that the file is unchanged, and simply use the hash info from the metadata instead of re-hashing. This
                // saves a lot of time.
                if (file.MetaExists[index] && file.CreationTimeUtc[index] == file.MetaCreationTimeUtc[index] && file.LastWriteTimeUtc[index] == file.MetaLastWriteTimeUtc[index] && file.Length[index] == file.MetaLength[index])
                {
                    file.Hash[index] = file.MetaHash[index];
                }
                else
                {
                    try
                    {
                        // Calculate the hash of the file
                        file.Hash[index] = CommonMethods.CalculateMD5Hash(Path.Combine(file.GetSmartParentPath(index), file.Name));
                    }
                    catch (HashFileException)
                    {
                        // If there is an error hashing, we set the FinalState to error, and make it invalid so it is excluded from all comparison and
                        // syncing.
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error hashing " + Path.Combine(file.GetSmartParentPath(index), file.Name + ".")));
                        file.FinalState[index] = FinalState.Error;
                        file.Invalid = true;
                    }
                }
            }
        }

        // Compare the metadata of each file with respects to itself,
        // without concerning itself with any other file.
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
            else if (file.LastKnownState[index].HasValue)
            {
                if (file.LastKnownState[index] == LastKnownState.Deleted)
                    file.ChangeType[index] = MetaChangeType.Delete;
            }
        }

        #endregion

        #region Folder Operations

        // Compare the metadata of each folder with respects to itself,
        // without concerning itself with any other file.
        private void ProcessFolderMetaData(FolderCompareObject folder, int index)
        {
            if (folder.Exists[index] && !folder.MetaExists[index])
                folder.ChangeType[index] = MetaChangeType.New; //Possible rename/move
            else if (!folder.Exists[index] && folder.MetaExists[index])
                folder.ChangeType[index] = MetaChangeType.Delete; //Possible rename/move
            else if (folder.Exists[index] && folder.MetaExists[index])
                folder.ChangeType[index] = MetaChangeType.NoChange;
            else if (folder.LastKnownState[index].HasValue)
            {
                if (folder.LastKnownState[index] == LastKnownState.Deleted)
                    folder.ChangeType[index] = MetaChangeType.Delete;
            }
        }

        #endregion

    }
}