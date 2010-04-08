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
        private long _createdTime;
        private long _lastModifiedTime;
        private string _hash;
        private long _lastUpdatedTime;

        /// <summary>
        /// Instantiates an XMLCompareObject.
        /// </summary>
        /// <param name="name">The name from the metadata.</param>
        /// <param name="hash">The hash from the metadata.</param>
        /// <param name="size">The size from the metadata.</param>
        /// <param name="createdTime">The created time from the metadata.</param>
        /// <param name="modifiedTime">The modified time from the metadata.</param>
        /// <param name="updatedTime">The updated time from the metadata.</param>
        public XMLCompareObject(string name, string hash, long size, long createdTime, long modifiedTime , long updatedTime)
        {
            _size = size;
            _name = name;
            _hash = hash;
            _createdTime = createdTime;
            _lastModifiedTime = modifiedTime;
            _lastUpdatedTime = updatedTime;
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
            set { _size = value;}
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
        public long LastModifiedTime
        {
            get { return _lastModifiedTime; }
            set { _lastModifiedTime = value; }
        }

        /// <summary>
        /// Gets or sets the created time of the <c>XMLCompareObject</c>.
        /// </summary>
        public long CreatedTime
        {
            get { return _createdTime; }
            set { _createdTime = value; }
        }

        /// <summary>
        /// Gets or sets the last updated time of the <c>XMLCompareObject</c>.
        /// </summary>
        public long LastUpdatedTime
        {
            get { return _lastUpdatedTime; }
            set { _lastUpdatedTime = value; }
        }

        #endregion

    }
}