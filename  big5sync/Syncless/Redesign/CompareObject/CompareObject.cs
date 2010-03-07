using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redesign
{
    public abstract class CompareObject
    {
        protected string _origin, _fullName, _name;
        protected long _creationTime;
        protected ChangeType _changeType = ChangeType.Unknown;

        protected CompareObject(string origin)
        {
            _origin = origin;
        }

        protected CompareObject(string origin, string fullName, string name, long creationTime)
        {
            _origin = origin;
            _fullName = fullName;
            _name = name;
            _creationTime = creationTime;
        }

        public string Origin
        {
            get { return _origin; }
        }

        public string FullName
        {
            get { return _fullName; }
        }

        public string Name
        {
            get { return _name; }
        }

        public long CreationTime
        {
            get { return _creationTime; }
        }

        public ChangeType ChangeType
        {
            get { return _changeType; }
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
