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
        public static void Sync(ManualSyncRequest request)
        {
            if (request.Paths.Length < 2)
            {
                ServiceLocator.UINotificationQueue().Enqueue(new NothingToSyncNotification(request.TagName));
                return;
            }

            Console.WriteLine("Sync 1");

            //Started
            SyncStartNotification notification = new SyncStartNotification(request.TagName);
            SyncProgress progress = notification.Progress;

            ServiceLocator.UINotificationQueue().Enqueue(notification);
            Console.WriteLine("Sync 2");
            List<Filter> filters = request.Filters.ToList();
            filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));
            RootCompareObject rco = new RootCompareObject(request.Paths);
            Console.WriteLine("Sync 3");

            //Analyzing
            progress.ChangeToAnalyzing();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters));
            Console.WriteLine("Sync 4");
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            Console.WriteLine("Sync 5");
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor());
            Console.WriteLine("Sync 6");
            ComparerVisitor comparerVisitor = new ComparerVisitor();
            Console.WriteLine("Sync 7");
            CompareObjectHelper.PostTraverseFolder(rco, comparerVisitor);
            Console.WriteLine("Sync 8");

            //Syncing
            progress.ChangeToSyncing(comparerVisitor.TotalNodes);
            SyncerVisitor syncerVisitor = new SyncerVisitor(request.Config,progress);
            Console.WriteLine("Sync 9");
            CompareObjectHelper.PreTraverseFolder(rco, syncerVisitor);
            Console.WriteLine("Sync 10");

            //XML Writer
            progress.ChangeToFinalizing(syncerVisitor.NodesCount);

            CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor(progress));
            Console.WriteLine("Sync 11");

            if (request.Notify)
                ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorTagNotification(request.TagName));

            Console.WriteLine("Sync 12");

            //Finished
            ServiceLocator.UINotificationQueue().Enqueue(new SyncCompleteNotification(request.TagName,rco));
            Console.WriteLine("Sync 13");
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
