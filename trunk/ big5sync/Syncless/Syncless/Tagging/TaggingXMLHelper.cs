using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Syncless.Tagging
{
    static class TaggingXMLHelper
    {
        private static XmlDocument ConvertTaggingProfileToXml(TaggingProfile taggingProfile)
        {
            XmlDocument TaggingDataDocument = new XmlDocument();
            XmlElement taggingElement = TaggingDataDocument.CreateElement("tagging");
            XmlElement profileElement = TaggingDataDocument.CreateElement("profile");
            profileElement.SetAttribute("name", taggingProfile.ProfileName);
            profileElement.SetAttribute("createdDate", taggingProfile.Created.ToString());
            profileElement.SetAttribute("lastUpdated", taggingProfile.LastUpdated.ToString());
            foreach (Tag tag in taggingProfile.TagList)
            {
                profileElement.AppendChild(TaggingXMLHelper.CreateTagElement(TaggingDataDocument, tag));
            }
            taggingElement.AppendChild(profileElement);
            TaggingDataDocument.AppendChild(taggingElement);
            return TaggingDataDocument;
        }

        private static TaggingProfile ConvertXmlToTaggingProfile(XmlDocument xml)
        {
            XmlElement profileElement = (XmlElement)xml.GetElementsByTagName("profile").Item(0);
            TaggingProfile taggingProfile = TaggingXMLHelper.CreateTaggingProfile(profileElement);
            XmlNodeList tagList = profileElement.ChildNodes;
            foreach (XmlElement tagElement in tagList)
            {
                if (tagElement.Name.Equals("folderTag"))
                {
                    Tag tag = TaggingXMLHelper.CreateTagFromXml(tagElement);
                    taggingProfile.TagList.Add(tag);
                }
            }
            return taggingProfile;
        }

        public static void SaveTo(TaggingProfile taggingProfile , List<string> xmlFilePaths)
        {
            XmlDocument xml = ConvertTaggingProfileToXml(taggingProfile);

            foreach (string path in xmlFilePaths)
            {
                TaggingXMLHelper.SaveXml(xml, path);
            }
        }

        public static TaggingProfile LoadFrom(string xmlFilePath)
        {
            TaggingProfile taggingProfile = new TaggingProfile();
            XmlDocument xml = TaggingXMLHelper.LoadXml(xmlFilePath);
            if (xml != null)
            {
                taggingProfile = ConvertXmlToTaggingProfile(xml);
                return taggingProfile;
            }
            else
            {
                return null;
            }
        }

        internal static bool SaveXml(XmlDocument xml, string path)
        {
            XmlTextWriter textWriter = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Create);
                textWriter = new XmlTextWriter(fs, Encoding.UTF8);
                textWriter.Formatting = Formatting.Indented;
                xml.WriteContentTo(textWriter);
            }
            catch (IOException)
            {
                return false;
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
            return true;
        }

        internal static XmlDocument LoadXml(string path)
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
            catch (XmlException)
            {
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

        #region create tagging profile
        internal static TaggingProfile CreateTaggingProfile(XmlElement profileElement)
        {
            TaggingProfile taggingProfile = new TaggingProfile();
            string profilename = profileElement.GetAttribute("name");
            long profilecreated = long.Parse(profileElement.GetAttribute("createdDate"));
            long profilelastupdated = long.Parse(profileElement.GetAttribute("lastUpdated"));
            taggingProfile.ProfileName = profilename;
            taggingProfile.Created = profilecreated;
            taggingProfile.LastUpdated = profilelastupdated;
            return taggingProfile;
        }

        internal static Tag CreateTagFromXml(XmlElement tagElement)
        {
            string tagname = tagElement.GetAttribute("name");
            long created = long.Parse(tagElement.GetAttribute("created"));
            long lastupdated = long.Parse(tagElement.GetAttribute("lastUpdated"));
            Tag tag = new Tag(tagname, created);
            tag.LastUpdated = lastupdated;
            XmlNodeList pathList = tagElement.ChildNodes;
            foreach (XmlElement path in pathList)
            {
                TaggedPath taggedPath = CreatePath(path);
                tag.PathList.Add(taggedPath);
            }
            return tag;
        }

        private static TaggedPath CreatePath(XmlElement path)
        {
            long pathcreated, pathlastupdated;
            string pathname;
            pathcreated = long.Parse(path.GetAttribute("created"));
            pathlastupdated = long.Parse(path.GetAttribute("lastUpdated"));
            XmlNodeList pathValues = path.ChildNodes;
            pathname = pathValues.Item(0).InnerText;
            TaggedPath taggedPath = CreateTaggedPath(pathcreated, pathlastupdated, pathname);
            return taggedPath;
        }

        private static TaggedPath CreateTaggedPath(long pathcreated, long pathlastupdated, string pathname)
        {
            TaggedPath taggedPath = new TaggedPath();
            taggedPath.Path = pathname;
            taggedPath.Created = pathcreated;
            taggedPath.LastUpdated = pathlastupdated;
            taggedPath.LogicalDriveId = TaggingHelper.GetLogicalID(pathname);
            return taggedPath;
        }
        #endregion

        #region create xml document
        internal static XmlElement CreateTagElement(XmlDocument TaggingDataDocument, Tag tag)
        {
            XmlElement tagElement = TaggingDataDocument.CreateElement("folderTag");
            tagElement.SetAttribute("name", tag.TagName);
            tagElement.SetAttribute("created", tag.Created.ToString());
            tagElement.SetAttribute("lastUpdated", tag.LastUpdated.ToString());
            foreach (TaggedPath path in tag.PathList)
            {
                tagElement.AppendChild(CreateTaggedFolderElement(TaggingDataDocument, path));
            }
            return tagElement;
        }

        private static XmlElement CreateTaggedFolderElement(XmlDocument TaggingDataDocument, TaggedPath path)
        {
            XmlElement taggedFolderElement = TaggingDataDocument.CreateElement("taggedFolder");
            taggedFolderElement.SetAttribute("created", path.Created.ToString());
            taggedFolderElement.SetAttribute("lastUpdated", path.LastUpdated.ToString());
            XmlElement folderPathElement = TaggingDataDocument.CreateElement("path");
            folderPathElement.InnerText = path.Path;
            taggedFolderElement.AppendChild(folderPathElement);
            return taggedFolderElement;
        }
        #endregion
    }
}
