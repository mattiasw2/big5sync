using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.Tagging.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to tag a path that already exists.
    /// </summary>
    public class PathAlreadyExistsException : Exception
    {
        private string _pathname;

        /// <summary>
        /// Gets the full name of the path
        /// </summary>
        public string Pathname
        {
            get { return _pathname; }
        }

        /// <summary>
        /// Initializes a new instance of the PathAlreadyExistsException class with its message string 
        /// set to a system-supplied message
        /// </summary>
        public PathAlreadyExistsException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PathAlreadyExistsException class with its message string 
        /// set to message that is given by <see cref="ErrorMessage.PATH_ALREADY_EXISTS_EXCEPTION">
        /// ErrorMessage.PATH_ALREADY_EXISTS_EXCEPTION</see> string value and the path property to 
        /// the path name that is passed as parameter
        /// </summary>
        /// <param name="pathname">The string value that represents the path name</param>
        public PathAlreadyExistsException(string pathname)
            : base(ErrorMessage.PATH_ALREADY_EXISTS_EXCEPTION)
        {
            this._pathname = pathname;
        }
    }
}
