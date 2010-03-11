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
        private bool _recycled;

        public bool Recycled
        {
            get { return _recycled; }
            set { _recycled = value; }
        }

        public SyncConfig(string archiveName, int archiveLimit , bool recycle)
        {
            _archiveName = archiveName;
            _archiveLimit = archiveLimit;
            _recycled = recycle;
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
