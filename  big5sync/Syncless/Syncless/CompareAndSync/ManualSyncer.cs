using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Visitor;
using Syncless.Core;
using Syncless.Filters;
using Syncless.Logging;
using Syncless.Notification;
using Syncless.Notification.SLLNotification;
using Syncless.Notification.UINotification;

namespace Syncless.CompareAndSync
{
    public static class ManualSyncer
    {
        public static void Sync(ManualSyncRequest request, SyncProgress progress)
        {
            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STARTED, "Started Manual Sync for " + request.TagName));

            List<Filter> filters = request.Filters.ToList();
            filters.Add(FilterFactory.CreateArchiveFilter(request.Config.ArchiveName));
            filters.Add(FilterFactory.CreateArchiveFilter(request.Config.ConflictDir));
            RootCompareObject rco = new RootCompareObject(request.Paths);

            //Analyzing
            progress.ChangeToAnalyzing();
            List<string> typeConflicts = new List<string>();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters, typeConflicts), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor(), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new ProcessMetadataHashVisitor(), progress);
            CompareObjectHelper.LevelOrderTraverseFolder(rco, new FolderRenameVisitor(), progress);
            ComparerVisitor comparerVisitor = new ComparerVisitor();
            CompareObjectHelper.PostTraverseFolder(rco, comparerVisitor, progress);

            if (progress.State != SyncState.Cancelled)
            {
                //Syncing
                progress.ChangeToSyncing(comparerVisitor.TotalNodes);
                HandleBuildConflicts(typeConflicts, request.Config);
                CompareObjectHelper.PreTraverseFolder(rco, new ConflictVisitor(request.Config), progress);
                SyncerVisitor syncerVisitor = new SyncerVisitor(request.Config, progress);
                CompareObjectHelper.PreTraverseFolder(rco, syncerVisitor, progress);

                //XML Writer
                progress.ChangeToFinalizing(syncerVisitor.NodesCount);
                CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor(progress), progress);
                AbstractNotification notification = new SyncCompleteNotification(request.TagName, rco);
                progress.ChangeToFinished(notification);

                if (request.Notify)
                    ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));

                //Finished
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STOPPED,
                                                                                "Completed Manual Sync for " +
                                                                                request.TagName));
            }
            else
            {

                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STOPPED,
                                                                                    "Cancelled Manual Sync for " +
                                                                                    request.TagName));
            }
        }

        public static RootCompareObject Compare(ManualCompareRequest request)
        {
            List<Filter> filters = request.Filters.ToList();
            filters.Add(FilterFactory.CreateArchiveFilter(request.Config.ArchiveName));
            filters.Add(FilterFactory.CreateArchiveFilter(request.Config.ConflictDir));
            RootCompareObject rco = new RootCompareObject(request.Paths);
            List<string> buildConflicts = new List<string>();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters, buildConflicts), null);
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor(), null);
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor(), null);
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor(), null);
            return rco;
        }

        private static void HandleBuildConflicts(List<string> typeConflicts, SyncConfig config)
        {
            foreach (string s in typeConflicts)
            {
                if (Directory.Exists(s))
                {
                    DirectoryInfo info = new DirectoryInfo(s);
                    string conflictPath = Path.Combine(info.Parent.FullName, config.ConflictDir);
                    if (!Directory.Exists(conflictPath))
                        Directory.CreateDirectory(conflictPath);
                    string dest = Path.Combine(conflictPath, info.Name);

                    try
                    {
                        CommonMethods.CopyDirectory(s, dest);
                        CommonMethods.DeleteFolder(s, true);
                    }
                    catch (CopyFolderException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying folder: " + s + " to " + dest));
                    }
                    catch (DeleteFolderException)
                    {
                        ;
                    }
                }
                else if (File.Exists(s))
                {
                    FileInfo info = new FileInfo(s);
                    string conflictPath = Path.Combine(info.DirectoryName, config.ConflictDir);
                    if (!Directory.Exists(conflictPath))
                        Directory.CreateDirectory(conflictPath);
                    string dest = Path.Combine(conflictPath, info.Name);

                    try
                    {
                        CommonMethods.CopyFile(s, dest, true);
                        CommonMethods.DeleteFile(s);
                    }
                    catch (CopyFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying file from " + s + " to " + dest));
                    }
                    catch (DeleteFileException)
                    {
                        ;
                    }
                }
            }
        }
    }
}
