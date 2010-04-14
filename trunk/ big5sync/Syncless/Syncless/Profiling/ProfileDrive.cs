/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System.IO;
namespace Syncless.Profiling
{
    /// <summary>
    /// ProfileDrive class encloses properties of a drive.
    /// </summary>
    public class ProfileDrive
    {
        #region attributes

        /// <summary>
        /// Last updated date
        /// </summary>
        public long LastUpdated { get; set; }

        /// <summary>
        /// Drive info
        /// </summary>
        public DriveInfo Info { get; set; }

        /// <summary>
        /// GUID
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Drive name
        /// </summary>
        public string DriveName { get; set; }

        /// <summary>
        /// Physical id
        /// </summary>
        public string PhysicalId
        {
            get 
            { 
                if (Info != null)
                {
                    return ProfilingHelper.ExtractDriveName(Info.Name);
                }
                return null;
            }
        }
        
        /// <summary>
        /// Logical id
        /// </summary>
        public string LogicalId
        {
            get { return Guid; }
        }
        #endregion

        /// <summary>
        /// Constructor for ProfileDrive
        /// </summary>
        /// <param name="guid">The GUID of the drive</param>
        /// <param name="driveName">The name of the drive</param>
        public ProfileDrive(string guid,string driveName)
        {
            Info = null;
            DriveName = driveName;
            Guid = guid;
        }

        /// <summary>
        /// Check if the given profile drive is the same as the current profile drive
        /// </summary>
        /// <param name="drive">The profile drive that is to be compared</param>
        /// <returns>True if the given profile drive is the same as the current profile drive, else false</returns>
        public bool Equals(ProfileDrive drive)
        {
            int matchCount = 0;
            if (LogicalId.Equals(drive.LogicalId))
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
