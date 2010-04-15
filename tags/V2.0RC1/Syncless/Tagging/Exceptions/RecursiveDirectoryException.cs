using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.Tagging.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to tag two folders which are parent/sub-folder of each other.
    /// </summary>
    public class RecursiveDirectoryException : Exception
    {
        private string _path;

        /// <summary>
        /// Gets and sets the path
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private string _tagname;

        /// <summary>
        /// Gets and sets the tag name
        /// </summary>
        public string Tagname
        {
            get { return _tagname; }
            set { _tagname = value; }
        }

        /// <summary>
        /// Initializes a new instance of the RecursiveDirectoryException class with its message string 
        /// set to a system-supplied message
        /// </summary>
        public RecursiveDirectoryException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TagAlreadyExistsException class with its message string 
        /// set to message that is given by <see cref="ErrorMessage.RECURSIVE_DIRECTORY_EXCEPTION">
        /// ErrorMessage.RECURSIVE_DIRECTORY_EXCEPTION</see> string value, the path property to the 
        /// path name that is passed as parameter and the tag name property to the tag name that is 
        /// passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path name</param>
        /// <param name="tagname">The string value that represents the tag name</param>
        public RecursiveDirectoryException(string path, string tagname)
            : base(ErrorMessage.RECURSIVE_DIRECTORY_EXCEPTION)
        {
            this._path = path;
            this._tagname = tagname;
        }
    }
}
