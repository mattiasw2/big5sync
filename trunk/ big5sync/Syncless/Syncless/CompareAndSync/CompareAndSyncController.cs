using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Visitor;
using Syncless.Filters;

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
            RootCompareObject rco = new RootCompareObject(request.Paths);
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            CompareObjectHelper.PreTraverseFolder(rco, new IgnoreMetaDataVisitor());
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.Config));
            return rco;
        }

        public void Sync(ManualSyncRequest request)
        {
            ManualQueueControl.Instance.AddSyncJob(request);
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
    }
}
