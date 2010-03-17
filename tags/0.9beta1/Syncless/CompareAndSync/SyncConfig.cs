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
        private bool _recycle;

        public bool Recycle
        {
            get { return _recycle; }
            set { _recycle = value; }
        }

        public SyncConfig(string archiveName, int archiveLimit , bool recycle)
        {
            _archiveName = archiveName;
            _archiveLimit = archiveLimit;
            _recycle = recycle;
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
