using System;
using System.Collections.Generic;
using System.Xml;
/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using Syncless.Helper;
using Syncless.Logging;
using Syncless.Core;
namespace Syncless.Profiling
{
    /// <summary>
    /// ProfilingXMLHelper class provides XML-related operations to load and save profiling.xml file.
    /// </summary>
    internal static class ProfilingXMLHelper
    {
        /// <summary>
        /// The default name of the profile
        /// </summary>
        public const string DEFAULT_NAME = "";

        #region Xml tag name
        /// <summary>
        /// The tag name for profiling root element
        /// </summary>
        private const string ELE_PROFILING_ROOT = "profiling";

        /// <summary>
        /// The tag name for profile root element
        /// </summary>
        private const string ELE_PROFILE_ROOT = "profile";
        /// <summary>
        /// The attribute name for profile name attribute
        /// </summary>
        private const string ATTR_PROFILE_NAME = "profilename";
        /// <summary>
        /// The attribute name for profile last updated attribute
        /// </summary>
        private const string ATTR_PROFILE_LAST_UPDATED = "last_updated";

        /// <summary>
        /// The tag name for profile drive root element
        /// </summary>
        private const string ELE_PROFILE_DRIVE_ROOT = "drive";
        /// <summary>
        /// The attribute name for profile drive name attribute
        /// </summary>
        private const string ATTR_PROFILE_DRIVE_NAME = "drivename";
        /// <summary>
        /// The attribute name for profile drive last updated attribute
        /// </summary>
        private const string ATTR_PROFILE_DRIVE_LAST_UPDATED = "last_updated";
        /// <summary>
        /// The attribute name for profile drive GUID attribute
        /// </summary>
        private const string ATTR_PROFILE_DRIVE_GUID = "guid";
        #endregion

        #region Save Profile
        /// <summary>
        /// Save the given profile to the given location
        /// </summary>
        /// <param name="profile">The profile to be saved</param>
        /// <param name="location">The location for which the profile is to be saved to</param>
        public static void SaveProfile(Profile profile, string location)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement(ELE_PROFILING_ROOT);
            xmlDoc.AppendChild(root);
            XmlElement profileEle = CreateProfileElement(profile, xmlDoc);
            root.AppendChild(profileEle);
            CommonXmlHelper.SaveXml(xmlDoc, location);
        }
        
        /// <summary>
        /// Append the given profile to the Xml document stored in the given list of locations
        /// </summary>
        /// <param name="profile">The profile to be appended</param>
        /// <param name="locations">The list of locations containing the Xml documents for which
        /// the given profile is to be appended to</param>
        public static void AppendProfile(Profile profile, List<string> locations)
        {
            foreach (string path in locations)
            {
                try
                {
                    XmlDocument xmlDoc = CommonXmlHelper.LoadXml(path);
                    if (xmlDoc == null)
                    {
                        //if the xml does not exist, just write it to the place
                        SaveProfile(profile, path);
                    }
                    else
                    {
                        //Loaded XML , replace the old profile data with a new.
                        //Find the Profile with the same name
                        XmlNode node = xmlDoc.SelectSingleNode(@"//" + ELE_PROFILE_ROOT + "[@" + ATTR_PROFILE_NAME + @"='" + profile.ProfileName + @"']");
                        XmlElement profileNode = CreateProfileElement(profile, xmlDoc);
                        XmlNode profilingRootNode = xmlDoc.SelectSingleNode(@"//" + ELE_PROFILING_ROOT);
                        if (node != null)
                        {
                            profilingRootNode.RemoveChild(node);
                        }
                        profilingRootNode.AppendChild(profileNode);
                    }
                }
                catch (Exception)
                {

                }
            }
        }
        
        /// <summary>
        /// Create a profile xml element storing the attributes of the given profile
        /// </summary>
        /// <param name="profile">The profile to be used to create the xml element</param>
        /// <param name="xmlDoc">The xml document for which the created xml element belongs to</param>
        /// <returns></returns>
        private static XmlElement CreateProfileElement(Profile profile, XmlDocument xmlDoc)
        {
            XmlElement profileElement = xmlDoc.CreateElement(ELE_PROFILE_ROOT);
            profileElement.SetAttribute(ATTR_PROFILE_NAME, profile.ProfileName);
            profileElement.SetAttribute(ATTR_PROFILE_LAST_UPDATED, profile.LastUpdatedTime + "");
            PopulateDrive(profile.ProfileDriveList, profileElement, xmlDoc);
            return profileElement;
        }
        
        /// <summary>
        /// Create an xml element for each profile drive in the given list of profile
        /// </summary>
        /// <param name="driveList">The list of profile drives for which xml element is to be created</param>
        /// <param name="profile">The profile root element for which the profile drive xml elements belong to</param>
        /// <param name="xmlDoc">The xml document for which the profile drive xml elements belong to</param>
        private static void PopulateDrive(List<ProfileDrive> driveList, XmlElement profile, XmlDocument xmlDoc)
        {
            foreach (ProfileDrive drive in driveList)
            {
                XmlElement element = xmlDoc.CreateElement(ELE_PROFILE_DRIVE_ROOT);
                element.SetAttribute(ATTR_PROFILE_DRIVE_GUID, drive.Guid);
                element.SetAttribute(ATTR_PROFILE_DRIVE_NAME, drive.DriveName);
                element.SetAttribute(ATTR_PROFILE_DRIVE_LAST_UPDATED, drive.LastUpdated + "");
                profile.AppendChild(element);
            }
        }
        #endregion

