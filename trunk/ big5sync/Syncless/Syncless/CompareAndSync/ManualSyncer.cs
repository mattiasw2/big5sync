using System;
using System.Collections.Generic;
using System.Linq;
using Syncless.CompareAndSync.CompareObject;
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
            filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));
            RootCompareObject rco = new RootCompareObject(request.Paths);

            //Analyzing
            progress.ChangeToAnalyzing();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor(), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor(), progress);
            ComparerVisitor comparerVisitor = new ComparerVisitor();
            CompareObjectHelper.PostTraverseFolder(rco, comparerVisitor, progress);

            if (progress.State != SyncState.Cancelled)
            {
                CompareObjectHelper.PreTraverseFolder(rco, new ConflictVisitor(request.Config), progress);

                //Syncing
                progress.ChangeToSyncing(comparerVisitor.TotalNodes);
                SyncerVisitor syncerVisitor = new SyncerVisitor(request.Config, progress);
                CompareObjectHelper.PreTraverseFolder(rco, syncerVisitor, progress);

                //XML Writer
                progress.ChangeToFinalizing(syncerVisitor.NodesCount);

                CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor(progress), progress);

                progress.ChangeToFinished();

                if (request.Notify)
                    ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));

                //Finished
                ServiceLocator.UINotificationQueue().Enqueue(new SyncCompleteNotification(request.TagName, rco));
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STOPPED,
                                                                                    "Completed Manual Sync for " +
                                                                                    request.TagName));
            }
            else
            {
                progress.ChangeToFinished();

                if (request.Notify)
                    ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));

                ServiceLocator.UINotificationQueue().Enqueue(new SyncCompleteNotification(request.TagName, rco));
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STOPPED,
                                                                                    "Cancelled Manual Sync for " +
                                                                                    request.TagName));
            }
        }

        public static RootCompareObject Compare(ManualCompareRequest request)
        {
            List<Filter> filters = request.Filters.ToList();
            filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));
            RootCompareObject rco = new RootCompareObject(request.Paths);
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters), null);
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor(), null);
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor(), null);
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor(), null);
            return rco;
        }
    }
}
