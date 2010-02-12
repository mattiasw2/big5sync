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
            ProfileMapping map = new ProfileMapping(logicalAddress, physicalAddress);
        }
        

    }
}
