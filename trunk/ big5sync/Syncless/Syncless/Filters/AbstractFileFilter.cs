using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Filters
{
    public abstract class AbstractFileFilter:AbstractFilter
    {
        public AbstractFileFilter(Mode mode)
            : base(mode)
        {
        }
    }
}
