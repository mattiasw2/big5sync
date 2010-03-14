using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;
namespace Syncless.Tagging.Exceptions
{
    public class FilterAlreadyExistException : Exception
    {
        private Filter _filter;

        public Filter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
        public FilterAlreadyExistException(Filter filter)
        {
            _filter = filter;
        }
    }
}
