using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Seamless.XMLWriteObject
{
    /// <summary>
    /// <c>XMLWriteFolderObject</c> stores information about folder metadata objects.
    /// </summary>
    public class XMLWriteFolderObject : BaseXMLWriteObject
    {
        /// <summary>
        /// Instantiates an instance of <c>XMLWriteFolderObject</c> for create and update purposes.
        /// </summary>
        /// <param name="name">The name of the folder object.</param>
        /// <param name="parent">The parent of the folder object.</param>
        /// <param name="creationTimeUtc">The creation time of the folder.</param>
        /// <param name="changeType">The type of change to write to XML.</param>
        /// <param name="metaUpdatedUtc">The time when the metadata was updated.</param>
        public XMLWriteFolderObject(string name, string parent, long creationTimeUtc, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, parent, creationTimeUtc, changeType, metaUpdatedUtc)
        {
        }

        /// <summary>
        /// Instantiates an instance of <c>XMLWriteFolderObject</c> for delete purposes.
        /// </summary>
        /// <param name="name">The name of the folder object.</param>
        /// <param name="parent">The parent of the folder object.</param>
        /// <param name="changeType">The type of change to write to XML.</param>
        /// <param name="metaUpdatedUtc">The time when the metadata was updated.</param>
        public XMLWriteFolderObject(string name, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, parent, changeType, metaUpdatedUtc)
        {
        }

        /// <summary>
        /// Instantiates an instance of <c>XMLWriteFolderObject</c> for rename purposes.
        /// </summary>
        /// <param name="name">The name of the folder object.</param>
        /// <param name="newName">The new name of the folder object.</param>
        /// <param name="parent">The parent of the folder object.</param>
        /// <param name="changeType">The type of change to write to XML.</param>
        /// <param name="metaUpdatedUtc">The time when the metadata was updated.</param>
        public XMLWriteFolderObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, newName, parent, changeType, metaUpdatedUtc)
        {
        }



    }
}