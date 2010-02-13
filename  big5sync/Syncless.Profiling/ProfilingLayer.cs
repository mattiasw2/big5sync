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
        private bool _saved;
        private ProfilingLayer()
        {
            _profile = new Profile("Unnamed");
            _saved = false;
        }
        /// <summary>
        /// Convert a Logical address to a Physical address
        ///    will replace 001:/Lectures with C:/Lectures
        /// </summary>
        /// <param name="path">The Logical Address to be converted</param>
        /// <returns>The Physical Address</returns>
        
        public string ConvertLogicalToPhysical(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(_profile != null);
            return _profile.FindPhysicalFromLogical(path);                
        }
        /// <summary>
        /// Convert a Physical address to a Logical address
        ///    will replace C:/Lectures with 001:/Lectures
        /// </summary>
        /// <param name="path">The Physical Address to be converted</param>
        /// <returns>The Logical Address</returns>
        public string ConvertPhysicalToLogical(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(_profile != null);
            return _profile.FindLogicalFromPhysical(path);
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
        public string GetRelativePath(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.IndexOf(':') != -1);
            return path.Substring(path.IndexOf(':') + 1);

        }
        /// <summary>
        /// Take in a list of logical address , convert them to physical and return only those that are currently available.
        /// </summary>
        /// <param name="pathList">The list of logical address to convert</param>
        /// <returns>The list of available Physical Address.</returns>
        public List<string> ConvertAndFilterToPhysical(List<string> pathList)
        {
            return null;
        }
        /// <summary>
        /// Get the Logical Id for a Drive.
        /// </summary>
        /// <param name="info">The Drive</param>
        /// <returns>The logical Id Assigned to the drive. null - if drive is not assigned.</returns>
        public string GetLogicalId(DriveInfo info)
        {
            return null;
        }
        
        public bool LoadProfile(string path, bool merge)
        {
            _saved = true;
            return true;
        }
        public bool SaveProfile()
        {
            _saved = true;
            return true;
        }
        /*
        private Profile LoadMapping(string path)
        {
            XmlDocument profilexml = LoadFile(path);
            Profile profile = ProcessXML(profilexml);


            return profile;
        }
        private Profile ProcessXML(XmlDocument profilexml)
        {
            List<ProfileMapping> mappings = new List<ProfileMapping>();
            Profile profile = ProcessHeader(profilexml);
            return profile;
        }
        private Profile ProcessHeader(XmlDocument profilexml)
        {
            profilexml.FirstChild;
        }

        private static XmlDocument LoadFile(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlDocument xml = new XmlDocument();
                xml.Load(fs);
            }
            catch (FileNotFoundException fnfe)
            {
                Console.WriteLine(fnfe.StackTrace);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return null;
        }
        */
        private bool SaveMapping(Profile profile,string path)
        {
            XmlDocument xml = ConvertToXMLDocument(profile);
            
            XmlTextWriter textWriter = new XmlTextWriter(path, Encoding.UTF8);
            textWriter.Formatting = Formatting.Indented;
            xml.WriteContentTo(textWriter);
            textWriter.Close();
            return true;
        }
        private XmlDocument ConvertToXMLDocument(Profile profile)
        {
            XmlDocument profilexml = new XmlDocument();
            XmlElement root = CreateRoot(profilexml,profile);
            foreach (ProfileMapping mapping in profile.Mappings)
            {
                XmlElement map = CreateElementForMapping(profilexml, mapping);
                root.AppendChild(map);
            }
            profilexml.AppendChild(root);
            return profilexml;
        }
        private XmlElement CreateRoot(XmlDocument profilexml, Profile profile)
        {
            XmlElement root = profilexml.CreateElement("profile");
            XmlAttribute nameAttr = profilexml.CreateAttribute("name");
            nameAttr.Value = profile.ProfileName;
            root.SetAttributeNode(nameAttr);
            return root;
        }
        private XmlElement CreateElementForMapping(XmlDocument profilexml, ProfileMapping mapping)
        {
            XmlElement map = profilexml.CreateElement("drive");
            XmlElement logical = profilexml.CreateElement("logical");
            XmlElement physical = profilexml.CreateElement("physical");
            XmlElement guid = profilexml.CreateElement("guid");
            Debug.Assert(mapping.GUID != null);
            Debug.Assert(mapping.LogicalAddress != null);
            if (mapping.PhyiscalAddress != null || !mapping.PhyiscalAddress.Equals(""))
            {
                physical.InnerText = mapping.PhyiscalAddress;
            }
            logical.InnerText = mapping.LogicalAddress;
            guid.InnerText = mapping.GUID;

            map.AppendChild(logical);
            map.AppendChild(physical);
            map.AppendChild(guid);
            return map;
        }
        /*
        public void DebugMode()
        {
            _profile = new Profile("new profile");
            _profile.CreateMapping("001", "C:", "128319478127312389178");
            _profile.CreateMapping("002", "D:", "219481938175817381736");

            
        }
        */
    }
}
