using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request {
    public class ManualSyncRequest : ManualRequest {
        private string _archiveFolder;
        private int _archiveLimit;

        public ManualSyncRequest(string[] paths, string[] unavailablePaths, List<Filter> filters, string archiveFolder, int archiveLimit)
            : base(paths, unavailablePaths, filters) {
            _archiveFolder = archiveFolder;
            _archiveLimit = archiveLimit;
        }

        public string ArchiveFolder {
            get { return _archiveFolder; }
        }

        public int ArchiveLimit {
            get { return _archiveLimit; }
        }
    }
}
