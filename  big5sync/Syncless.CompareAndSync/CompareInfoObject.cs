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
        private string _origin, _fullName, _name;
        private DateTime _lastWriteTime;

        public CompareInfoObject(string origin, string fullName, string name, DateTime lastWriteTime)
        {
            _origin = origin;
            _fullName = fullName;
            _name = name;
            _lastWriteTime = lastWriteTime;
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
                return LastWriteTime;
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
