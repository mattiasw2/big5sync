
namespace Syncless.Tagging
{
    internal class TagConfig
    {
        private bool _isSeamless;
        private int _archiveCount;
        private string _archiveFolderName;

        public bool IsSeamless
        {
            get { return _isSeamless; }
            set { _isSeamless = value; }
        }

        public string ArchiveFolderName
        {
            get { return _archiveFolderName; }
            set { _archiveFolderName = value; }
        }
        
        public int ArchiveCount
        {
            get { return _archiveCount; }
            set { _archiveCount = value; }
        }

        public TagConfig()
        {
            _isSeamless = true;
            _archiveCount = 5;
            _archiveFolderName = "_synclessArchive";
        }
    }
}
