namespace Syncless.CompareAndSync
{
    public class SyncConfig
    {

        private static SyncConfig _instance;
        private string _archiveName;
        private int _archiveLimit;
        private bool _recycle;
        private string _conflictDir;

        private SyncConfig(string archiveName, int archiveLimit, bool recycle)
        {
            _archiveName = archiveName;
            _archiveLimit = archiveLimit;
            _recycle = recycle;
            _conflictDir = "_synclessConflict";
        }

        internal static SyncConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SyncConfig("_synclessArchive", 2, false);
                }
                return _instance;
            }
            set
            {
                
                if (_instance == null)
                {
                    _instance = new SyncConfig(value.ArchiveName, value.ArchiveLimit, value.Recycle);
                    return;
                }
                lock (_instance)
                {
                    _instance.ArchiveName = value.ArchiveName;
                    _instance.ArchiveLimit = value.ArchiveLimit;
                    _instance.Recycle = value.Recycle;
                    _instance.ConflictDir = value.ConflictDir;
                }
            }
        }

        internal static SyncConfig Copy
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SyncConfig("_synclessArchive", 2, false);
                }
                return _instance.Clone();
            }
        }

        public bool Recycle
        {
            get { return _recycle; }
            set { _recycle = value; }
        }
        private SyncConfig Clone()
        {
            SyncConfig config = new SyncConfig(ArchiveName, ArchiveLimit, Recycle);
            return config;
        }

        public string ArchiveName
        {
            get { return _archiveName; }
            set { _archiveName = value; }
        }

        public int ArchiveLimit
        {
            get { return _archiveLimit; }
            set { _archiveLimit = value; }
        }

        public string ConflictDir
        {
            get { return _conflictDir; }
            set { _conflictDir = value; }
        }

    }
}
