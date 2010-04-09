using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Seamless.XMLWriteObject
{
    public abstract class BaseXMLWriteObject
    {
        private readonly string _name, _parent, _newName;
        private readonly long _creationTimeUtc;
        private readonly MetaChangeType _changeType;
        private readonly long _metaUpdatedUtc;

        protected BaseXMLWriteObject(string name, string parent, MetaChangeType changeType, long metaUpdatedUtc)
        {
            _name = name;
            _parent = parent;
            _changeType = changeType;
            _metaUpdatedUtc = metaUpdatedUtc;
        }

        protected BaseXMLWriteObject(string name, string parent, long creationTimeUtc, MetaChangeType changeType, long metaUpdatedUtc)
            : this(name, parent, changeType, metaUpdatedUtc)
        {
            _creationTimeUtc = creationTimeUtc;
        }

        protected BaseXMLWriteObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : this(name, parent, changeType, metaUpdatedUtc)
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

        public long CreationTimeUtc
        {
            get { return _creationTimeUtc; }
        }

        public MetaChangeType ChangeType
        {
            get { return _changeType; }
        }

        public string NewName
        {
            get { return _newName; }
        }

        public long MetaUpdatedUtc
        {
            get { return _metaUpdatedUtc; }
        }

    }
}