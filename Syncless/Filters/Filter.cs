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
        public abstract bool Match(string pattern);

        
    }
    public enum FilterMode
    {
        INCLUDE, EXCLUDE
    }
}
