/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

namespace Syncless.CompareAndSync
{
    /// <summary>
    /// The object containing all the SyncConfiguration
    /// </summary>
    public class SyncConfig
    {
        
        private static SyncConfig _instance;
        
        /// <summary>
        /// Get and Set the Archive Name
        /// </summary>
        public string ArchiveName { get; set; }

        /// <summary>
        /// Get and Set the Archive Limit
        /// </summary>
        public int ArchiveLimit { get; set; }

        /// <summary>
        /// Get and Set the Conflict Directory
        /// </summary>
        public string ConflictDir { get; set; }

        /// <summary>
        /// Get and Set if to Recycle deletedFiles.
        /// </summary>
        public bool Recycle { get; set; }

        /// <summary>
        /// Private initalizer
        /// </summary>
        /// <param name="archiveName">Name of archive</param>
        /// <param name="archiveLimit">Archive Limit</param>
        /// <param name="recycle">Recycle enabled</param>
        private SyncConfig(string archiveName, int archiveLimit, bool recycle)
        {
            ArchiveName = archiveName;
            ArchiveLimit = archiveLimit;
            Recycle = recycle;
            ConflictDir = "_synclessConflict";
        }

        /// <summary>
        /// Return the instance of Sync Config
        /// </summary>
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

        /// <summary>
        /// Return a Copy of Sync Config
        /// </summary>
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

        private SyncConfig Clone()
        {
            SyncConfig config = new SyncConfig(ArchiveName, ArchiveLimit, Recycle);
            return config;
        }

    }
}
