using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
namespace Syncless.Profiling
{
    public class ProfilingLayer
    {
        public const string RELATIVE_PROFILING_SAVE_PATH = "\\_syncless\\profiling.xml";
        public const string RELATIVE_GUID_SAVE_PATH = "\\_syncless\\guid.id";
        public const string RELATIVE_PROFILING_ROOT_SAVE_PATH = "profiling.xml";
        #region Singleton/Profile 
        private static ProfilingLayer _instance;
        public static ProfilingLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProfilingLayer();
                }
                return _instance;
            }
        }
        private Profile _profile;
        private ProfilingLayer()
        {
            _profile = new Profile("Unnamed");
        }
        public void ChangeProfileName(string newName)
        {
            _profile.ProfileName = newName;
        }
        public Profile CurrentProfile
        {
            get { return _profile; }
        }
        #endregion

        #region conversion methods
        /// <summary>
        /// Convert a Logical address to a Physical address
        ///    will replace 001:/Lectures with C:/Lectures
        /// </summary>
        /// <param name="path">The Logical Address to be converted</param>
        /// <returns>The Physical Address</returns>
        public string ConvertLogicalToPhysical(string path)
        {
            string logicalid = ProfilingHelper.ExtractDriveName(path);
            string driveid = GetDriveFromLogicalId(logicalid);
            if (driveid == null)
            {
                return null;
            }
            string relativepath = ProfilingHelper.ExtractRelativePath(path);
            return driveid + ":" + relativepath;
            
        }
        /// <summary>
        /// Convert a Physical address to a Logical address
        ///    will replace C:/Lectures with 001:/Lectures
        /// </summary>
        /// <param name="path">The Physical Address to be converted</param>
        /// <returns>The Logical Address</returns>
        public string ConvertPhysicalToLogical(string path,bool create)
        {
            string driveid = ProfilingHelper.ExtractDriveName(path);
            string logicalid = GetLogicalIdFromDrive(driveid);
            if (logicalid == null)
            {
                if (!create)
                {
                    return null;
                }
                else
                {
                    string guid = ProfilingGUIDHelper.GetGUID(driveid);
                    Debug.Assert(guid != null && !guid.Equals(""));
                    _profile.CreateMapping(guid, driveid, guid);
                    logicalid = guid;
                }
            }
            string relativepath = ProfilingHelper.ExtractRelativePath(path);
            return logicalid + ":" + relativepath; 
        }
        /// <summary>
        /// Take in a list of logical address , convert them to physical and return only those that are currently available.
        /// </summary>
        /// <param name="pathList">The list of logical address to convert</param>
        /// <returns>The list of available Physical Address.</returns>
        public List<string> ConvertAndFilterToPhysical(List<string> pathList)
        {
            List<string> returnPathList = new List<string>();
            foreach (string path in pathList)
            {
                string convertedPath = ConvertLogicalToPhysical(path);
                if (convertedPath != null)
                {
                    returnPathList.Add(convertedPath);
                }
            }
            return returnPathList;
        }
        /// <summary>
        /// Get the relative path of a path.
        ///    001:/Lectures will return /Lectures
        ///    C:/Lectures will return /Lectures
        ///    C:/ will return /
        ///    path assert not null.
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <returns>The Relative path</returns>
        
        /// <summary>
        /// Get the Logical Id for a Drive.
        /// </summary>
        /// <param name="info">The Drive</param>
        /// <returns>The logical Id Assigned to the drive. null - if drive is not assigned.</returns>
        public string GetLogicalIdFromDrive(DriveInfo info)
        {
            Debug.Assert(info != null);
            String name = info.Name.Substring(0, info.Name.IndexOf(":"));
            return GetLogicalIdFromDrive(name);
        }
        public string GetLogicalIdFromDrive(string name)
        {            
            Debug.Assert(_profile != null);
            return _profile.FindLogicalFromPhysical(name);
        }
        public String GetDriveFromLogicalId(string id)
        {
            Debug.Assert(id != null);
            Debug.Assert(_profile != null);
            return _profile.FindPhysicalFromLogical(id);
        }
        #endregion        

        public bool SaveToAllUsedDrive()
        {
            return ProfilingXMLHelper.SaveToAllDrive(_profile);
        }
        
        public bool Init(string path)
        {
            try
            {
                Profile p = ProfilingXMLHelper.ConvertToProfile(path);
                Debug.Assert(p != null);
                _profile = p;
            }
            catch (FileNotFoundException)
            {
                Profile profile = ProfilingXMLHelper.CreateDefaultProfile(path);
                _profile = profile;
                //Since the profile is newly created , no need to traverse all the drive
                return true;
            }
            DriveInfo[] driveList = DriveInfo.GetDrives();

            foreach (DriveInfo driveinfo in driveList)
            {
                #region Get Profiling XML
                FileInfo info = new FileInfo(driveinfo.RootDirectory.Name + RELATIVE_PROFILING_SAVE_PATH);
                if (info.Exists)
                {
                    Profile profile = null;
                    try
                    {
                        profile = ProfilingXMLHelper.ConvertToProfile(info.FullName);
                    }
                    catch (FileNotFoundException)
                    {

                    }
                    if (profile == null)
                    {
                        //TODO throw EXCEPTION
                    }
                    else
                    {
                        try
                        {
                            _profile.Merge(profile);
                        }
                        catch (ProfileConflictException pce)
                        {
                            //Log ?
                        }
                    }
                }
                #endregion
            }
            
            foreach (DriveInfo driveinfo in driveList)
            {
                UpdateDrive(driveinfo);
            }
            SaveToAllUsedDrive();
            return true;
        }

        public bool UpdateDrive(DriveInfo driveinfo)
        {
            FileInfo info = new FileInfo(driveinfo.RootDirectory.Name + RELATIVE_GUID_SAVE_PATH);
            if (info.Exists)
            {
                string guid = ProfilingGUIDHelper.ReadGUID(info);
                string driveid = ProfilingHelper.ExtractDriveName(info.FullName);
                _profile.UpdateDrive(guid, driveid);
                return true;
            }
            return false;
        }

        


    }
}
