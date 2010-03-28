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
        /// Temporary method to handle non-existent folders until we handle deletion of tagged folders
        /// </summary>
        /// <param name="paths"></param>
        private void CreateRootIfNotExist(string[] paths)
        {
            foreach (string path in paths)
            {
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
            }
        }

        /// <summary>
        /// Sync a list of folders, without tagging or writing to metadata (if it exists)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RootCompareObject SyncFolders(ManualSyncRequest request)
        {
            CreateRootIfNotExist(request.Paths);
            SyncStartNotification notification = new SyncStartNotification(request.TagName);
            SyncProgress progress = notification.Progress;
            ServiceLocator.UINotificationQueue().Enqueue(notification);
            RootCompareObject rco = new RootCompareObject(request.Paths);
            progress.ChangeToAnalyzing();
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            CompareObjectHelper.PreTraverseFolder(rco, new IgnoreMetaDataVisitor());
            ComparerVisitor visitor = new ComparerVisitor();
            
            CompareObjectHelper.PostTraverseFolder(rco, visitor);
            int totalJobs = visitor.TotalNodes;
            progress.ChangeToSyncing(totalJobs);
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.Config,progress));
            progress.ChangeToFinalizing(0);
            progress.ChangeToFinished();
            ServiceLocator.UINotificationQueue().Enqueue(new SyncCompleteNotification(request.TagName,rco));
            return rco;
        }

        public void Sync(ManualSyncRequest request)
        {
            ManualQueueControl.Instance.AddSyncJob(request);
        }

        public void Cancel(CancelSyncRequest request)
        {
            ManualQueueControl.Instance.CancelSyncJob(request);
        }

        public RootCompareObject Compare(ManualCompareRequest request)
        {
            return ManualSyncer.Compare(request);
        }

        public void Sync(AutoSyncRequest request)
        {
            SeamlessQueueControl.Instance.AddSyncJob(request);
        }

        public void PrepareForTermination()
        {
            SeamlessQueueControl.Instance.Dispose();
            ManualQueueControl.Instance.Dispose();
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
