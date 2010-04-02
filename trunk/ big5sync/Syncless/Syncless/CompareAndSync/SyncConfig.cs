namespace Syncless.CompareAndSync
{
    public class SyncConfig
    {

        private static SyncConfig _instance;
        private readonly string _archiveName;
        private readonly int _archiveLimit;
        private readonly bool _recycle;
        private readonly string _conflictDir;

        private SyncConfig(string archiveName, int archiveLimit, bool recycle)
        {
            _archiveName = archiveName;
            _archiveLimit = archiveLimit;
            _recycle = recycle;
            _conflictDir = "_synclessConflict";
        }

        public static SyncConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SyncConfig("_synclessArchive", 2, false);
                }
                return _instance;
            }
        }

        public bool Recycle
        {
            get { return _recycle; }
        }

        public string ArchiveName
        {
            get { return _archiveName; }
        }

        public int ArchiveLimit
        {
            get { return _archiveLimit; }
        }

        public string ConflictDir
        {
            get { return _conflictDir; }
        }
    }
}
