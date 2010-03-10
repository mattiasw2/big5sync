using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    internal class TagConfig
    {
        private bool _isSeamless;

        public bool IsSeamless
        {
            get { return _isSeamless; }
            set { _isSeamless = value; }
        }
    }
}
