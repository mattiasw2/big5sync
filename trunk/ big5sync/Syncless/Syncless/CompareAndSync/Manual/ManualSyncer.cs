using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.CompareAndSync.Manual.Visitor;
using Syncless.CompareAndSync.Request;
using Syncless.Core;
using Syncless.Filters;
using Syncless.Logging;
using Syncless.Notification;


namespace Syncless.CompareAndSync.Manual
{
    /// <summary>
    /// <c>ManualSyncer</c> contains all the methods for a manual synchronization job.
    /// </summary>
    public static class ManualSyncer
    {
        /// <summary>
        /// Synchronizes a job given a <see cref="ManualSyncRequest"/> and a <see cref="SyncProgress"/>.
        /// </summary>
        /// <param name="request">The <see cref="ManualSyncRequest"/> to pass in.</param>
        /// <param name="progress">The <see cref="SyncProgress"/> to pass in.</param>
        /// <returns></returns>
        public static RootCompareObject Sync(ManualSyncRequest request, SyncProgress progress)
        {
            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STARTED, "Started Manual Sync for " + request.TagName));

            //Initialize and add filters conflict and archive filters to it
            List<Filter> filters = request.Filters.ToList();
            filters.Add(FilterFactory.CreateArchiveFilter(request.Config.ArchiveName));
            filters.Add(FilterFactory.CreateArchiveFilter(request.Config.ConflictDir));
            RootCompareObject rco = new RootCompareObject(request.Paths);

            // Analyzing
            progress.ChangeToAnalyzing();
            List<string> typeConflicts = new List<string>();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters, typeConflicts,progress), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor(), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new ProcessMetadataVisitor(), progress);
            CompareObjectHelper.LevelOrderTraverseFolder(rco, new FolderRenameVisitor(), progress);
            ComparerVisitor comparerVisitor = new ComparerVisitor();
            CompareObjectHelper.PostTraverseFolder(rco, comparerVisitor, progress);

            if (progress.State != SyncState.Cancelled)
            {
                // Syncing
                progress.ChangeToSyncing(comparerVisitor.TotalNodes);
                HandleBuildConflicts(typeConflicts, request.Config);
                CompareObjectHelper.PreTraverseFolder(rco, new ConflictVisitor(request.Config), progress);
                SyncerVisitor syncerVisitor = new SyncerVisitor(request.Config, progress);
                CompareObjectHelper.PreTraverseFolder(rco, syncerVisitor, progress);

                // XML Writer
                progress.ChangeToFinalizing(syncerVisitor.NodesCount);
                CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor(progress), progress);
                progress.ChangeToFinished();

                if (request.Notify)
                    ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));

                // Finished
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STOPPED, "Completed Manual Sync for " + request.TagName));
                return rco;
            }

            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STOPPED, "Cancelled Manual Sync for " + request.TagName));
            return null;
        }

        /// <summary>
        /// Compares/previews a job given a <see cref="ManualCompareRequest"/> and a <see cref="PreviewProgress"/>.
        /// </summary>
        /// <param name="request">The <see cref="ManualCompareRequest"/> to pass in.</param>
        /// <param name="progress">The <see cref="PreviewProgress"/> to pass in.</param>
        /// <returns></returns>
        public static RootCompareObject Compare(ManualCompareRequest request, PreviewProgress progress)
        {
            List<Filter> filters = request.Filters.ToList();
            filters.Add(FilterFactory.CreateArchiveFilter(request.Config.ArchiveName));
            filters.Add(FilterFactory.CreateArchiveFilter(request.Config.ConflictDir));
            RootCompareObject rco = new RootCompareObject(request.Paths);
            
            List<string> typeConflicts = new List<string>();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters, typeConflicts, progress), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor(), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new ProcessMetadataVisitor(), progress);
            CompareObjectHelper.LevelOrderTraverseFolder(rco, new FolderRenameVisitor(), progress);
            ComparerVisitor comparerVisitor = new ComparerVisitor();
            CompareObjectHelper.PostTraverseFolder(rco, comparerVisitor, progress);

            return rco;
        }

        private static void HandleBuildConflicts(List<string> typeConflicts, SyncConfig config)
        {
            foreach (string s in typeConflicts)
            {
                if (Directory.Exists(s))
                {
                    DirectoryInfo info = new DirectoryInfo(s);
                    // ReSharper disable PossibleNullReferenceException
                    string conflictPath = Path.Combine(info.Parent.FullName, config.ConflictDir);
                    // ReSharper restore PossibleNullReferenceException
                    if (!Directory.Exists(conflictPath))
                        Directory.CreateDirectory(conflictPath);
                    string dest = Path.Combine(conflictPath, info.Name);

                    try
                    {
                        CommonMethods.CopyDirectory(s, dest);
                        CommonMethods.DeleteFolder(s);
                    }
                    catch (CopyFolderException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying folder: " + s + " to " + dest));
                    }
                    catch (DeleteFolderException)
                    {
                    }
                }
                else if (File.Exists(s))
                {
                    FileInfo info = new FileInfo(s);
                    // ReSharper disable AssignNullToNotNullAttribute
                    string conflictPath = Path.Combine(info.DirectoryName, config.ConflictDir);
                    // ReSharper restore AssignNullToNotNullAttribute
                    if (!Directory.Exists(conflictPath))
                        Directory.CreateDirectory(conflictPath);
                    string dest = Path.Combine(conflictPath, info.Name);

                    try
                    {
                        CommonMethods.CopyFile(s, dest);
                        CommonMethods.DeleteFile(s);
                    }
                    catch (CopyFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying file from " + s + " to " + dest));
                    }
                    catch (DeleteFileException)
                    {
                    }
                }
            }
        }
    }
}