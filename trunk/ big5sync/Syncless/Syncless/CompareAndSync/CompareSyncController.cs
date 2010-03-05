using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
using System.IO;
using System.Diagnostics;
using Syncless.Core;

namespace Syncless.CompareAndSync
{
    public class CompareSyncController
    {
        private IOriginsFinder _originsFinder;

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

        public bool Init(IOriginsFinder originsFinder)
        {
            _originsFinder = originsFinder;
            return true;
        }

        public void Sync(MonitorSyncRequest syncRequest)
        {
            List<string> paths = null;
            List<CompareResult> results = null;
            switch (syncRequest.IsFolder)
            {
                case IsFolder.Yes:
                    new Comparer().MonitorCompareFolder(syncRequest, out paths, out results);
                    break;
                case IsFolder.No:
                    new Comparer().MonitorCompareFile(syncRequest, out paths, out results);
                    break;
                case IsFolder.Unknown:
                    break;
            }

            new Syncer().SyncFolder(paths, results, _originsFinder);
        }

        public List<SyncResult> Sync(SyncRequest syncRequest)
        {
            syncRequest.Results = new Comparer().CompareFolder(syncRequest.Paths);
            return new Syncer().SyncFolder(syncRequest.Paths, syncRequest.Results, _originsFinder);
        }

        /// <summary>
        /// Preview the result of a foldertag synchronization
        /// </summary>
        /// <param name="tag">The Folder Tag to preview</param>
        /// <returns>The list of Compare Result</returns>
        public List<CompareResult> Compare(CompareRequest compareRequest)
        {
            return new Comparer().CompareFolder(compareRequest.Paths);            
        }

    }
}
