using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Core
{
    public class FileTagView:TagView
    {
        public FileTagView(string tagname, long lastupdated)
            : base(tagname, lastupdated)
        {
        }
    }
}
