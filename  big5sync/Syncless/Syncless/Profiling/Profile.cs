using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Profiling.Exceptions;
namespace Syncless.Profiling
{
    public class Profile
    {
        #region attributes
        private const string DEFAULT_DRIVE_NAME = "-";

        /// <summary>
        /// Profile name
        /// </summary>
        private string _profilename;
        public string ProfileName
        {
            get { return _profilename; }
            set { _profilename = value; }
        }

        /// <summary>
        /// Last Updated Time
        /// </summary>
        private long _lastUpdatedTime;
        public long LastUpdatedTime
        {
            get { return _lastUpdatedTime; }
            set { _lastUpdatedTime = value; }
        }

        /// <summary>
        /// The list of ProfileDrive
        /// </summary>
        private List<ProfileDrive> _fullList;
        public List<ProfileDrive> ProfileDriveList
        {
            get
            {
                lock (_fullList)
                {
                    List<ProfileDrive> driveList = new List<ProfileDrive>();
                    driveList.AddRange(_fullList);
                    return driveList;
                }
            }

        }

        /// <summary>
        /// Connected list of ProfileDrive using logical id as key
        /// </summary>
        private Dictionary<string, ProfileDrive> _logicalDict;

        /// <summary>
        /// Connected list of ProfileDrive using physical id as key
        /// </summary>
        private Dictionary<string, ProfileDrive> _phyiscalDict;
        #endregion

        /// <summary>
        /// Constructor for Profile
        /// </summary>
        /// <param name="profileName">The name of the profile for the Profile constructed</param>
        public Profile(string profileName)
        {
            _profilename = profileName;
            _fullList = new List<ProfileDrive>();
            _logicalDict = new Dictionary<string, ProfileDrive>();
            _phyiscalDict = new Dictionary<string, ProfileDrive>();
        }

        #region public methods
        /// <summary>
        /// Find a Logical Mapping From a Physical Mapping
        /// </summary>
        /// <param name="physical">The physical address to be used to find its corresponding logical address</param>
        /// <returns>The corresponding logical address if it is available or found, else null</returns>
        public string FindLogicalFromPhysical(string physical)
        {
            ProfileDrive drive = null;
            return _phyiscalDict.TryGetValue(physical, out drive) ? drive.LogicalId : null;
        }

        /// <summary>
        /// Find a Physical Mapping From a Logical Mapping
        /// </summary>
        /// <param name="logical">The logical address to be used to find its corresponding physical address</param>
        /// <returns>The corresponding physical address if it is available or found, else null</returns>
        public string FindPhysicalFromLogical(string logical)
        {
            ProfileDrive drive = null;
            return _logicalDict.TryGetValue(logical, out drive) ? drive.PhysicalId : null;
        }

        /// <summary>
        /// Find the logical id that corresponds to the GUID given
        /// </summary>
        /// <param name="guid">The GUID for which its logical id is to be retrieved</param>
        /// <returns>The logical id that corresponds to the GUID given if the drive is available or exists,
        /// else null</returns>
        public string FindLogicalIdFromGUID(string guid)
        {
            ProfileDrive drive = FindProfileDriveFromLogicalId(guid);
            return drive != null ? drive.LogicalId : null;
        }

        /// <summary>
        /// Find the drive name that corresponds to the logical id given
        /// </summary>
        /// <param name="logical">The logical id for which its GUID is to be retrieved</param>
        /// <returns>The drive name that corresponds to the logical id given if the drive is available
        /// or exists, else null</returns>
        public string FindDriveNameFromLogical(string logical)
        {
            ProfileDrive drive = FindProfileDriveFromLogicalId(logical);
            return drive != null ? drive.DriveName : null;
        }

        /// <summary>
        /// Not yet implemented : Set the drive name of a given drive to the name given
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        public void SetDriveName(DriveInfo info, string name)
        {
            throw new NotImplementedException();
            //ProfileDrive drive = FindProfileDriveFromPhysicalId(info.Name);
            //drive.LastUpdated = DateTime.Now.Ticks;
            //_lastUpdatedTime = DateTime.Now.Ticks;
            //drive.DriveName = name;
        }

