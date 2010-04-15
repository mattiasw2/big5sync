/*
 * 
 * Author: Gordon Hoi Chi Kit
 * 
 */

using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Seamless.XMLWriteObject
{
    /// <summary>
    /// Abstract base class for all XML write objects.
    /// </summary>
    public abstract class BaseXMLWriteObject
    {
        private readonly string _name, _parent, _newName;
        private readonly long _creationTimeUtc;
        private readonly MetaChangeType _changeType;
        private readonly long _metaUpdatedUtc;

        /// <summary>
        /// Instantiates an instance of <c>BaseXMLWriteObject</c> for create and update purposes.
        /// </summary>
        /// <param name="name">The name of the file or folder object.</param>
        /// <param name="parent">The parent of the file or folder object.</param>
        /// <param name="creationTimeUtc">The creation time of the file or folder.</param>
        /// <param name="changeType">The type of change to write to XML.</param>
        /// <param name="metaUpdatedUtc">The time when the metadata was updated.</param>
        protected BaseXMLWriteObject(string name, string parent, long creationTimeUtc, MetaChangeType changeType, long metaUpdatedUtc)
            : this(name, parent, changeType, metaUpdatedUtc)
        {
            _creationTimeUtc = creationTimeUtc;
        }

        /// <summary>
        /// Instantiates an instance of <c>BaseXMLWriteObject</c> for delete purposes.
        /// </summary>
        /// <param name="name">The name of the file or folder object.</param>
        /// <param name="parent">The parent of the file or folder object.</param>
        /// <param name="changeType">The type of change to write to XML.</param>
        /// <param name="metaUpdatedUtc">The time when the metadata was updated.</param>
        protected BaseXMLWriteObject(string name, string parent, MetaChangeType changeType, long metaUpdatedUtc)
        {
            _name = name;
            _parent = parent;
            _changeType = changeType;
            _metaUpdatedUtc = metaUpdatedUtc;
        }

        /// <summary>
        /// Instantiates an instance of <c>BaseXMLWriteObject</c> for rename purposes.
        /// </summary>
        /// <param name="name">The name of the file or folder object.</param>
        /// <param name="newName">The new name of the file or folder object.</param>
        /// <param name="parent">The parent of the file or folder object.</param>
        /// <param name="changeType">The type of change to write to XML.</param>
        /// <param name="metaUpdatedUtc">The time when the metadata was updated.</param>
        protected BaseXMLWriteObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : this(name, parent, changeType, metaUpdatedUtc)
        {
            _newName = newName;
        }

        /// <summary>
        /// Gets the name of the write object.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the parent of the write object.
        /// </summary>
        public string Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Gets the creation time of the write object.
        /// </summary>
        public long CreationTimeUtc
        {
            get { return _creationTimeUtc; }
        }

        /// <summary>
        /// Gets the change type of the write object.
        /// </summary>
        public MetaChangeType ChangeType
        {
            get { return _changeType; }
        }

        /// <summary>
        /// Gets the new name of the write object.
        /// </summary>
        public string NewName
        {
            get { return _newName; }
        }

        /// <summary>
        /// Gets the metadata updated time of the write object.
        /// </summary>
        public long MetaUpdatedUtc
        {
            get { return _metaUpdatedUtc; }
        }

    }
}