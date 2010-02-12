using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class DummyTag
    {
        private string tagName;
        private List<string> paths;

        public DummyTag(string tagName, List<string> paths)
        {
            this.tagName = tagName;
            this.paths = paths;
        }

        public string TagName
        {
            get
            {
                return tagName;
            }
        }

        public List<string> Paths
        {
            get
            {
                return paths;
            }
        }
    }
}
