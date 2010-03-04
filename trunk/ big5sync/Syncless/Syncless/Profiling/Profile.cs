using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Profiling.Exceptions;
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

        private List<ProfileMapping> _mappingList;
        public List<ProfileMapping> Mappings
        {
            get { return _mappingList;}
        }
        
        public Profile(string name)
        {
            this._profilename = name;
            _mappingList = new List<ProfileMapping>();
            
        }
        private long _lastUpdatedTime;
        public long LastUpdatedTime
        {
            get { return _lastUpdatedTime; }
            set { _lastUpdatedTime = value; }
        }
            
        /// <summary>
        /// Check if a Profilemapping is Contain in this profile.
        ///   throw ProfileMappingConflictException if Profile Mapping has Conflict.
        ///   conflict is identified as there is a similar only 1 or 2 field.
        /// </summary>
        /// <param name="mapping">The Mapping to Check</param>
        /// <exception cref="ProfileMappingConflictException">Thrown when there is conflict in the Profile Mapping</exception>
        /// <returns>true if there is a mapping that is equals to the mapping. Else return false</returns>
        public bool Contains(ProfileMapping mapping)
        {
            foreach (ProfileMapping map in _mappingList)
            {
                if (map.Equals(mapping))
                {
                    return true;
                }
            }
            return false;
        }
        public void CreateMapping(string logicalAddress, string physicalAddress,string guid)
        {
            ProfileMapping map = new ProfileMapping(logicalAddress, physicalAddress, guid);
            if (Contains(map))
            {
                throw new ProfileMappingExistException(map);
            }            
            _mappingList.Add(map);
            
        }
        public void CreateMapping(ProfileMapping mapping)
        {
            this.CreateMapping(mapping.LogicalAddress, mapping.PhyiscalAddress, mapping.GUID);
        }

        /// <summary>
        /// Find a Physical Address from a Logical Address
        /// </summary>
        /// <param name="logical">logical address</param>
        /// <returns>physical address . return null if the logical address is not found or the physical drive is not in.</returns>
        public string FindPhysicalFromLogical(string logical)
        {
            foreach (ProfileMapping mapping in _mappingList)
            {
                if (mapping.LogicalAddress.Equals(logical))
                {
                    if (mapping.PhyiscalAddress.Trim().Equals(""))
                    {
                        return null;
                    }
                    
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
        public void UpdateDrive(string guid, string driveid)
        {
            ProfileMapping mapping = FindMappingFromGUID(guid);
            if (mapping == null)
            {
                // why lol ?
                return;
            }
            if (mapping.PhyiscalAddress.Equals(""))
            {
                mapping.PhyiscalAddress = driveid;
            }
            else if (!mapping.PhyiscalAddress.Equals(driveid))
            {
                throw new ProfileGuidConflictException();
            }

        }
        public bool RemoveDrive(string driveid)
        {
            ProfileMapping mapping = FindMappingFromPhysical(driveid);
            if (mapping == null)
            {
                return false;
            }
            else
            {
                if (mapping.PhyiscalAddress.Equals("driveid"))
                {
                    mapping.PhyiscalAddress = "";
                }
                
            }
            return true;
        }
        private ProfileMapping FindMappingFromGUID(string guid)
        {
            foreach (ProfileMapping mapping in _mappingList)
            {
                if (mapping.GUID.Equals(guid))
                {
                    return mapping;
                }
            }
            return null;
        }
        private ProfileMapping FindMappingFromPhysical(string physical)
        {
            foreach (ProfileMapping mapping in _mappingList)
            {
                if (mapping.PhyiscalAddress.Equals(physical))
                {
                    return mapping;
                }
            }
            return null;
        }
        public bool CanMerge(Profile profile)
        {
            try
            {
                foreach (ProfileMapping mapping in profile.Mappings)
                {
                    Contains(mapping);
                }
                foreach (ProfileMapping mapping in Mappings)
                {
                    profile.Contains(mapping);
                }
            }
            catch (ProfileMappingConflictException pmce)
            {
                throw new ProfileConflictException(pmce);
            }

            return true;
        }
        public Profile Merge(Profile profile)
        {
            if (CanMerge(profile))
            {
                foreach (ProfileMapping mapping in profile.Mappings)
                {
                    if(!Contains(mapping))
                    {
                        CreateMapping(mapping);
                    }
                }
            }
            return profile;
        }
    }
}
