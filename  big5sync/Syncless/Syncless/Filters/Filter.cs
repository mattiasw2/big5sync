/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System;
using System.Collections.Generic;

namespace Syncless.Filters
{
    /// <summary>
    /// The abstract super class for all Filter.
    /// </summary>
    public abstract class Filter
    {
        /// <summary>
        /// The <see cref="FilterMode"/>, Include or Exclude
        /// </summary>
        public FilterMode Mode { get; set; }
        /// <summary>
        /// Initialize a Filter object
        /// </summary>
        /// <param name="mode">The <see cref="FilterMode"/> for Filter</param>
        protected Filter(FilterMode mode){
            Mode = mode ;
        }

        /// <summary>
        /// If mode is Include, it will return a list that match the criteria.
        /// If mode is Exclude, it will return a list that does not match the criteria.
        /// </summary>
        /// <param name="patterns">list of patterns to check</param>
        /// <returns>filtered list.</returns>
        public virtual List<string> ApplyFilter(List<string> patterns) {
            switch (Mode)
            {
                case FilterMode.EXCLUDE: return Exclude(patterns);
                case FilterMode.INCLUDE: return Include(patterns);
            }
            throw new Exception("Unknown Filter Type");                
        }
        /// <summary>
        /// Exclude a list of pathes that Matches.   
        /// </summary>
        /// <param name="patterns">The list of path to match</param>
        /// <returns>The list of path that is not excluded</returns>
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
        /// <summary>
        /// Include a list of pathes that Matches
        /// </summary>
        /// <param name="patterns">the list of path to match</param>
        /// <returns>The list of path that is included</returns>
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
        /// <summary>
        /// Override the Equals of Object.
        ///   2 Filter is consider similiar if the Mode is the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Check if two Filter is equals.
        /// </summary>
        /// <param name="other">the Filter to compare to</param>
        /// <returns>true if it matches, false otherwise</returns>
        public virtual bool Equals(Filter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Mode, Mode);
        }
        /// <summary>
        /// Override the Get HashCode Method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Mode.GetHashCode();
        }
    }
    /// <summary>
    /// Define the Filter Mode
    /// </summary>
    public enum FilterMode
    {
        /// <summary>
        /// Include only files that matches
        /// </summary>
        INCLUDE,
        /// <summary>
        /// Exlcude files that matches.
        /// </summary>
        EXCLUDE

    }
}
