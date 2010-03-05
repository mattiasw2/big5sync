using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Syncless.Filters
{
    public abstract class AbstractFileFilter:AbstractFilter
    {
        public AbstractFileFilter(Mode mode)
            : base(mode)
        {
        }

        public sealed override string Filter(string path)
        {
            if (!Directory.Exists(path))
            {
                return path;
            }
            else
            {
                if (!File.Exists(path))
                {
                    return null;
                }
                return base.Filter(path);
            }
        }
    }
}
