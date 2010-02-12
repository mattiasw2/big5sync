using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Profiling
{
    public class Profile
    {
        
        private string _profilename;
        public string ProfileName
        {
            get { return _profilename; }
            set { _profilename = value; }
        }

        public List<ProfileMapping> _mappingList;
        public Profile(string name)
        {
            this._profilename = name;
            _mappingList = new List<ProfileMapping>();
        }

        public void CreateMapping(string logicalAddress, string physicalAddress)
        {
            if (FindLogicalFromPhysical(physicalAddress) != null || FindPhysicalFromLogical(logicalAddress))
            {
                //Mapping Exist Exception
                //TODO
                throw new Exception();

            }
            ProfileMapping map = new ProfileMapping(logicalAddress, physicalAddress);
            _mappingList.Add(map);
            
        }
        public string FindPhysicalFromLogical(string logical)
        {
            foreach (ProfileMapping mapping in _mappingList)
            {
                if (mapping.LogicalAddress.Equals(logical))
                {
                    return mapping.PhyiscalAddress;
                }
            }
            return null;
        }
        public string FindLogicalFromPhysical(string physical)
        {
            foreach (ProfileMapping mapping in _mappingList)
            {
                if (mapping.PhyiscalAddress.Equals(physical))
                {
                    return mapping.LogicalAddress;
                }
            }
            return null;
        }

    }
}
