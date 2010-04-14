/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System.Text.RegularExpressions;
namespace Syncless.Filters
{
    /// <summary>
    /// Extension Filter for file
    /// </summary>
    public class ExtensionFilter:FileFilter
    {
        private string _pattern;

        /// <summary>
        /// Get and Set the Pattern.
        /// </summary>
        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; BuildRegex(_pattern); }
        }
        private Regex _regex;

        /// <summary>
        /// Initialize the Extension Filter
        /// </summary>
        /// <param name="pattern">pattern for the extension filter</param>
        /// <param name="mode">The <see cref="FilterMode"/> for Filter</param>
        internal ExtensionFilter(string pattern,FilterMode mode):base(mode)
        {
            if(pattern == null){
                pattern = "";
            }
            this._pattern = pattern;
            BuildRegex(_pattern.ToLower());
        }
        private void BuildRegex(string pattern)
        {   
            pattern = Regex.Escape(pattern);
            pattern = "^" + pattern;
            pattern = pattern.Replace(@"\*", ".*");
            pattern = pattern.Replace(@"\?", ".{1}");
            pattern = pattern + @"$";
            _regex = new Regex(pattern);
        }
        /// <summary>
        /// Override the Match Method.
        /// </summary>
        /// <param name="path">path to match</param>
        /// <returns>true if it matches, otherwise false</returns>
        public override bool Match(string path)
        {
            return _regex.Match(path.ToLower()).Success;    
        }
        /// <summary>
        /// Override the Equal method.
        /// </summary>
        /// <param name="other">The Filter to compare to.</param>
        /// <returns>true if the other Filter is a Extension Filter and have the same pattern and <see cref="FilterMode"/> </returns>
        public override bool Equals(Filter other)
        {
            ExtensionFilter filter = other as ExtensionFilter;
            if (filter == null)
            {
                return false;
            }
            bool parentEqual = base.Equals(filter);
            if (parentEqual)
            {
                if (!filter.Pattern.ToLower().Equals(_pattern.ToLower()))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        
    }
}
