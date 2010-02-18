using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
namespace Syncless.CompareAndSync
{
    public class CompareSyncController
    {
        private static CompareSyncController _instance;
        public static CompareSyncController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CompareSyncController();
                }
                return _instance;
            }
        }

        private CompareSyncController()
        {

        }

        public void Sync(MonitorSyncRequest syncRequest)
        {

        }

        public List<SyncResult> Sync(SyncRequest syncRequest)
        {            
            if (syncRequest.IsFolder)
            {
                syncRequest.Results = new Comparer().CompareFolder(syncRequest.TagName, syncRequest.Paths);
                return new Syncer().SyncFolder(syncRequest.TagName, syncRequest.Paths, syncRequest.Results);
            }
            else
            {
                syncRequest.Results = new Comparer().CompareFolder(syncRequest.TagName, syncRequest.Paths);
                return new Syncer().SyncFile(syncRequest.TagName, syncRequest.Paths, syncRequest.Results);
            }
        }

        /// <summary>
        /// Preview the result of a foldertag synchronization
        /// </summary>
        /// <param name="tag">The Folder Tag to preview</param>
        /// <returns>The list of Compare Result</returns>
        public List<CompareResult> Compare(CompareRequest compareRequest)
        {
            if (compareRequest.IsFolder)
            {
                return new Comparer().CompareFolder(compareRequest.TagName, compareRequest.Paths);
            }
            else
            {
                return new Comparer().CompareFile(compareRequest.TagName, compareRequest.Paths);
            }
            
        }

    }
}
