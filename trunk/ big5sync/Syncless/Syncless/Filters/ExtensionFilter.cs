using System.Text.RegularExpressions;
namespace Syncless.Filters
{
    public class ExtensionFilter:FileFilter
    {
        private string _pattern;
        private Regex _regex;
                
        internal ExtensionFilter(string pattern,FilterMode mode):base(mode)
        {
            if(pattern == null){
                pattern = "";
            }
            _pattern = pattern;
            BuildRegex(_pattern);
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
            return _regex.Match(path).Success;    
        }
    }
}
