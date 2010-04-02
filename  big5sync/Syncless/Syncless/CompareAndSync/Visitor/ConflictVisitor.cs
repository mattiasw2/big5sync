using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Visitor
{
    public class ConflictVisitor : IVisitor
    {
        private readonly SyncConfig _syncConfig;

        public ConflictVisitor(SyncConfig syncConfig)
        {
            _syncConfig = syncConfig;
        }

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            if (file.ConflictPositions == null || file.ConflictPositions.Count == 0)
                return;

            foreach (int i in file.ConflictPositions)
            {
                ConflictHandler(file, i);
            }
        }

        public void Visit(FolderCompareObject folder, int numOfPaths) { }

        public void Visit(RootCompareObject root) { }

        private void ConflictHandler(FileCompareObject fco, int fileIndex)
        {
            string src = Path.Combine(fco.GetSmartParentPath(fileIndex), fco.Name);
            string conflictFolder = Path.Combine(fco.GetSmartParentPath(fileIndex), _syncConfig.ConflictDir);
            if (!Directory.Exists(conflictFolder))
                Directory.CreateDirectory(conflictFolder);
            string dest = Path.Combine(conflictFolder, fco.Name);
            try
            {
                CommonMethods.CopyFile(src, dest, true);
                CommonMethods.DeleteFile(src);
                fco.FinalState[fileIndex] = null; //Set back to null
            }
            catch (CopyFileException)
            {
                fco.FinalState[fileIndex] = FinalState.Error;
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying file from " + src + " to " + dest));
            }
            catch (DeleteFileException)
            {
            }
        }
    }
}
