using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Syncless.Profiling
{
    public class ProfileDrive
    {
        private long _lastUpdated;

        
        private DriveInfo _info;
        private string _guid;
        private string _driveName;
        public DriveInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }
        public string PhysicalId {
            get { 
                if(_info!=null){
                    return ProfilingHelper.ExtractDriveName(_info.Name);
                }
                return null;
            }
        }
        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        public string LogicalId
        {
            get { return _guid; }
        }
        public string DriveName
        {
            get { return _driveName; }
            set { _driveName = value; }
        }
        public long LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }
        public ProfileDrive(string guid,string driveName)
        {
            this._info = null;
            this._driveName = driveName;
            this._guid = guid;
        }
        //Check if 2 Drive Profile is Equal
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
