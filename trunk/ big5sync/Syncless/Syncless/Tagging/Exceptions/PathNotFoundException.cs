using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.Tagging.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt retrieve a tagged path that does not exist.
    /// </summary>
    public class PathNotFoundException : Exception
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
        /// Initializes a new instance of the PathNotFoundException class with its message string 
        /// set to a system-supplied message
        /// </summary>
        public PathNotFoundException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PathNotFoundException class with its message string 
        /// set to message that is given by <see cref="ErrorMessage.PATH_NOT_FOUND_EXCEPTION">
        /// ErrorMessage.PATH_NOT_FOUND_EXCEPTION</see> string value and the path property to the 
        /// path name that is passed as parameter
        /// </summary>
        /// <param name="pathname">The string value that represents the path name</param>
        public PathNotFoundException(string pathname)
            : base(ErrorMessage.PATH_NOT_FOUND_EXCEPTION)
        {
            this._pathname = pathname;
        }
    }
}
