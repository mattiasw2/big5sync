using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Syncless.Profiling
{
    /// <summary>
    /// ProfileDrive class encloses properties of a drive.
    /// </summary>
    public class ProfileDrive
    {
        #region attributes
        private long _lastUpdated;
        /// <summary>
        /// Last updated date
        /// </summary>
        public long LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }
        
        private DriveInfo _info;
        /// <summary>
        /// Drive info
        /// </summary>
        public DriveInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }
        
        private string _guid;
        /// <summary>
        /// GUID
        /// </summary>
        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        
        private string _driveName;
        /// <summary>
        /// Drive name
        /// </summary>
        public string DriveName
        {
            get { return _driveName; }
            set { _driveName = value; }
        }
        
        /// <summary>
        /// Physical id
        /// </summary>
        public string PhysicalId
        {
            get 
            { 
                if (_info != null)
                {
                    return ProfilingHelper.ExtractDriveName(_info.Name);
                }
                return null;
            }
        }
        
        /// <summary>
        /// Logical id
        /// </summary>
        public string LogicalId
        {
            get { return _guid; }
        }
        #endregion

        /// <summary>
        /// Constructor for ProfileDrive
        /// </summary>
        /// <param name="guid">The GUID of the drive</param>
        /// <param name="driveName">The name of the drive</param>
        public ProfileDrive(string guid,string driveName)
        {
            this._info = null;
            this._driveName = driveName;
            this._guid = guid;
        }

        /// <summary>
        /// Check if the given profile drive is the same as the current profile drive
        /// </summary>
        /// <param name="drive">The profile drive that is to be compared</param>
        /// <returns>True if the given profile drive is the same as the current profile drive, else false</returns>
        public bool Equals(ProfileDrive drive)
        {
            int matchCount = 0;
            if (this.LogicalId.Equals(drive.LogicalId))
            {
                matchCount++;
            }
            if (matchCount == 1)
            {
                return true;
            }
            return false;
        }
    }
}
