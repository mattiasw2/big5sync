using Syncless.CompareAndSync.Manual.Visitor;

namespace Syncless.CompareAndSync.Manual.CompareObject
{
    /// <summary>
    /// Used by <see cref="XMLMetadataVisitor"/> for the purpose of populating and creating new <see cref="BaseCompareObject"/> nodes.
    /// </summary>
    public class XMLCompareObject
    {
        private long _size;
        private string _name;
        private long _createdTimeUtc;
        private long _lastModifiedTimeUtc;
        private string _hash;
        private long _lastUpdatedTimeUtc;

        /// <summary>
        /// Instantiates an XMLCompareObject.
        /// </summary>
        /// <param name="name">The name from the metadata.</param>
        /// <param name="hash">The hash from the metadata.</param>
        /// <param name="size">The size from the metadata.</param>
        /// <param name="createdTime">The created time from the metadata.</param>
        /// <param name="modifiedTime">The modified time from the metadata.</param>
        /// <param name="updatedTime">The updated time from the metadata.</param>
        public XMLCompareObject(string name, string hash, long size, long createdTimeUtc, long modifiedTimeUtc, long updatedTimeUtc)
        {
            _size = size;
            _name = name;
            _hash = hash;
            _createdTimeUtc = createdTimeUtc;
            _lastModifiedTimeUtc = modifiedTimeUtc;
            _lastUpdatedTimeUtc = updatedTimeUtc;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the name of the <c>XMLCompareObject</c>.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the size of the <c>XMLCompareObject</c>.
        /// </summary>
        public long Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Gets or sets the hash of the <c>XMLCompareObject</c>.
        /// </summary>
        public string Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        /// <summary>
        /// Gets or sets the last modified time of the <c>XMLCompareObject</c>.
        /// </summary>
        public long LastModifiedTimeUtc
        {
            get { return _lastModifiedTimeUtc; }
            set { _lastModifiedTimeUtc = value; }
        }

        /// <summary>
        /// Gets or sets the created time of the <c>XMLCompareObject</c>.
        /// </summary>
        public long CreatedTimeUtc
        {
            get { return _createdTimeUtc; }
            set { _createdTimeUtc = value; }
        }

        /// <summary>
        /// Gets or sets the last updated time of the <c>XMLCompareObject</c>.
        /// </summary>
        public long LastUpdatedTimeUtc
        {
            get { return _lastUpdatedTimeUtc; }
            set { _lastUpdatedTimeUtc = value; }
        }

        #endregion

    }
}