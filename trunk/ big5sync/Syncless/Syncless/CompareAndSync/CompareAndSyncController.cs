using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Visitor;

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

        public RootCompareObject Sync(ManualSyncRequest request)
        {
            RootCompareObject rco = new RootCompareObject(request.Paths);
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.ArchiveFolder, request.ArchiveLimit));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor());
            return rco;
        }

        public RootCompareObject Compare(ManualCompareRequest request)
        {
            RootCompareObject rco = new RootCompareObject(request.Paths);
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            return rco;
        }

        public void Sync(AutoSyncRequest request)
        {
            SeamlessSyncer.Sync(request);
        }

    }
}