        #region Load Profile
        /// <summary>
        /// Load the list of profiles from the xml document loaded from given path
        /// </summary>
        /// <param name="path">The path for which the xml document is saved at</param>
        /// <returns>The list of profiles loaded from the xml document</returns>
        public static List<Profile> LoadProfile(string path)
        {
            XmlDocument xmlDoc = CommonXmlHelper.LoadXml(path);
            if (xmlDoc == null)
            {
                return null;
            }

            XmlNodeList profileElementList = xmlDoc.GetElementsByTagName(ELE_PROFILE_ROOT);
            List<Profile> profileList = new List<Profile>();

            foreach (XmlNode profile in profileElementList)
            {
                XmlElement profileElement = profile as XmlElement;
                Profile profileObj = CreateProfile(profileElement);
                if (profileObj == null)
                {
                    return null;
                }
                profileList.Add(profileObj);
            }
            return profileList;
        }
        
        /// <summary>
        /// Load the first profile from the xml document saved at the given path
        /// </summary>
        /// <param name="path">The path for which the xml document is saved at</param>
        /// <returns>The first profile saved in the xml document</returns>
        public static Profile LoadSingleProfile(string path)
        {
            XmlDocument xmlDoc = CommonXmlHelper.LoadXml(path);
            if (xmlDoc == null)
            {
                return CreateDefaultProfile(path);
            }

            XmlNodeList profileElementList = xmlDoc.GetElementsByTagName(ELE_PROFILE_ROOT);

            XmlElement profileElement = profileElementList[0] as XmlElement;
            
            return CreateProfile(profileElement);
        }
        
        /// <summary>
        /// Load a profile having the given profile name from the xml document saved at the given path
        /// </summary>
        /// <param name="path">The path for which the xml document is saved at</param>
        /// <param name="profileName">The name of the profile to be loaded</param>
        /// <returns>The profile having the given profile name</returns>
        public static Profile LoadSingleProfile(string path, string profileName)
        {
            XmlDocument xmlDoc = CommonXmlHelper.LoadXml(path);
            if (xmlDoc == null)
            {
                return null;
            }
            XmlNode selectedProfile = xmlDoc.SelectSingleNode(@"\\" + ELE_PROFILE_ROOT + "[@" + ATTR_PROFILE_NAME + "=" + profileName + "]");
            if(selectedProfile == null){
                return null;
            }
            XmlElement profileElement = selectedProfile as XmlElement;
            return CreateProfile(profileElement);
        }
        
        /// <summary>
        /// Create a profile from the attributes of the given profile xml element
        /// </summary>
        /// <param name="profileElement">The xml element for which the profile is to be created from</param>
        /// <returns>The profile created from the given profile xml element</returns>
        private static Profile CreateProfile(XmlElement profileElement)
        {
            string profileName = profileElement.GetAttribute(ATTR_PROFILE_NAME);
            Profile profile = new Profile(profileName);
            XmlNodeList driveList = profileElement.GetElementsByTagName(ELE_PROFILE_DRIVE_ROOT);
            foreach (XmlNode drive in driveList)
            {
                XmlElement driveElement = drive as XmlElement;
                if (driveElement != null)
                {
                    ProfileDrive driveObj = CreateProfileDrive(driveElement);
                    if (driveObj == null)
                    {
                        //Fail Load
                        return null;
                    }
                    profile.AddProfileDrive(driveObj);
                }
            }

            return profile;
        }

        /// <summary>
        /// Create a profile drive from the attribtues of the given profile drive xml element
        /// </summary>
        /// <param name="driveElement">The xml element for which the profile drive is to be created from</param>
        /// <returns>The profile drive created from the given profile drive xml element</returns>
        private static ProfileDrive CreateProfileDrive(XmlElement driveElement)
        {
            try
            {
                string guid = driveElement.GetAttribute(ATTR_PROFILE_DRIVE_GUID);
                string lastUpdatedString = driveElement.GetAttribute(ATTR_PROFILE_LAST_UPDATED);

                long lastUpdated = long.Parse(lastUpdatedString);

                string driveName = driveElement.GetAttribute(ATTR_PROFILE_DRIVE_NAME);

                ProfileDrive drive = new ProfileDrive(guid, driveName);
                drive.LastUpdated = lastUpdated;

                return drive;
            }
            catch (Exception)
            {
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.UNKNOWN,"Error Loading Profile"));
                return null;
            }
        }
        #endregion

        /// <summary>
        /// Create and save the default profile at the given path
        /// </summary>
        /// <param name="path">The path for which the default profile is to be saved to</param>
        /// <returns>The default profile created</returns>
        public static Profile CreateDefaultProfile(string path)
        {
            Profile profile = new Profile(DEFAULT_NAME);

            SaveProfile(profile, path);
            return profile;
        }
    }
}
