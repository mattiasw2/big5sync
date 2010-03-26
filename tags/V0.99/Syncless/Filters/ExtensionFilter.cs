using System.Text.RegularExpressions;
namespace Syncless.Filters
{
    public class ExtensionFilter:FileFilter
    {
        private string _pattern;

        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; BuildRegex(_pattern); }
        }
        private Regex _regex;
                
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
        public override bool Match(string path)
        {
            return _regex.Match(path.ToLower()).Success;    
        }
        
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
                if (!filter.Pattern.Equals(_pattern.ToLower()))
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
