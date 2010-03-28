using System;
using System.Collections.Generic;
using System.Linq;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Visitor;
using Syncless.Core;
using Syncless.Filters;
using Syncless.Notification;
using Syncless.Notification.SLLNotification;
using Syncless.Notification.UINotification;

namespace Syncless.CompareAndSync
{
    public static class ManualSyncer
    {
        public static void Sync(ManualSyncRequest request, SyncProgress progress)
        {
            //if (request.Paths.Length < 2)
            //{
            //    ServiceLocator.UINotificationQueue().Enqueue(new NothingToSyncNotification(request.TagName));
            //    if (request.Notify)
            //        ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));
            //    return;
            //}

            //Started
            //SyncStartNotification notification = new SyncStartNotification(request.TagName);
            //SyncProgress progress = notification.Progress;

            //ServiceLocator.UINotificationQueue().Enqueue(notification);

            List<Filter> filters = request.Filters.ToList();
            filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));
            RootCompareObject rco = new RootCompareObject(request.Paths);

            //Analyzing
            progress.ChangeToAnalyzing();
            if (progress.State == SyncState.Cancelled)
                ServiceLocator.UINotificationQueue().Enqueue(new CancelSyncNotification(request.TagName));
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters), progress);
            if (progress.State == SyncState.Cancelled)
                ServiceLocator.UINotificationQueue().Enqueue(new CancelSyncNotification(request.TagName));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor(), progress);
            if (progress.State == SyncState.Cancelled)
                ServiceLocator.UINotificationQueue().Enqueue(new CancelSyncNotification(request.TagName));
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor(), progress);
            if (progress.State == SyncState.Cancelled)
                ServiceLocator.UINotificationQueue().Enqueue(new CancelSyncNotification(request.TagName));
            ComparerVisitor comparerVisitor = new ComparerVisitor();
            CompareObjectHelper.PostTraverseFolder(rco, comparerVisitor, progress);
            if (progress.State == SyncState.Cancelled)
                ServiceLocator.UINotificationQueue().Enqueue(new CancelSyncNotification(request.TagName));

            if (progress.State != SyncState.Cancelled)
            {
                //Syncing
                progress.ChangeToSyncing(comparerVisitor.TotalNodes);
                SyncerVisitor syncerVisitor = new SyncerVisitor(request.Config, progress);
                CompareObjectHelper.PreTraverseFolder(rco, syncerVisitor, progress);

                //XML Writer
                progress.ChangeToFinalizing(syncerVisitor.NodesCount);

                CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor(progress), progress);

                if (request.Notify)
                    ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));

                //Finished
                ServiceLocator.UINotificationQueue().Enqueue(new SyncCompleteNotification(request.TagName, rco));
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
