using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Filters
{
    public abstract class Filter
    {
        private FilterMode _mode;

        public FilterMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public Filter(FilterMode mode){
            this._mode = mode ;
        }

        /// <summary>
        /// If mode is Include, it will return a list that match the criteria.
        /// If mode is Exclude, it will return a list that does not match the criteria.
        /// </summary>
        /// <param name="patterns">list of patterns to check</param>
        /// <returns>filtered list.</returns>
        public virtual List<string> ApplyFilter(List<string> patterns) {
            switch (_mode)
            {
                case FilterMode.EXCLUDE: return Exclude(patterns);
                case FilterMode.INCLUDE: return Include(patterns);
            }
            throw new Exception("Unknown Filter Type");                
        }
        protected virtual List<string> Exclude(List<string> patterns)
        {
            List<string> outputList = new List<string>();
            foreach (string pattern in patterns)
            {
                if (!Match(pattern))
                {
                    outputList.Add(pattern);
                }
            }

            return outputList;
        }
        protected virtual List<string> Include(List<string> patterns)
        {
            List<string> outputList = new List<string>();
            foreach (string pattern in patterns)
            {
                if (Match(pattern))
                {
                    outputList.Add(pattern);
                }
            }
            return outputList;
        }

        /// <summary>
        /// Match the a given file path with current filters to check whether to include or exclude the path
        /// </summary>
        /// <param name="pattern">The file path to be filtered</param>
        /// <returns>True if the file path matches the filter, else false</returns>
        public abstract bool Match(string pattern);

        public override bool Equals(object obj)
        {
            
            Filter filter = obj as Filter;
            if (filter == null)
            {
                return false;
            }
            if (filter.GetType() != GetType())
            {
                return false;
            }
            return Equals(filter);
        }

        public virtual bool Equals(Filter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._mode, _mode);
        }

        public override int GetHashCode()
        {
            return _mode.GetHashCode();
        }
    }
    public enum FilterMode
    {
        INCLUDE, EXCLUDE
    }
}
