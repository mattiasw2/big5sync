using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.XMLWriteObject
{
    public class XMLWriteFileObject : BaseXMLWriteObject
    {
        private long _size;
        private string _hash;
        private long _lastModified;       

        //Delete
        public XMLWriteFileObject(string name, string fullPath, MetaChangeType changeType, long metaUpdated)
            : base(name, fullPath, changeType, metaUpdated)
        {
        }

        //Update, create
        public XMLWriteFileObject(string name, string fullPath, string hash, long size, long creationTime, long modifiedTime, MetaChangeType changeType, long metaUpdated)
            : base(name, fullPath, creationTime, changeType, metaUpdated)
        {
            _size = size;
            _hash = hash;
            _lastModified = modifiedTime;
        }

        //Rename
        public XMLWriteFileObject(string name, string newName, string fullPath, string hash, long size, long creationTime, long modifiedTime, MetaChangeType changeType, long metaUpdated)
            : base(name, newName, fullPath, creationTime, changeType, metaUpdated)
        {
            _size = size;
            _hash = hash;
            _lastModified = modifiedTime;
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
