namespace Syncless.CompareAndSync.Manual.CompareObject
{
    public class XMLCompareObject
    {
        private long _size;
        private string _name;
        private long _createdTime;
        private long _lastModifiedTime;
        private string _hash;
        private long _lastUpdatedTime;

        public XMLCompareObject(string newName, string newHash, long newSize, long newCreatedTime, long newModifiedTime , long newUpdatedTime)
        {
            _size = newSize;
            _name = newName;
            _hash = newHash;
            _createdTime = newCreatedTime;
            _lastModifiedTime = newModifiedTime;
            _lastUpdatedTime = newUpdatedTime;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public long Size
        {
            get { return _size; }
            set { _size = value;}
        }

        public string Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        public long LastModifiedTime
        {
            get { return _lastModifiedTime; }
            set { _lastModifiedTime = value; }
        }

        public long CreatedTime
        {
            get { return _createdTime; }
            set { _createdTime = value; }
        }

        public long LastUpdatedTime
        {
            get { return _lastUpdatedTime; }
            set { _lastUpdatedTime = value; }
        }
    }
}