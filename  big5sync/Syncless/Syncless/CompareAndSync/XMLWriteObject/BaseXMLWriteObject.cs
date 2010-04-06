using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.XMLWriteObject
{
    public abstract class BaseXMLWriteObject
    {
        private readonly string _name, _parent, _newName;
        private readonly long _creationTime;
        private readonly MetaChangeType _changeType;
        private readonly long _metaUpdated;

        protected BaseXMLWriteObject(string name, string parent, MetaChangeType changeType, long metaUpdated)
        {
            _name = name;
            _parent = parent;
            _changeType = changeType;
            _metaUpdated = metaUpdated;
        }

        protected BaseXMLWriteObject(string name, string parent, long creationTime, MetaChangeType changeType, long metaUpdated)
            : this(name, parent, changeType, metaUpdated)
        {
            _creationTime = creationTime;
        }

        protected BaseXMLWriteObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdated)
            : this(name, parent, changeType, metaUpdated)
        {
            _newName = newName;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Parent
        {
            get { return _parent; }
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

        public long MetaUpdated
        {
            get { return _metaUpdated; }
        }

    }
}
