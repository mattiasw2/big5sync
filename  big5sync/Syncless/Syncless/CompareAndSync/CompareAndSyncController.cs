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
        /// Sync a list of folders, without tagging or writing to metadata (if it exists)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RootCompareObject SyncFolders(ManualSyncRequest request)
        {
            ManualQueueControl.Instance.AddSyncJob(request);
            //RootCompareObject rco = new RootCompareObject(request.Paths);
            //CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            //CompareObjectHelper.PreTraverseFolder(rco, new IgnoreMetaDataVisitor());
            //CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            //CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.Config));
            //return rco;
            return null;
        }

        public RootCompareObject Sync(ManualSyncRequest request)
        {
            ManualQueueControl.Instance.AddSyncJob(request);
            //List<Filter> filters = request.Filters.ToList<Filter>();
            //filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));

            //RootCompareObject rco = new RootCompareObject(request.Paths);
            //CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters));
            //CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            //CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor());
            //CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            //CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.Config));
            //CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor());
            //return rco;
            return null;
        }

        public RootCompareObject Compare(ManualCompareRequest request)
        {
            ManualQueueControl.Instance.AddSyncJob(request);
            //TODO: Add config into ManualCompareRequest
            //List<Filter> filters = request.Filters.ToList<Filter>();
            //filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));

            //RootCompareObject rco = new RootCompareObject(request.Paths);
            //CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            //CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            //CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor());
            //CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            //return rco;
            return null;
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
