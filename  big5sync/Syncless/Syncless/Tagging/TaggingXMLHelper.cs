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
            foreach (FolderTag folderTag in taggingProfile.FolderTagList)
            {
                profileElement.AppendChild(TaggingXMLHelper.CreateFolderTagElement(TaggingDataDocument, folderTag));
            }
            foreach (FileTag fileTag in taggingProfile.FileTagList)
            {
                profileElement.AppendChild(TaggingXMLHelper.CreateFileTagElement(TaggingDataDocument, fileTag));
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
            foreach (XmlElement tag in tagList)
            {
                if (tag.Name.Equals("folderTag"))
                {
                    FolderTag folderTag = TaggingXMLHelper.CreateFolderTagFromXml(tag);
                    taggingProfile.FolderTagList.Add(folderTag);
                }
                else
                {
                    FileTag fileTag = TaggingXMLHelper.CreateFileTagFromXml(tag);
                    taggingProfile.FileTagList.Add(fileTag);
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

        internal static FolderTag CreateFolderTagFromXml(XmlElement tag)
        {
            string tagname = tag.GetAttribute("name");
            long created = long.Parse(tag.GetAttribute("created"));
            long lastupdated = long.Parse(tag.GetAttribute("lastUpdated"));
            FolderTag folderTag = new FolderTag(tagname, created);
            folderTag.LastUpdated = lastupdated;
            XmlNodeList pathList = tag.ChildNodes;
            foreach (XmlElement path in pathList)
            {
                TaggedPath taggedPath = CreatePath(path);
                folderTag.PathList.Add(taggedPath);
            }
            return folderTag;
        }

        internal static FileTag CreateFileTagFromXml(XmlElement tag)
        {
            string tagname = tag.GetAttribute("name");
            long created = long.Parse(tag.GetAttribute("created"));
            long lastupdated = long.Parse(tag.GetAttribute("lastUpdated"));
            FileTag fileTag = new FileTag(tagname, created);
            fileTag.LastUpdated = lastupdated;
            XmlNodeList pathList = tag.ChildNodes;
            foreach (XmlElement path in pathList)
            {
                TaggedPath taggedPath = CreatePath(path);
                fileTag.PathList.Add(taggedPath);
            }
            return fileTag;
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
        internal static XmlElement CreateFolderTagElement(XmlDocument TaggingDataDocument, FolderTag folderTag)
        {
            XmlElement folderTagElement = TaggingDataDocument.CreateElement("folderTag");
            folderTagElement.SetAttribute("name", folderTag.TagName);
            folderTagElement.SetAttribute("created", folderTag.Created.ToString());
            folderTagElement.SetAttribute("lastUpdated", folderTag.LastUpdated.ToString());
            foreach (TaggedPath path in folderTag.PathList)
            {
                folderTagElement.AppendChild(CreateTaggedFolderElement(TaggingDataDocument, path));
            }
            return folderTagElement;
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

        internal static XmlElement CreateFileTagElement(XmlDocument TaggingDataDocument, FileTag fileTag)
        {
            XmlElement fileTagElement = TaggingDataDocument.CreateElement("fileTag");
            fileTagElement.SetAttribute("name", fileTag.TagName);
            fileTagElement.SetAttribute("created", fileTag.Created.ToString());
            fileTagElement.SetAttribute("lastUpdated", fileTag.LastUpdated.ToString());
            foreach (TaggedPath path in fileTag.PathList)
            {
                fileTagElement.AppendChild(CreatedTaggedFileElement(TaggingDataDocument, path));
            }
            return fileTagElement;
        }

        private static XmlElement CreatedTaggedFileElement(XmlDocument TaggingDataDocument, TaggedPath path)
        {
            XmlElement taggedFileElement = TaggingDataDocument.CreateElement("taggedFile");
            taggedFileElement.SetAttribute("created", path.Created.ToString());
            taggedFileElement.SetAttribute("lastUpdated", path.LastUpdated.ToString());
            XmlElement filePathElement = TaggingDataDocument.CreateElement("path");
            filePathElement.InnerText = path.Path;
            taggedFileElement.AppendChild(filePathElement);
            return taggedFileElement;
        }
        #endregion
    }
}
