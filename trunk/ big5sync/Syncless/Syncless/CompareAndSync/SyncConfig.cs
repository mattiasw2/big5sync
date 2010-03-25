using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class SyncConfig
    {

        private static SyncConfig _instance;
        public static SyncConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SyncConfig("_synclessArchive", 5, true);
                }
                return _instance;
            }
        }

        

        private string _archiveName;
        private int _archiveLimit;
        private bool _recycle;

        public bool Recycle
        {
            get { return _recycle; }
            set { _recycle = value; }
        }

        private SyncConfig(string archiveName, int archiveLimit , bool recycle)
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
