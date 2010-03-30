using System.Collections.Generic;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Visitor;
using Syncless.Core;
using Syncless.Notification;
using Syncless.Notification.UINotification;

namespace Syncless.CompareAndSync
{
    public class CompareAndSyncController
    {
        private static CompareAndSyncController _instance;

        public static CompareAndSyncController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CompareAndSyncController();
                }
                return _instance;
            }
        }

        private CompareAndSyncController()
        {

        }

        /// <summary>
        /// Sync a list of folders, without tagging or writing to metadata (if it exists)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RootCompareObject SyncFolders(ManualSyncRequest request)
        {
            SyncStartNotification notification = new SyncStartNotification(request.TagName);
            SyncProgress progress = notification.Progress;
            ServiceLocator.UINotificationQueue().Enqueue(notification);
            RootCompareObject rco = new RootCompareObject(request.Paths);
            progress.ChangeToAnalyzing();
            List<string> buildConflicts = new List<string>();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters, buildConflicts), progress);
            CompareObjectHelper.PreTraverseFolder(rco, new IgnoreMetaDataVisitor(), progress);
            ComparerVisitor visitor = new ComparerVisitor();

            CompareObjectHelper.PostTraverseFolder(rco, visitor, progress);
            int totalJobs = visitor.TotalNodes;
            progress.ChangeToSyncing(totalJobs);
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.Config, progress), progress);
            progress.ChangeToFinalizing(0);
            AbstractNotification endnotification = new SyncCompleteNotification(request.TagName, rco);
            progress.ChangeToFinished(endnotification);
            return rco;
        }

        public void Sync(ManualSyncRequest request)
        {
            ManualQueueControl.Instance.AddSyncJob(request);
        }

        public bool Cancel(CancelSyncRequest request)
        {
            return ManualQueueControl.Instance.CancelSyncJob(request);
        }

        public RootCompareObject Compare(ManualCompareRequest request)
        {
            return ManualSyncer.Compare(request);
        }

        public void Sync(AutoSyncRequest request)
        {
            SeamlessQueueControl.Instance.AddSyncJob(request);
        }

        public bool PrepareForTermination()
        {
            return (ManualQueueControl.Instance.PrepareForTermination() &&
                    SeamlessQueueControl.Instance.PrepareForTermination());
        }

        public void Terminate()
        {
            ManualQueueControl.Instance.Terminate();
            SeamlessQueueControl.Instance.Terminate();
        }

        public bool IsQueued(string tagName)
        {
            return ManualQueueControl.Instance.IsQueued(tagName);
        }

        public bool IsSyncing(string tagName)
        {
            return ManualQueueControl.Instance.IsSyncing(tagName);
        }

        public bool IsQueuedOrSyncing(string tagName)
        {
            return ManualQueueControl.Instance.IsQueuedOrSyncing(tagName);
        }
    }
}
