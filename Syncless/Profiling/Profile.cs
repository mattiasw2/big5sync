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
        /// <summary>
        /// Profile name
        /// </summary>
        private string _profilename;
        /// <summary>
        /// Profile name
        /// </summary>
        public string ProfileName
        {
            get { return _profilename; }
            set { _profilename = value; }
        }

        /// <summary>
        /// List of Profile Mapping
        /// </summary>
        private List<ProfileMapping> _mappingList;
        /// <summary>
        /// List of Profile Mapping
        /// </summary>
        public List<ProfileMapping> Mappings
        {
            get { return _mappingList;}
        }
        /// <summary>
        /// Last Updated Time
        /// </summary>
        private long _lastUpdatedTime;
        /// <summary>
        /// Last Updated Time
        /// </summary>
        public long LastUpdatedTime
        {
            get { return _lastUpdatedTime; }
            set { _lastUpdatedTime = value; }
        }
                

        public Profile(string name)
        {
            this._profilename = name;
            _mappingList = new List<ProfileMapping>();
            
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
        /// <summary>
        /// Create a Profile Mapping
        /// </summary>
        /// <param name="logicalAddress">the logical address</param>
        /// <param name="physicalAddress">the physical address</param>
        /// <param name="guid">the guid</param>
        public void CreateMapping(string logicalAddress, string physicalAddress,string guid)
        {
            ProfileMapping map = new ProfileMapping(logicalAddress, physicalAddress, guid);
            if (Contains(map))
            {
                throw new ProfileMappingExistException(map);
            }            
            _mappingList.Add(map);
            
        }
        /// <summary>
        /// Create a Profille Mapping
        /// </summary>
        /// <param name="mapping">the profilemapping to create.</param>
        public void CreateMapping(ProfileMapping mapping)
        {
            this.CreateMapping(mapping.LogicalAddress, mapping.PhyiscalAddress, mapping.GUID);
        }
        /// <summary>
        /// Find a Physical Mapping From a Logical Mapping
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
        /// <summary>
        /// Find a Logical Mapping From a Physical Mapping
        /// </summary>
        /// <param name="physical"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Update a Particular drive with to its GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="driveid"></param>
        public void UpdateDrive(string guid, string driveid)
        {
            ProfileMapping mapping = FindMappingFromGUID(guid);
            if (mapping == null)
            {
                CreateMapping(guid, driveid, guid);
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
        /// <summary>
        /// Remove a Drive from Profiling to mark it unavailable
        /// </summary>
        /// <param name="driveid">The id of the drive to remove</param>
        /// <returns>true if something was removed</returns>
        public bool RemoveDrive(string driveid)
        {
            ProfileMapping mapping = FindMappingFromPhysical(driveid);
            if (mapping == null)
            {
                return false;
            }
            else
            {
                if (mapping.PhyiscalAddress.Equals(driveid))
                {
                    mapping.PhyiscalAddress = "";
                }
                
            }
            return true;
        }

        


        #region private method
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
        #endregion
    }
}
