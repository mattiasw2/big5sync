using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Visitor;
using Syncless.Filters;

namespace Syncless.CompareAndSync
{
    public static class ManualRequestHelper
    {

        /// <summary>
        /// Sync a list of folders without processing any metadata
        /// </summary>
        /// <param name="request">ManualSyncRequest object</param>
        /// <returns>RootCompareObject to be further processed, if necessary</returns>
        public static RootCompareObject SyncFolders(ManualSyncRequest request)
        {
            CreateRootIfNotExist(request.Paths);
            RootCompareObject rco = new RootCompareObject(request.Paths);
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            CompareObjectHelper.PreTraverseFolder(rco, new IgnoreMetaDataVisitor());
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.Config));
            return rco;
        }

        public static RootCompareObject Sync(ManualSyncRequest request)
        {
            List<Filter> filters = request.Filters.ToList<Filter>();
            filters.Add(new SynclessArchiveFilter(request.Config.ArchiveName));

            RootCompareObject rco = new RootCompareObject(request.Paths);
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(filters));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor());
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor(request.Config));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor());
            return rco;
        }

        public static RootCompareObject Compare(ManualCompareRequest request)
        {
            RootCompareObject rco = new RootCompareObject(request.Paths);
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor(request.Filters));
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new FolderRenameVisitor());
            CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
            return rco;
        }

        /// <summary>
        /// Temporary method to handle non-existent folders until we handle deletion of tagged folders
        /// </summary>
        /// <param name="paths"></param>
        private static void CreateRootIfNotExist(string[] paths)
        {
            foreach (string path in paths)
            {
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
            }
        }

    }
}
