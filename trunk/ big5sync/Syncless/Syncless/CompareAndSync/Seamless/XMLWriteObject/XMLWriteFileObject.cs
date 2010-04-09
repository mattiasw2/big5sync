using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Seamless.XMLWriteObject
{
    public class XMLWriteFileObject : BaseXMLWriteObject
    {
        private readonly long _size;
        private readonly string _hash;
        private readonly long _lastModifiedUtc;

        //Update, create
        public XMLWriteFileObject(string name, string parent, string hash, long size, long creationTimeUtc, long modifiedTimeUtc, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, parent, creationTimeUtc, changeType, metaUpdatedUtc)
        {
            _size = size;
            _hash = hash;
            _lastModifiedUtc = modifiedTimeUtc;
        }

        //Rename
        public XMLWriteFileObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, newName, parent, changeType, metaUpdatedUtc)
        {
        }

        //Delete
        public XMLWriteFileObject(string name, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, parent, changeType, metaUpdatedUtc)
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

        public long LastModifiedUtc
        {
            get { return _lastModifiedUtc; }
        }

    }
}