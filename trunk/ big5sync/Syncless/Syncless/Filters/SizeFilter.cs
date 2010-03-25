using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Filters
{
    public class SizeFilter : FileFilter
    {
        public SizeFilter(FilterMode mode) : base(mode) {}
        public override bool Match(string pattern)
        {
            throw new NotImplementedException();
        }
    }
}
