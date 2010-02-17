using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public class FileTag:Tag
    {
        public FileTag(string tagname, long lastupdated)
            : base(tagname, lastupdated)
        {
        }
    }
}
