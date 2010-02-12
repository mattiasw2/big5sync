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
        private string _guid;
        public string GUID
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public ProfileMapping(string logical, string physical, string guid)
        {
            this._logicalAddress = logical;
            this._phyiscalAddress = physical;
            this._guid = guid;
        }
        
    }
}
