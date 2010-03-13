using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.XMLWriteObject
{
    public abstract class BaseXMLWriteObject
    {
        private string _name, _fullPath, _newName;
        private long _creationTime;
        private MetaChangeType _changeType;

        public BaseXMLWriteObject(string name, string fullPath, MetaChangeType changeType)
        {
            _name = name;
            _fullPath = fullPath;
            _changeType = changeType;
        }

        public BaseXMLWriteObject(string name, string fullPath, long creationTime, MetaChangeType changeType)
            : this(name, fullPath, changeType)
        {
            _creationTime = creationTime;
        }

        public BaseXMLWriteObject(string name, string newName, string fullPath, long creationTime, MetaChangeType changeType)
            : this(name, fullPath, creationTime, changeType)
        {
            _newName = newName;
        }

        public string Name
        {
            get { return _name; }
        }

        public string FullPath
        {
            get { return _fullPath; }
        }

        public long CreationTime
        {
            get { return _creationTime; }
        }

        public MetaChangeType ChangeType
        {
            get { return _changeType; }
        }

        public string NewName
        {
            get { return _newName; }
        }

    }
}
