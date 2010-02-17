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
        public Boolean Equals(ProfileMapping mapping)
        {
            int equalCount = 0;
            if (this._guid.Equals(mapping.GUID))
            {
                equalCount++;
            }
            if (this._logicalAddress.Equals(mapping.LogicalAddress))
            {
                equalCount++;
            }
            if (equalCount == 0)
            {
                return false;
            }
            if (equalCount == 2)
            {
                return true;
            }
            throw new ProfileMappingConflictException(this, mapping);
            
        }
    }
}
