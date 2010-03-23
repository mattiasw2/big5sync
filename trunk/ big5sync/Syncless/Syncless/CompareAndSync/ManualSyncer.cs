using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Visitor;
using Syncless.Filters;
using Syncless.Core;
using Syncless.Notification.SLLNotification;
using Syncless.Notification.UINotification;
using Syncless.Notification;

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
            List<Filter> filters = request.Filters.ToList<Filter>();
            filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));
            RootCompareObject rco = new RootCompareObject(request.Paths);
            //Analyzing
            progress.ChangeToAnalyzing();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor());
            ComparerVisitor visitor = new ComparerVisitor();
            CompareObjectHelper.PostTraverseFolder(rco, visitor);
            int totalJobs = visitor.TotalNodes;
            //Syncing
            progress.ChangeToSyncing(totalJobs);
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.Config,progress));
            //XML Writer
            progress.ChangeToFinalizing(totalJobs);
            CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor());

            if (request.Notify)
                ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));
            //Finished/
            progress.ChangeToFinished();
            ServiceLocator.UINotificationQueue().Enqueue(new SyncCompleteNotification());
        }

        public static RootCompareObject Compare(ManualCompareRequest request)
        {
            List<Filter> filters = request.Filters.ToList<Filter>();
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
