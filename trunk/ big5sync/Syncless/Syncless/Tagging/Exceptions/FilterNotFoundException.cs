using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;
using Syncless.Helper;
namespace Syncless.Tagging.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt retrieve a filter that does not exist.
    /// </summary>
    public class FilterNotFoundException : Exception
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
        /// Initializes a new instance of the FilterNotFoundException class with its message string 
        /// set to a system-supplied message
        /// </summary>
        public FilterNotFoundException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the FilterNotFoundException class with its message string 
        /// set to message that is given by <see cref="ErrorMessage.FILTER_NOT_FOUND_EXCEPTION">
        /// ErrorMessage.FILTER_NOT_FOUND_EXCEPTION</see> string value and the filter property to the 
        /// filter that is passed as parameter
        /// </summary>
        /// <param name="filter">The <see cref="Filter">Filter</see> object that represents the filter
        /// that is not found</param>
        public FilterNotFoundException(Filter filter)
            : base(ErrorMessage.FILTER_NOT_FOUND_EXCEPTION)
        {
            _filter = filter;
        }
    }
}
