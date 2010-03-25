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
        public static void Sync(ManualSyncRequest request)
        {
            //Started
            SyncStartNotification notification = new SyncStartNotification(request.TagName);
            SyncProgress progress = notification.Progress;
            ServiceLocator.UINotificationQueue().Enqueue(notification);
            List<Filter> filters = request.Filters.ToList();
            filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));
            RootCompareObject rco = new RootCompareObject(request.Paths);

            //Analyzing
            progress.ChangeToAnalyzing();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor());
            ComparerVisitor comparerVisitor = new ComparerVisitor();
            CompareObjectHelper.PostTraverseFolder(rco, comparerVisitor);

            //Syncing
            progress.ChangeToSyncing(comparerVisitor.TotalNodes);
            SyncerVisitor syncerVisitor = new SyncerVisitor(request.Config,progress);
            CompareObjectHelper.PreTraverseFolder(rco, syncerVisitor);

            //XML Writer
            progress.ChangeToFinalizing(syncerVisitor.NodesCount);
            CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor(progress));

            if (request.Notify)
                ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));

            //Finished
            ServiceLocator.UINotificationQueue().Enqueue(new SyncCompleteNotification(request.TagName,rco));
        }

        public static RootCompareObject Compare(ManualCompareRequest request)
        {
            List<Filter> filters = request.Filters.ToList();
            filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));
            RootCompareObject rco = new RootCompareObject(request.Paths);
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor());
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            return rco;
        }
    }
}
