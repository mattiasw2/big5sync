using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    /// <summary>
    /// Stores only necessary information for file comparison
    /// TODO: Include hash value, changetype soon
    /// </summary>
    public class CompareInfoObject
    {
        private string _origin, _fullName, _name, _hash;
        private long _creationTime, _lastWriteTime, _length;
        private FileChangeType _changeType = FileChangeType.None;

        public CompareInfoObject(string origin, string fullName, string name, long creationTime, long lastWriteTime, long length, string hash)
        {
            _origin = origin;
            _fullName = fullName;
            _name = name;
            _creationTime = creationTime;
            _lastWriteTime = lastWriteTime;
            _length = length;
            _hash = hash;
        }

        public CompareInfoObject(string origin, string fullName, string name, long creationTime, long lastWriteTime, long length, string hash, FileChangeType changeType) :
            this(origin, fullName, name, creationTime, lastWriteTime, length, hash)
        {
            _changeType = changeType;
        }

        public string Origin
        {
            get
            {
                return _origin;
            }
        }

        public string FullName
        {
            get
            {
                return _fullName;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public long CreationTime
        {
            get
            {
                return _creationTime;
            }
        }

        public long LastWriteTime
        {
            get
            {
                return _lastWriteTime;
            }
        }

        public long Length
        {
            get
            {
                return _length;
            }
        }

        public string MD5Hash
        {
            get
            {
                return _hash;
            }
        }

        public string RelativePathToOrigin
        {
            get
            {
                return _fullName.Substring(_origin.Length + 1);
            }
        }

        public FileChangeType ChangeType
        {
            get
            {
                return _changeType;
            }
            set
            {
                _changeType = value;
            }
        }
    }
}
