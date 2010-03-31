using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Syncless.Filters
{
    public class FolderFilter : Filter
    {
        private string _pattern;
        public string Pattern
        {
            get;
            set;
        }
        public FolderFilter(string pattern, FilterMode mode)
            : base(mode)
        {
        }
        public override bool Match(string pattern)
        {
            return false;
        }

        private void BuildRegex(string pattern)
        {
            pattern = Regex.Escape(pattern);
            pattern = @"\S" + pattern;
        }
    }
}
