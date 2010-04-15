using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.Tagging.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to create a tag that already exists.
    /// </summary>
    public class TagAlreadyExistsException : Exception
    {
        private string _tagname;

        /// <summary>
        /// Gets the name of the tag
        /// </summary>
        public string Tagname
        {
            get { return _tagname; }
        }

        /// <summary>
        /// Initializes a new instance of the TagAlreadyExistsException class with its message string 
        /// set to a system-supplied message
        /// </summary>
        public TagAlreadyExistsException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TagAlreadyExistsException class with its message string 
        /// set to message that is given by <see cref="ErrorMessage.TAG_ALREADY_EXISTS_EXCEPTION">
        /// ErrorMessage.TAG_ALREADY_EXISTS_EXCEPTION</see> string value and the tag name property to 
        /// the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name</param>
        public TagAlreadyExistsException(string tagname)
            : base(ErrorMessage.TAG_ALREADY_EXISTS_EXCEPTION)
        {
            this._tagname = tagname;
        }
    }
}
