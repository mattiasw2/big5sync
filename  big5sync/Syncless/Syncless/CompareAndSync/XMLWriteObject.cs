using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class XMLWriteObject
    {
        private FileChangeType _changeType;
        private string _origin, _from, _to, _newHash;
        private long _creationTime, _lastWriteTime, _length;

        //Files: Used for delete        
        private XMLWriteObject(FileChangeType changeType, string origin, string from)
        {
            _changeType = changeType;
            _origin = origin;
            _from = from;
        }
        
        //Files: Used for rename
        private XMLWriteObject(FileChangeType changeType, string origin, string from, string to)
            : this(changeType, origin, from)
        {
            _to = to;
        }

        //Files: Used for create/update
        private XMLWriteObject(FileChangeType changeType, string origin, string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
            : this (changeType, origin, from, to)
        {
            _newHash = newHash;
            _creationTime = creationTime;
            _lastWriteTime = lastWriteTime;
            _length = length;
        }

        public static XMLWriteObject CreateXMLWriteObject(string origin, string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
        {
            return new XMLWriteObject(FileChangeType.Create, origin, from, to, newHash, creationTime, lastWriteTime, length);
        }

        public static XMLWriteObject UpdateXMLWriteObject(string origin, string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
        {
            return new XMLWriteObject(FileChangeType.Update, origin, from, to, newHash, creationTime, lastWriteTime, length);
        }

        public static XMLWriteObject RenameXMLWriteObject(string origin, string from, string to)
        {
            return new XMLWriteObject(FileChangeType.Rename, origin, from, to);
        }

        public static XMLWriteObject DeleteXMLWriteObject(string origin, string from)
        {
            return new XMLWriteObject(FileChangeType.Delete, origin, from);
        }
    }
}
