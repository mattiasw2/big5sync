using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class SyncConfig
    {
        private string _archiveName;
        private int _archiveLimit;

        public SyncConfig(string archiveName, int archiveLimit)
        {
            _archiveName = archiveName;
            _archiveLimit = archiveLimit;           
        }

        public string ArchiveName
        {
            get { return _archiveName; }
        }

        public int ArchiveLimit
        {
            get { return _archiveLimit; }
        }
    }
}