        /// <summary>
        /// If the given drive is not already in the list of existing profile drives, 
        /// add the profile drive to the list of profile drives
        /// </summary>
        /// <param name="info">The DriveInfo of the drive to be added</param>
        /// <param name="guid">The corresponding GUID for the drive to be added</param>
        public void InsertDrive(DriveInfo info, string guid)
        {
            ProfileDrive drive = null;
            string driveLetter = ProfilingHelper.ExtractDriveName(info.Name);
            if (!_phyiscalDict.TryGetValue(driveLetter, out drive))
            {
                drive = FindProfileDriveFromGUID(guid);
                if (drive == null)
                {
                    drive = CreateProfileDrive(guid);
                }
                drive.Info = info;
                _phyiscalDict[drive.PhysicalId] = drive;
                _logicalDict[drive.LogicalId] = drive;
            }
            else
            {
                if (drive.Guid.Equals(guid))
                {
                    throw new ProfileDriveConflictException();
                }
            }
        }

        /// <summary>
        /// If the given drive is in the list of existing profile drives,
        /// remove the profile drive from the list
        /// </summary>
        /// <param name="info">The DriveInfo of the drive to be removed</param>
        /// <returns>True if the drive is successfully removed from the list of existing profile drives,
        /// else false</returns>
        public bool RemoveDrive(DriveInfo info)
        {
            string driveLetter = ProfilingHelper.ExtractDriveName(info.Name);
            ProfileDrive drive = null;
            if (_phyiscalDict.TryGetValue(driveLetter, out drive))
            {
                Debug.Assert(drive != null);
                if (_phyiscalDict.ContainsKey(drive.PhysicalId))
                {
                    _phyiscalDict.Remove(drive.PhysicalId);
                }
                if (_logicalDict.ContainsKey(drive.LogicalId))
                {
                    _logicalDict.Remove(drive.LogicalId);
                }
                drive.Info = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Create a ProfileDrive using the GUID given and add to the list of existing profile drives
        /// </summary>
        /// <param name="guid">The GUID for which the profile drive is to be created</param>
        /// <returns>The ProfileDrive that is created</returns>
        public ProfileDrive CreateProfileDrive(string guid)
        {
            ProfileDrive drive = new ProfileDrive(guid, DEFAULT_DRIVE_NAME);
            lock (_fullList)
            {
                _fullList.Add(drive);
            }
            return drive;
        }
        #endregion

        #region internal methods
        /// <summary>
        /// Add the given profile drive the the list of existing profile drives
        /// </summary>
        /// <param name="drive">The new profile drive to be added</param>
        /// <returns>True if the drive is added</returns>
        internal bool AddProfileDrive(ProfileDrive drive)
        {
            lock (_fullList)
            {
                this._fullList.Add(drive);
            }
            return true;
        }
        
        /// <summary>
        /// Find the profile drive using the given GUID
        /// </summary>
        /// <param name="guid">The GUID for which the profile drive is to be retrieved</param>
        /// <returns>The profile drive if it is found in the list of existing profile drives, else null</returns>
        internal ProfileDrive FindProfileDriveFromGUID(string guid)
        {
            lock (_fullList)
            {
                foreach (ProfileDrive drive in _fullList)
                {
                    if (drive.Guid.Equals(guid))
                    {
                        return drive;
                    }
                }
                return null;
            }
        }
        
        /// <summary>
        /// Find the profile drive using the given logical id
        /// </summary>
        /// <param name="logicalid">The logical id for which the profile drive is to be retrieved</param>
        /// <returns>The profile drive if it is found in the list of existing profile drives,
        /// else null</returns>
        internal ProfileDrive FindProfileDriveFromLogicalId(string logicalid)
        {
            lock (_fullList)
            {
                foreach (ProfileDrive drive in _fullList)
                {
                    if (drive.Guid.Equals(logicalid))
                    {
                        return drive;
                    }
                }
                return null;
            }
        }
        
        /// <summary>
        /// Find the profile drive using the given physical id
        /// </summary>
        /// <param name="physicalid">The physical id for which the profile drive is to be retrieved</param>
        /// <returns>The profile drive if it is found in the list of existing profile drives,
        /// else null</returns>
        internal ProfileDrive FindProfileDriveFromPhysicalId(string physicalid)
        {
            if (_phyiscalDict.ContainsKey(physicalid))
            {
                return _phyiscalDict[physicalid];
            }
            return null;
        }
        #endregion

    }
}
