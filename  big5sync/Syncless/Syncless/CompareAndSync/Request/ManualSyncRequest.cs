using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public class ManualSyncRequest : ManualRequest
    {
        private string _archiveFolder;
        private int _archiveLimit;
        private SyncConfig _syncConfig;

        public ManualSyncRequest(string[] paths, string[] unavailablePaths, List<Filter> filters, string archiveFolder, int archiveLimit, SyncConfig syncConfig)
            : base(paths, unavailablePaths, filters)
        {
            _archiveFolder = archiveFolder;
            _archiveLimit = archiveLimit;
            _syncConfig = syncConfig;
        }

        public string ArchiveFolder
        {
            get { return _archiveFolder; }
        }

        public int ArchiveLimit
        {
            get { return _archiveLimit; }
        }

        public SyncConfig Config
        {
            get { return _syncConfig; }
        }
    }
}
