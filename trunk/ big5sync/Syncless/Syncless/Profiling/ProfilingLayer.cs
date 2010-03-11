using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Syncless.Profiling.Exceptions;
namespace Syncless.Profiling
{
    public class ProfilingLayer
    {
        public const string RELATIVE_PROFILING_SAVE_PATH = "\\.syncless\\profiling.xml";
        public const string RELATIVE_GUID_SAVE_PATH = "\\.syncless\\guid.id";
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
        public List<string> ConvertAndFilterToPhysical(List<string> pathList)
        {
            List<string> convertedPathList = new List<string>();
            
            foreach (string path in pathList)
            {
                string convertedPath = ConvertLogicalToPhysical(path);
                if (convertedPath != null)
                {
                    convertedPathList.Add(convertedPath);
                }

            }


            return convertedPathList;

        }
        /// <summary>
        /// Take in a list of logical address , convert them to physical and return only those that are currently available.
        /// </summary>
        /// <param name="pathList">The list of logical address to convert</param>
        /// <returns>two array of path. string[0] -> converted path , string[1] -> unconverted path.</returns>
        public List<string>[] ConvertAndFilter(List<string> pathList)
        {
            List<string> convertedPathList = new List<string>();
            List<string> unconvertedPathList = new List<string>();
            foreach (string path in pathList)
            {
                string convertedPath = ConvertLogicalToPhysical(path);
                if (convertedPath != null)
                {
                    convertedPathList.Add(convertedPath);
                }
                else
                {
                    unconvertedPathList.Add(path);
                }

            }

            return new List<string>[2] { convertedPathList, unconvertedPathList }; 
        }
               
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
        /// <summary>
        /// Get the Logical Id for a Drive.
        /// </summary>
        /// <param name="name">The name of the drive</param>
        /// <returns></returns>
        public string GetLogicalIdFromDrive(string name)
        {            
            Debug.Assert(_profile != null);
            return _profile.FindLogicalFromPhysical(name);
        }
        /// <summary>
        /// get the Drive from Logical Id
        /// </summary>
        /// <param name="id">The Logical Id</param>
        /// <returns>The Name of the Drive (i.e 'C')</returns>
        public String GetDriveFromLogicalId(string id)
        {
            Debug.Assert(id != null);
            Debug.Assert(_profile != null);
            return _profile.FindPhysicalFromLogical(id);
        }
        #endregion        

        public void Merge(string path)
        {
            Profile p = ProfilingXMLHelper.LoadProfile(path);
            try
            {
                _profile.Merge(p);
            }
            catch (ProfileConflictException)
            {

            }
        }
        /// <summary>
        /// Save all the profiling xml to all the various Location.
        /// </summary>
        /// <returns>true if the save is complete.</returns>
        public void SaveTo(List<string> savedLocation)
        {   
            ProfilingXMLHelper.SaveProfile(_profile,savedLocation);
        }
        /// <summary>
        /// Initialize the Profiling Layer.
        /// </summary>
        /// <param name="path">The root path for the profiling configuration file.</param>
        /// <returns>true if the profile is load.</returns>
        public bool Init(List<string> paths)
        {
            string path = paths[0];//path 0 is the root.
            try
            {
                Profile p = ProfilingXMLHelper.LoadProfile(path);
                Debug.Assert(p != null);
                _profile = p;
            }
            catch (FileNotFoundException)
            {
                Profile profile = ProfilingXMLHelper.CreateDefaultProfile(path);
                _profile = profile;
                //Since the profile is newly created , no need to traverse all the drive
                
            }
            for (int i = 1; i < paths.Count; i++)
            {
                try
                {
                    if (File.Exists(paths[i]))
                    {
                        Profile p = ProfilingXMLHelper.LoadProfile(paths[i]);
                        _profile.Merge(p);
                    }
                }
                catch (ProfileConflictException)
                {
                    //Conflict
                }
                catch (ProfileNameDifferentException)
                {
                }
            }
            
            DriveInfo[] driveList = DriveInfo.GetDrives();
            
            foreach (DriveInfo driveinfo in driveList)
            {
                UpdateDrive(driveinfo);
            }            
            return true;
        }
        /// <summary>
        /// Update a Drive with it guid and create a mapping in the profile.
        ///   used when a device is plugin or when startup.
        /// </summary>
        /// <param name="driveinfo">The drive to update</param>
        /// <returns>true if drive has a guid. False if it does not.</returns>
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
        /// <summary>
        /// Remove a drive from a mapping. Used when a device is pluged out.
        /// </summary>
        /// <param name="driveinfo">The Drive that was plug out</param>
        /// <returns>true if the drive is being removed.</returns>
        public bool RemoveDrive(DriveInfo driveinfo)
        {
            return _profile.RemoveDrive(ProfilingHelper.ExtractDriveName(driveinfo.Name));
        }
    }
}
