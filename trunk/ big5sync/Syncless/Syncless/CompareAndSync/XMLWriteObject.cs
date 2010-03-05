using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class XMLWriteObject
    {
        private FileChangeType _changeType;
        private string _from, _to, _newHash;
        private long _creationTime, _lastWriteTime, _length;

        public FileChangeType ChangeType
        {
            get { return _changeType; }
        }

        public string From
        {
            get { return _from; }
        }

        public string To
        {
            get { return _to; }
        }

        public string NewHash
        {
            get { return _newHash; }
        }

        public long CreationTime
        {
            get { return _creationTime; }
        }

        public long LastWriteTime
        {
            get { return _lastWriteTime; }
        }

        public long Length
        {
            get { return _length; }
        }

        //Files: Used for delete        
        private XMLWriteObject(FileChangeType changeType, string from)
        {
            _changeType = changeType;
            _from = from;
        }
        
        //Files: Used for rename
        private XMLWriteObject(FileChangeType changeType, string from, string to)
            : this(changeType, from)
        {
            _to = to;
        }

        //Files: Used for create/update
        private XMLWriteObject(FileChangeType changeType, string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
            : this (changeType, from, to)
        {
            _newHash = newHash;
            _creationTime = creationTime;
            _lastWriteTime = lastWriteTime;
            _length = length;
        }

        public static XMLWriteObject CreateXMLWriteObject(string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
        {
            return new XMLWriteObject(FileChangeType.Create, from, to, newHash, creationTime, lastWriteTime, length);
        }

        public static XMLWriteObject UpdateXMLWriteObject(string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
        {
            return new XMLWriteObject(FileChangeType.Update, from, to, newHash, creationTime, lastWriteTime, length);
        }

        public static XMLWriteObject RenameXMLWriteObject(string from, string to)
        {
            return new XMLWriteObject(FileChangeType.Rename, from, to);
        }

        public static XMLWriteObject DeleteXMLWriteObject(string from)
        {
            return new XMLWriteObject(FileChangeType.Delete, from);
        }
    }
}
