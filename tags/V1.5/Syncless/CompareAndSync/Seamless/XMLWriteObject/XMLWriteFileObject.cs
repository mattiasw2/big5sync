using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Seamless.XMLWriteObject
{
    /// <summary>
    /// <c>XMLWriteFileObject</c> stores information about file metadata objects.
    /// </summary>
    public class XMLWriteFileObject : BaseXMLWriteObject
    {
        private readonly long _size;
        private readonly string _hash;
        private readonly long _lastModifiedUtc;

        /// <summary>
        /// Instantiates a <c>XMLWriteFileObject</c>. Used for create and update.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="parent">The parent of the file.</param>
        /// <param name="hash">The hash of the file.</param>
        /// <param name="size">The size of the file.</param>
        /// <param name="creationTimeUtc">The creation time of the file.</param>
        /// <param name="modifiedTimeUtc">The modified time of the file.</param>
        /// <param name="changeType">The change type of the file.</param>
        /// <param name="metaUpdatedUtc">The metadata updated time of the file.</param>
        public XMLWriteFileObject(string name, string parent, string hash, long size, long creationTimeUtc, long modifiedTimeUtc, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, parent, creationTimeUtc, changeType, metaUpdatedUtc)
        {
            _size = size;
            _hash = hash;
            _lastModifiedUtc = modifiedTimeUtc;
        }

        /// <summary>
        /// Instantiates a <c>XMLWriteFileObject</c>. Used for rename.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="newName">The new name of the file.</param>
        /// <param name="parent">The parent of the file.</param>
        /// <param name="changeType">The change type of the file.</param>
        /// <param name="metaUpdatedUtc">The metadata updated time of the file.</param>
        public XMLWriteFileObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, newName, parent, changeType, metaUpdatedUtc)
        {
        }

        /// <summary>
        /// Instantiates a <c>XMLWriteFileObject</c>. Used for delete.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="parent">The parent of the file.</param>
        /// <param name="changeType">The change type of the file.</param>
        /// <param name="metaUpdatedUtc">The metadata updated time of the file.</param>
        public XMLWriteFileObject(string name, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, parent, changeType, metaUpdatedUtc)
        {
        }

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        public long Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the size of the hash.
        /// </summary>
        public string Hash
        {
            get { return _hash; }
        }

        /// <summary>
        /// Gets the last modified time of the file.
        /// </summary>
        public long LastModifiedUtc
        {
            get { return _lastModifiedUtc; }
        }

    }
}