using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Seamless.XMLWriteObject
{
    public class XMLWriteFileObject : BaseXMLWriteObject
    {
        private readonly long _size;
        private readonly string _hash;
        private readonly long _lastModified;

        //Update, create
        public XMLWriteFileObject(string name, string parent, string hash, long size, long creationTime, long modifiedTime, MetaChangeType changeType, long metaUpdated)
            : base(name, parent, creationTime, changeType, metaUpdated)
        {
            _size = size;
            _hash = hash;
            _lastModified = modifiedTime;
        }

        //Rename
        public XMLWriteFileObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdated)
            : base(name, newName, parent, changeType, metaUpdated)
        {
        }

        //Delete
        public XMLWriteFileObject(string name, string parent, MetaChangeType changeType, long metaUpdated)
            : base(name, parent, changeType, metaUpdated)
        {
        }

        public long Size
        {
            get { return _size; }
        }

        public string Hash
        {
            get { return _hash; }
        }

        public long LastModified
        {
            get { return _lastModified; }
        }

    }
}