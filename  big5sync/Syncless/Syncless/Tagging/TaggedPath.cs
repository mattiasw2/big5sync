/*
 * 
 * Author: Goh Khoon Hiang
 * 
 */

using Syncless.Helper;

namespace Syncless.Tagging
{
    /// <summary>
    /// TaggedPath class encloses properties of a folder path and provides operations to manipulate 
    /// the properties
    /// </summary>
    public class TaggedPath
    {
        private string _logicalid;
        private string _pathName;
        private long _lastUpdatedDate;
        private long _createdDate;
        private bool _isDeleted;
        private long _deletedDate;

        /// <summary>
        /// Gets or sets the logical drive ID of the tagged path
        /// </summary>
        public string LogicalDriveId
        {
            get { return _logicalid; }
            set { _logicalid = value; }
        }

        /// <summary>
        /// Gets or sets the full path name of the tagged path
        /// </summary>
        public string PathName
        {
            get { return _pathName; }
            set { _pathName = value; }
        }

        /// <summary>
        /// Gets or sets the last updated date of the tagged path
        /// </summary>
        public long LastUpdatedDate
        {
            get { return _lastUpdatedDate; }
            set { _lastUpdatedDate = value; }
        }

        /// <summary>
        /// Gets or sets the created date of the tagged path
        /// </summary>
        public long CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        /// <summary>
        /// Gets or sets the boolean value that represents whether the tagged path is deleted
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }

        /// <summary>
        /// Gets or sets the deleted date of the tagged path
        /// </summary>
        public long DeletedDate
        {
            get { return _deletedDate; }
            set { _deletedDate = value; }
        }

        /// <summary>
        /// Creates a new TaggedPath object
        /// </summary>
        /// <param name="path">The string that represents the path to be tagged</param>
        /// <param name="created">The long value that represents the created date of the tagged path</param>
        /// <remarks>The logical drive ID is retrieved from the path that is passed as parameter using 
        /// <see cref="TaggingHelper.GetLogicalID">TaggingHelper.GetLogicalID</see> method.
        /// The last updated date is set to the created date that is passed as parameter.
        /// The boolean value that represents whether the tagged path is deleted is set to false by default.
        /// The deleted date is set to 0 by default.</remarks>
        public TaggedPath(string path, long created)
        {
            this._logicalid = TaggingHelper.GetLogicalID(path);
            this._pathName = path;
            this._lastUpdatedDate = created;
            this._createdDate = created;
            this._isDeleted = false;
            this._deletedDate = 0;
        }

        /// <summary>
        /// Sets the boolean value that represents whether the tagged path is deleted to true
        /// </summary>
        /// <param name="deletedDate">The long value that represents the deleted date of the tagged path</param>
        /// <remarks>The last updated date is set to the deleted date that is passed as parameter.</remarks>
        public void Remove(long deletedDate)
        {
            _isDeleted = true;
            _lastUpdatedDate = deletedDate;
            _deletedDate = deletedDate;
        }

        /// <summary>
        /// Combines the full path name of the tagged path with the trailing path that is passed as parameter
        /// </summary>
        /// <param name="trailingPath">The string value that represents the trailing path to be combined
        /// with the tagged path</param>
        /// <returns>The string value that represents the combined path name from appending the trailing
        /// path to the full path name of the tagged path</returns>
        public string Append(string trailingPath)
        {
            return System.IO.Path.Combine(_pathName, trailingPath);
        }

        /// <summary>
        /// Sets the full path name of the tagged path to the new path that is passed as parameter
        /// </summary>
        /// <param name="newpath">The string value that represents the new path that is to replace the
        /// full path name of the tagged path</param>
        public void Rename(string newpath)
        {
            _pathName = newpath;
        }

        /// <summary>
        /// Sets part of the tagged path represented by the old path that is passed as parameter
        /// to the new path that is passed as parameter
        /// </summary>
        /// <param name="oldPath">The string value that represents the old path which is part of the 
        /// full path name of the tagged path</param>
        /// <param name="newPath">The string value that represents the new path which is to replace
        /// part of the full path name of the tagged path represented by the old path that is passed as
        /// parameter</param>
        public void Replace(string oldPath, string newPath)
        {
            _pathName = _pathName.Replace(oldPath, newPath);
        }
    }
}
