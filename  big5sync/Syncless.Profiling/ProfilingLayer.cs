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
        private  const string RELATIVE_PROFILING_SAVE_PATH = "\\_syncless\\profiling.xml";
        private const string RELATIVE_GUID_SAVE_PATH = "\\_syncless\\guid.id";
        #region Singleton
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
            string logicalid = ExtractDriveName(path);
            string driveid = GetDriveFromLogicalId(logicalid);
            if (driveid == null)
            {
                return null;
            }
            string relativepath = ExtractRelativePath(path);
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
            string driveid = ExtractDriveName(path);
            string logicalid = GetLogicalIdFromDrive(driveid);
            if (logicalid == null)
            {
                if (!create)
                {
                    return null;
                }
                else
                {
                    string guid = GetGUID(driveid);
                    Debug.Assert(guid != null && !guid.Equals(""));
                    _profile.CreateMapping(guid, driveid, guid);
                    logicalid = guid;
                }
            }
            string relativepath = ExtractRelativePath(path);
            return logicalid + ":" + relativepath; 
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

        public XmlDocument ToXml()
        {
            return this.ConvertToXMLDocument(_profile);
        }

        #region convert xmldocument private method

        private XmlDocument ConvertToXMLDocument(Profile profile)
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
        #endregion
               
        #region convert to profile private methods
        private Profile ConvertToProfile(string path)
        {
            XmlDocument profilexml = LoadFile(path);
            if (profilexml == null)
            {
                //TODO
                throw new FileNotFoundException();
            }
            return ConvertToProfile(profilexml);
        }
        private Profile ConvertToProfile(XmlDocument profilexml)
        {
            XmlNodeList list = profilexml.GetElementsByTagName("profile");
            if (list.Count != 0)
            {
                XmlElement element = (XmlElement)list.Item(0);
                string profilename = element.GetAttribute("name");
                Profile profile = new Profile(profilename);
                profile = ProcessListing(profile, element);
                return profile;
            }
            return null;
        }
        private Profile ProcessListing(Profile profile, XmlElement root)
        {
            XmlNodeList nodeList = root.GetElementsByTagName("drive");
            foreach (XmlNode node in nodeList)
            {
                XmlElement drive = (XmlElement)node;
                XmlElement logical = (XmlElement)drive.GetElementsByTagName("logical").Item(0);
                XmlElement physical = (XmlElement)drive.GetElementsByTagName("physical").Item(0);
                XmlElement guid = (XmlElement)drive.GetElementsByTagName("guid").Item(0);
                profile.CreateMapping(logical.InnerText, physical.InnerText, guid.InnerText);
            }
            return profile;
        }

        #endregion
                       
        #region static methods 
        private static XmlDocument LoadFile(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlDocument xml = new XmlDocument();
                xml.Load(fs);
                return xml;
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


        }
        private static string ExtractRelativePath(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.IndexOf(':') != -1);
            return path.Substring(path.IndexOf(':') + 1);

        }
        private static string ExtractDriveName(string path){
            Debug.Assert(path!=null);
            Debug.Assert(path.IndexOf(':') != -1);
            return path.Substring(0,path.IndexOf(':'));
        }
        #endregion
        
        #region Save method Deprecated

        private bool SaveMapping(Profile profile,string path)
        {
            XmlDocument xml = ConvertToXMLDocument(profile);
            XmlTextWriter textWriter = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);
                textWriter = new XmlTextWriter(fs, Encoding.UTF8);
                textWriter.Formatting = Formatting.Indented;
                xml.WriteContentTo(textWriter);
            }
            catch (IOException io)
            {
                //TODO : Error Log
                Console.WriteLine(io.StackTrace);
                return false;
            }finally{
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
            return true;
        }
        
        #endregion

        public bool Init(string path)
        {
            try
            {
                Profile p = ConvertToProfile(path);
                Debug.Assert(p != null);
                _profile = p;
            }
            catch (FileNotFoundException)
            {
                
                Profile profile = CreateDefaultProfile(path);
                _profile = profile;
                //Since the profile is newly created , no need to traverse all the drive
                return true;
            }
            DriveInfo[] driveList = DriveInfo.GetDrives();

            foreach (DriveInfo driveinfo in driveList)
            {
                FileInfo info = new FileInfo(driveinfo.RootDirectory.Name + RELATIVE_PROFILING_SAVE_PATH);
                if (info.Exists)
                {
                    Profile profile = ConvertToProfile(info.FullName);
                    if (profile == null)
                    {
                        //TODO throw EXCEPTION
                    }
                    else
                    {
                        try
                        {
                            MergeProfile(profile, _profile);
                        }
                        catch (ProfileConflictException pce)
                        {
                            //Log ?
                        }
                    }
                }
            }
            return true;
        }
        private void MergeProfile(Profile profile1, Profile profile2)
        {

        }
        private Profile CreateDefaultProfile(string path)
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
                    Debug.Assert(fs != null);
                    try
                    {
                        fs.Close();
                    }
                    catch (IOException)
                    {

                    }
                }
                
            }
            return profile;
        }
        private string GetGUID(string driveid)
        {
            FileInfo fileInfo = new FileInfo(driveid + ":" + RELATIVE_GUID_SAVE_PATH);
            if (fileInfo.Exists)
            {
                return ReadGUID(fileInfo);
            }
            else
            {
                return CreateGUID(fileInfo.FullName);
            }
        }
        private string ReadGUID(FileInfo fileInfo)
        {
            Debug.Assert(fileInfo.Exists);
            FileStream fs = fileInfo.Open(FileMode.Open);
            StreamReader reader = new StreamReader(fs);
            string guid = reader.ReadLine();
            return guid;

        }
        private string CreateGUID(string path)
        {
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            FileInfo fileInfo = new FileInfo(path);
            Debug.Assert(!fileInfo.Exists);
            
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

            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(guidString);
            sw.Flush();
            try
            {
                sw.Close();
            }catch(IOException){

            }

            return guidString;
        }
        
    }
}
