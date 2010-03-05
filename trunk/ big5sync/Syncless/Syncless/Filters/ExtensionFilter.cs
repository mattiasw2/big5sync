using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
namespace Syncless.Filters
{
    public class ExtensionFilter : AbstractFileFilter
    {
        private string _pattern;
        private Regex _regex;

        
        public ExtensionFilter(string pattern,Mode mode):base(mode)
        {
            if(pattern == null){
                pattern = "";
            }
            _pattern = pattern;
            BuildRegex(_pattern);
        }
        private void BuildRegex(string pattern)
        {
            
            pattern = pattern.Replace("." , "\\.");
            pattern = pattern.Replace("*", ".*");
            pattern = pattern.Replace("?", ".{1}");   
            _regex = new Regex(pattern);
        }
        public override bool Match(string path)
        {
            Match m = _regex.Match(path);
            
            return m.Success;
        }
    }
}
