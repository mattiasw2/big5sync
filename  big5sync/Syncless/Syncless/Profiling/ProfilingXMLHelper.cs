using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Syncless.Helper;
using Syncless.Profiling.Exceptions;

namespace Syncless.Profiling
{
    internal static class ProfilingXMLHelper
    {
        private const string DEFAULT_NAME = "";
        private const string ELE_PROFILILING_ROOT = "profiling";
        
        private const string ELE_PROFILE = "profile";
        private const string ELE_PROFILE_NAME = "profilename";
        private const string ELE_PROFILE_LAST_UPDATED = "last_updated";

        private const string ELE_PROFILE_DRIVE = "drive";
        private const string ELE_PROFILE_DRIVE_NAME = "drivename";
        private const string ELE_PROFILE_DRIVE_LAST_UPDATED = "last_updated";
        private const string ELE_PROFILE_DRIVE_GUID = "guid";
        
        public static void SaveProfile(List<Profile> profileList ,List<string> location){
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement profileEle = CreateProfileElement(profileList[0], xmlDoc);
            xmlDoc.AppendChild(profileEle);

            CommonXmlHelper.SaveXml(xmlDoc, location[0]);
            for (int i = 1; i < profileList.Count; i++)
            {
                xmlDoc.AppendChild(CreateProfileElement(profileList[i],xmlDoc));
            }

            for (int i = 1; i < location.Count; i++)
            {
                CommonXmlHelper.SaveXml(xmlDoc, location[i]);
            }
        }

        public static void SaveProfile(Profile profile, string location)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement profileEle = CreateProfileElement(profile, xmlDoc);
            xmlDoc.AppendChild(profileEle);
            CommonXmlHelper.SaveXml(xmlDoc, location);



        }
        private static XmlElement CreateProfileElement(Profile profile, XmlDocument doc)
        {
            XmlElement profileElement = doc.CreateElement(ELE_PROFILE);
            profileElement.SetAttribute(ELE_PROFILE_NAME, profile.ProfileName);
            profileElement.SetAttribute(ELE_PROFILE_LAST_UPDATED, profile.LastUpdatedTime+"");
            PopulateDrive(profile.ProfileDriveList, profileElement, doc);

            return profileElement;

        }

        private static void PopulateDrive(List<ProfileDrive> driveList, XmlElement profile, XmlDocument doc)
        {
            foreach (ProfileDrive drive in driveList)
            {
                XmlElement element = doc.CreateElement(ELE_PROFILE_DRIVE);
                element.SetAttribute(ELE_PROFILE_DRIVE_GUID, drive.Guid);
                element.SetAttribute(ELE_PROFILE_DRIVE_NAME, drive.DriveName);
                element.SetAttribute(ELE_PROFILE_DRIVE_LAST_UPDATED, drive.LastUpdated+"");

                profile.AppendChild(element);
            }
            
        }



        public static Profile LoadProfile(string path)
        {
            XmlDocument xmlDoc = CommonXmlHelper.LoadXml(path);
            if (xmlDoc == null)
            {
                return CreateDefaultProfile(path);
            }

            XmlNodeList profileElementList = xmlDoc.GetElementsByTagName(ELE_PROFILE);
            List<Profile> profileList = new List<Profile>();

            foreach (XmlNode profile in profileElementList)
            {
                XmlElement profileElement = profile as XmlElement;
                profileList.Add(CreateProfile(profileElement));
            }
            return profileList[0];
        }
        private static Profile CreateProfile(XmlElement profileElement)
        {
            string profileName = profileElement.GetAttribute(ELE_PROFILE_NAME);
            Profile profile = new Profile(profileName);
            XmlNodeList driveList = profileElement.GetElementsByTagName(ELE_PROFILE_DRIVE);
            foreach (XmlNode drive in driveList)
            {
                XmlElement driveElement = drive as XmlElement;
                if (driveElement != null)
                {
                    profile.AddProfileDrive(CreateProfileDrive(driveElement));
                }
            }

            return profile;
        }

        private static ProfileDrive CreateProfileDrive(XmlElement driveElement)
        {
            string guid = driveElement.GetAttribute(ELE_PROFILE_DRIVE_GUID);
            string lastUpdatedString = driveElement.GetAttribute(ELE_PROFILE_LAST_UPDATED);
            long lastUpdated = long.Parse(lastUpdatedString);
            string driveName = driveElement.GetAttribute(ELE_PROFILE_DRIVE_NAME);

            ProfileDrive drive = new ProfileDrive(guid, driveName);
            drive.LastUpdated = lastUpdated;

            return drive;
        }

        public static Profile CreateDefaultProfile(string path)
        {
            Profile profile = new Profile(DEFAULT_NAME);

            SaveProfile(profile, path);
            return profile;
        }
    }
}
