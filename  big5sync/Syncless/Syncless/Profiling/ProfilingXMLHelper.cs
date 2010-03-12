using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
namespace Syncless.Profiling
{
    internal class ProfilingXMLHelper
    {
        #region XML ELEMENT 
        public const string ELE_PROFILE_ROOT = "profile";
        public const string ELE_PROFILE_ROOT_NAME = "name";
        public const string ELE_PROFILE_ROOT_LASTUPDATED = "lastupdated";

        public const string ELE_DRIVE_ROOT = "drive";
        public const string ELE_DRIVE_LOGICAL = "logical";
        public const string ELE_DRIVE_GUID = "guid";
        #endregion



        #region Save Profile
        public static void SaveProfile(Profile profile, List<string> paths)
        {
            XmlDocument xml = ConvertToXMLDocument(profile);
            UpdateLastUpdateTime(xml, DateTime.Now.Ticks);
            foreach (string path in paths)
            {
                SaveProfile(xml, path);
            }
        }
        private static void SaveProfile(XmlDocument xml, string path)
        {
            XmlTextWriter textWriter = null;
            FileStream fs = null;
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                fs = new FileStream(path, FileMode.OpenOrCreate);
                textWriter = new XmlTextWriter(fs, Encoding.UTF8);
                textWriter.Formatting = Formatting.Indented;
                xml.WriteContentTo(textWriter);
            }
            catch (IOException io)
            {
                //TODO : Error Log
                Console.WriteLine(io.StackTrace);                
            }
            finally
            {
                if (textWriter != null)
                {
                    try
                    {
                        textWriter.Close();
                    }
                    catch (Exception)
                    {
                    }
                }

            }

        }
        #endregion      
        public static Profile CreateDefaultProfile(string path)
        {
            Profile profile = new Profile("Unnamed Profile");
            XmlDocument xml = ConvertToXMLDocument(profile);
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                FileStream fs = null;
                try
                {
                    fs = fileInfo.Create();

                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                    fs = fileInfo.Create();
                }
                finally
                {
                    if (fs != null)
                    {
                        try
                        {
                            fs.Close();
                        }
                        catch (IOException)
                        {

                        }
                    }
                }

            }
            SaveProfile(xml, fileInfo.FullName);
            return profile;
        }
       
        private static XmlDocument LoadFile(string path)
        {
            FileStream fs = null;
            try
            {
                FileInfo info = new FileInfo(path);
                if (!info.Exists)
                {
                    return null;
                }
                fs = info.Open(FileMode.Open);
                XmlDocument xml = new XmlDocument();
                xml.Load(fs);
                return xml;
            }
            catch (XmlException xml)
            {
                Console.WriteLine(xml.ToString());
                return null;
            }
            finally
            {
                if (fs != null)
                {
                    try { fs.Close(); }
                    catch (Exception) { }
                }
            }

        }
        
        #region convert to profile public methods
        public static Profile LoadProfile(string path)
        {
            XmlDocument profilexml = ProfilingXMLHelper.LoadFile(path);
            if (profilexml == null)
            {
                //TODO
                throw new FileNotFoundException();
            }
            return ConvertToProfile(profilexml);
        }
        private static Profile ConvertToProfile(XmlDocument profilexml)
        {
            XmlNodeList list = profilexml.GetElementsByTagName(ELE_PROFILE_ROOT);
            Syncless.Core.ServiceLocator.Getlogger(Syncless.Core.ServiceLocator.DEBUG_LOG).WriteLine("" + list.Count);
            if (list.Count != 0)
            {
                XmlElement element = (XmlElement)list.Item(0);
                string profilename = element.GetAttribute(ELE_PROFILE_ROOT_NAME);
                string lastupdate = element.GetAttribute(ELE_PROFILE_ROOT_LASTUPDATED);
                long lastUpdatedLong = long.Parse(lastupdate);
                Profile profile = new Profile(profilename);
                profile.LastUpdatedTime = lastUpdatedLong;
                profile = ProcessListing(profile, element);
                return profile;
            }
            //TODO need to throw exception here =D
            throw new FileNotFoundException();
        }
        private static Profile ProcessListing(Profile profile, XmlElement root)
        {
            XmlNodeList nodeList = root.GetElementsByTagName(ELE_DRIVE_ROOT);
            foreach (XmlNode node in nodeList)
            {
                XmlElement drive = (XmlElement)node;
                XmlElement logical = (XmlElement)drive.GetElementsByTagName(ELE_DRIVE_LOGICAL).Item(0);
                
                XmlElement guid = (XmlElement)drive.GetElementsByTagName(ELE_DRIVE_GUID).Item(0);
                profile.CreateMapping(logical.InnerText, "", guid.InnerText);
            }
            return profile;
        }
        #endregion

        #region convert xmldocument private method
        private static XmlDocument ConvertToXMLDocument(Profile profile)
        {
            XmlDocument profilexml = new XmlDocument();
            XmlElement root = CreateRoot(profilexml, profile);
            foreach (ProfileMapping mapping in profile.Mappings)
            {
                XmlElement map = CreateElementForMapping(profilexml, mapping);
                root.AppendChild(map);
            }
            profilexml.AppendChild(root);
            return profilexml;
        }
        private static XmlElement CreateRoot(XmlDocument profilexml, Profile profile)
        {
            XmlElement root = profilexml.CreateElement(ELE_PROFILE_ROOT);
            XmlAttribute nameAttr = profilexml.CreateAttribute(ELE_PROFILE_ROOT_NAME);
            XmlAttribute timeAttr = profilexml.CreateAttribute(ELE_PROFILE_ROOT_LASTUPDATED);
            nameAttr.Value = profile.ProfileName;
            timeAttr.Value = profile.LastUpdatedTime + "";
            root.SetAttributeNode(nameAttr);
            root.SetAttributeNode(timeAttr);
            return root;
        }
        private static XmlElement CreateElementForMapping(XmlDocument profilexml, ProfileMapping mapping)
        {
            XmlElement map = profilexml.CreateElement(ELE_DRIVE_ROOT);
            XmlElement logical = profilexml.CreateElement(ELE_DRIVE_LOGICAL);

            XmlElement guid = profilexml.CreateElement(ELE_DRIVE_GUID);
            Debug.Assert(mapping.GUID != null);
            Debug.Assert(mapping.LogicalAddress != null);
            logical.InnerText = mapping.LogicalAddress;
            guid.InnerText = mapping.GUID;

            map.AppendChild(logical);

            map.AppendChild(guid);
            return map;
        }
        private static void UpdateLastUpdateTime(XmlDocument doc, long date)
        {
            XmlNode node = doc.SelectSingleNode("//profile//@lastupdated");
            node.Value = date + "";
        }
        #endregion
    }
}
