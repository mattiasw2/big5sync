using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
using System.IO;

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
            List<CompareResult> results = new List<CompareResult>();
            List<string> paths = new List<string>();

            switch (syncRequest.ChangeType)
            {
                case FileChangeType.Create:
                case FileChangeType.Update:
                    foreach (MonitorPathPair dest in syncRequest.Dest)
                    {
                        results.Add(new CompareResult(syncRequest.ChangeType, syncRequest.OldPath.FullPath, dest.FullPath, syncRequest.IsFolder));
                    }
                    break;
                case FileChangeType.Delete:
                    foreach (MonitorPathPair dest in syncRequest.Dest)
                    {
                        results.Add(new CompareResult(syncRequest.ChangeType, dest.FullPath, syncRequest.IsFolder));
                    }
                    break;                
                case FileChangeType.Rename:
                    FileInfo file = new FileInfo(syncRequest.NewPath.FullPath);
                    string fileName = file.Name;
                    foreach (MonitorPathPair dest in syncRequest.Dest)
                    {
                        string newDestPath = new FileInfo(dest.FullPath).DirectoryName;
                        results.Add(new CompareResult(syncRequest.ChangeType, dest.FullPath, Path.Combine(newDestPath, fileName), syncRequest.IsFolder));
                    }
                    break;
            }

            paths.AddRange(syncRequest.OldPath.Origin);
            paths.AddRange(syncRequest.NewPath.Origin);

            foreach (MonitorPathPair dest in syncRequest.Dest)
            {
                paths.AddRange(dest.Origin);
            }

            paths.Distinct<string>();

            new Syncer().SyncFolder(paths, results);
        }

        public List<SyncResult> Sync(SyncRequest syncRequest)
        {            
            if (syncRequest.IsFolder)
            {
                syncRequest.Results = new Comparer().CompareFolder(syncRequest.Paths);
                return new Syncer().SyncFolder(syncRequest.Paths, syncRequest.Results);
            }
            else
            {
                syncRequest.Results = new Comparer().CompareFolder(syncRequest.Paths);
                return new Syncer().SyncFile(syncRequest.Paths, syncRequest.Results);
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
                return new Comparer().CompareFolder(compareRequest.Paths);
            }
            else
            {
                return new Comparer().CompareFile(compareRequest.Paths);
            }
            
        }

    }
}
