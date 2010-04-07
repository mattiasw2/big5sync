using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;
using Syncless.Helper;
namespace Syncless.Tagging.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to create a filter that already exists.
    /// </summary>
    public class FilterAlreadyExistException : Exception
    {
        private Filter _filter;

        /// <summary>
        /// Gets or sets the filter
        /// </summary>
        public Filter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Initializes a new instance of the FilterAlreadyExistException class with its message string 
        /// set to a system-supplied message
        /// </summary>
        public FilterAlreadyExistException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the FilterAlreadyExistException class with its message string 
        /// set to message that is given by <see cref="ErrorMessage.FILTER_ALREADY_EXISTS_EXCEPTION">
        /// ErrorMessage.FILTER_ALREADY_EXISTS_EXCEPTION</see> string value and the filter property to the 
        /// filter that is passed as parameter
        /// </summary>
        /// <param name="filter"></param>
        public FilterAlreadyExistException(Filter filter)
            : base(ErrorMessage.FILTER_ALREADY_EXISTS_EXCEPTION)
        {
            _filter = filter;
        }
    }
}
