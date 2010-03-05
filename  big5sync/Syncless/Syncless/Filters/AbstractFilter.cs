using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace Syncless.Filters
{
    public abstract class AbstractFilter
    {
        private Mode _mode;

        public Mode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public AbstractFilter(Mode mode)
        {
            _mode = mode;
        }
        public AbstractFilter()
            : this(Mode.EXCLUDE)
        {
        }
        public virtual string Filter(string path)
        {
            Debug.Assert(path != null, "Path Cannot Be Null");
            if (Match(path))
            {
                if (_mode == Mode.EXCLUDE)
                {
                    return null;
                }
                else
                {
                    return path;
                }
            }
            else
            {
                if (_mode == Mode.INCLUDE)
                {
                    return null;
                }
                else
                {
                    return path;
                }
            }
        }
        public virtual List<string> Filter(List<string> paths)
        {
            Debug.Assert(paths != null, "Paths List cannot be Null");
            List<string> returnPaths = new List<string>();
            if (_mode == Mode.INCLUDE)
            {
                foreach (string path in paths)
                {
                    if (Match(path))
                    {
                        returnPaths.Add(path);
                    }
                }
            }
            else
            {
                foreach (string path in paths)
                {
                    if (!Match(path))
                    {
                        returnPaths.Add(path);
                    }
                }
            }
            return returnPaths;
        }
        public abstract bool Match(string path);

    }
    public enum Mode {
        INCLUDE,EXCLUDE
    }
}
