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
        private DateTime _lastWriteTime;
        long _length;
        FileChangeType _changeType;

        public CompareInfoObject(string origin, string fullName, string name, DateTime lastWriteTime, long length, string hash)
        {
            _origin = origin;
            _fullName = fullName;
            _name = name;
            _lastWriteTime = lastWriteTime;
            _length = length;
            _hash = hash;
            _changeType = FileChangeType.None;
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

        public DateTime LastWriteTime
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
    }
}
