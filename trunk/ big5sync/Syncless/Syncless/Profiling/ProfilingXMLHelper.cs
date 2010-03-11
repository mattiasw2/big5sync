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
            XmlNodeList list = profilexml.GetElementsByTagName("profile");
            Syncless.Core.ServiceLocator.Getlogger(Syncless.Core.ServiceLocator.DEBUG_LOG).WriteLine("" + list.Count);
            if (list.Count != 0)
            {
                XmlElement element = (XmlElement)list.Item(0);
                string profilename = element.GetAttribute("name");
                string lastupdate = element.GetAttribute("lastupdated");
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
            XmlNodeList nodeList = root.GetElementsByTagName("drive");
            foreach (XmlNode node in nodeList)
            {
                XmlElement drive = (XmlElement)node;
                XmlElement logical = (XmlElement)drive.GetElementsByTagName("logical").Item(0);
                
                XmlElement guid = (XmlElement)drive.GetElementsByTagName("guid").Item(0);
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
            XmlElement root = profilexml.CreateElement("profile");
            XmlAttribute nameAttr = profilexml.CreateAttribute("name");
            XmlAttribute timeAttr = profilexml.CreateAttribute("lastupdated");
            nameAttr.Value = profile.ProfileName;
            timeAttr.Value = profile.LastUpdatedTime + "";
            root.SetAttributeNode(nameAttr);
            root.SetAttributeNode(timeAttr);
            return root;
        }
        private static XmlElement CreateElementForMapping(XmlDocument profilexml, ProfileMapping mapping)
        {
            XmlElement map = profilexml.CreateElement("drive");
            XmlElement logical = profilexml.CreateElement("logical");

            XmlElement guid = profilexml.CreateElement("guid");
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
