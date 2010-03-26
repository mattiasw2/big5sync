using System.Collections.Generic;
using System.Text;

namespace SynclessSeamlessTester
{
    public class TestInfo
    {
        private bool _compared, _pass, _propagated;
        private List<LazyFileCompare> _fileResults;
        private List<LazyFolderCompare> _folderResults;

        public bool Compared
        {
            get { return _compared; }
            set { _compared = value; }
        }

        public bool Passed
        {
            get { return _pass; }
            set { _pass = value; }
        }

        public bool Propagated
        {
            get { return _propagated; }
            set { _propagated = value; }
        }

        public List<LazyFileCompare> FileResults
        {
            get { return _fileResults; }
            set { _fileResults = value; }
        }

        public List<LazyFolderCompare> FolderResults
        {
            get { return _folderResults; }
            set { _folderResults = value; }
        }

    }

    public abstract class LazyObjectCompare
    {
        private readonly string _relativeName, _fullPath;

        protected LazyObjectCompare(string relativeName, string fullPath)
        {
            _relativeName = relativeName;
            _fullPath = fullPath;
        }

        public string RelativeName
        {
            get { return _relativeName; }
        }

        public string FullPath
        {
            get { return _fullPath; }
        }
    }

    public class LazyFileCompare : LazyObjectCompare
    {
        private readonly string _hash;

        public LazyFileCompare(string hash, string relativeName, string fullPath)
            : base(relativeName, fullPath)
        {
            _hash = hash;
        }

        public string Hash
        {
            get { return _hash; }
        }
    }

    public class LazyFolderCompare : LazyObjectCompare
    {

        public LazyFolderCompare(string relativeName, string fullPath)
            : base(relativeName, fullPath)
        {
        }
    }
}
