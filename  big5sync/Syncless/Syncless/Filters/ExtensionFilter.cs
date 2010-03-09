using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
namespace Syncless.Filters
{
    public class ExtensionFilter:FileFilter
    {
        private string _pattern;
        private Regex _regex;
                
        public ExtensionFilter(string pattern,FilterMode mode):base(mode)
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
