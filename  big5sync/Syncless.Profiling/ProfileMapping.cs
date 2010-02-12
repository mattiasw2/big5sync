using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Profiling
{
    public class ProfileMapping
    {
        private string _logicalAddress;
        public string LogicalAddress
        {
            get { return _logicalAddress; }
            set { _logicalAddress = value; }
        }
        private string _phyiscalAddress;
        public string PhyiscalAddress
        {
            get { return _phyiscalAddress; }
            set { _phyiscalAddress = value; }
        }
        public ProfileMapping(string logical, string physical)
        {
            this._logicalAddress = logical;
            this._phyiscalAddress = physical;
        }
    }
}
