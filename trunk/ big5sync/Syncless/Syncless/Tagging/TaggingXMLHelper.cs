using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Syncless.Filters;
namespace Syncless.Tagging
{
    static class TaggingXMLHelper
    {
        #region element name
        private const string ELE_PROFILE_ROOT = "tagging";
        private const string ELE_PROFILE = "profile";
        private const string ELE_PROFILE_NAME = "name";
        private const string ELE_PROFILE_CREATEDDATE = "createdDate";
        private const string ELE_PROFILE_LASTUPDATEDDATE = "lastUpdated";

        private const string ELE_TAG_ROOT = "tag";
        private const string ELE_TAG_NAME = "name";
        private const string ELE_TAG_CREATEDDATE = "created";
        private const string ELE_TAG_UPDATEDDATE = "updated";
        private const string ELE_TAG_ISDELETED = "isDeleted";
        private const string ELE_TAG_DELETEDDATE = "deletedDate";

        private const string ELE_FOLDER_ROOT = "folders";
        private const string ELE_TAGGED_FOLDER = "taggedFolder";
        private const string ELE_TAGGED_FOLDER_CREATED = "created";
        private const string ELE_TAGGED_FOLDER_UPDATED = "updated";
        private const string ELE_TAGGED_FOLDER_PATH = "path";

        private const string ELE_FILTER_ROOT = "filters";
        private const string ELE_FILTER_CHILD_FILTER = "filter";

        private const string ELE_FILTER_TYPE = "type";
        private const string ELE_FILTER_MODE = "mode";
        private const string ELE_FILTER_UPDATED = "updated";
        //Type of Mode
        private const string ELE_FILTER_MODE_INCLUDE = "Include";
        private const string ELE_FILTER_MODE_EXCLUDE = "Exclude";

        //Type of Filter
        #region Extension
        private const string ELE_FILTER_TYPE_EXT = "Extension";
        private const string ELE_FILTER_TYPE_EXT_PATTERN = "pattern";
        #endregion

        private const string ELE_CONFIG_ROOT = "config";
        private const string ELE_CONFIG_SEAMLESS = "seamless";
        private const string ELE_CONFIG_ARCHIVE_ROOT = "archive";
        private const string ELE_CONFIG_ARCHIVE_NAME = "name";
        private const string ELE_CONFIG_ARCHIVE_COUNT = "count";
        #endregion

        public static void SaveTo(TaggingProfile taggingProfile, List<string> xmlFilePaths)
        {
            XmlDocument xml = ConvertTaggingProfileToXml(taggingProfile);
            foreach (string path in xmlFilePaths)
            {
                SaveXml(xml, path);
            }
        }

        private static bool SaveXml(XmlDocument xml, string path)
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
        
        private static XmlDocument ConvertTaggingProfileToXml(TaggingProfile taggingProfile)
        {
            XmlDocument TaggingDataDocument = new XmlDocument();
            XmlElement taggingElement = TaggingDataDocument.CreateElement(ELE_PROFILE_ROOT);
            XmlElement profileElement = TaggingDataDocument.CreateElement(ELE_PROFILE);
            profileElement.SetAttribute(ELE_PROFILE_NAME, taggingProfile.ProfileName);
            profileElement.SetAttribute(ELE_PROFILE_CREATEDDATE, taggingProfile.Created.ToString());
            profileElement.SetAttribute(ELE_PROFILE_LASTUPDATEDDATE, taggingProfile.LastUpdated.ToString());
            foreach (Tag tag in taggingProfile.TagList)
            {
                profileElement.AppendChild(CreateTagElement(TaggingDataDocument, tag));
            }
            taggingElement.AppendChild(profileElement);
            TaggingDataDocument.AppendChild(taggingElement);
            return TaggingDataDocument;
        }

        public static TaggingProfile LoadFrom(string xmlFilePath)
        {
            TaggingProfile taggingProfile = null;
            XmlDocument xml = LoadXml(xmlFilePath);
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
        
        private static TaggingProfile ConvertXmlToTaggingProfile(XmlDocument xml)
        {
            XmlElement profileElement = (XmlElement)xml.GetElementsByTagName(ELE_PROFILE).Item(0);
            TaggingProfile taggingProfile = CreateTaggingProfile(profileElement);
            XmlNodeList tagList = profileElement.ChildNodes;
            foreach (XmlElement tagElement in tagList)
            {
                if (tagElement.Name.Equals(ELE_TAG_ROOT))
                {
                    Tag tag = CreateTagFromXml(tagElement);
                    taggingProfile.TagList.Add(tag);
                }
            }
            return taggingProfile;
        }

        private static XmlDocument LoadXml(string path)
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
        private static TaggingProfile CreateTaggingProfile(XmlElement profileElement)
        {
            string profilename = profileElement.GetAttribute(ELE_PROFILE_NAME);
            long profilecreated = long.Parse(profileElement.GetAttribute(ELE_PROFILE_CREATEDDATE));
            long profilelastupdated = long.Parse(profileElement.GetAttribute(ELE_PROFILE_LASTUPDATEDDATE));
            TaggingProfile taggingProfile = new TaggingProfile(profilecreated);
            taggingProfile.ProfileName = profilename;
            taggingProfile.Created = profilecreated;
            taggingProfile.LastUpdated = profilelastupdated;
            return taggingProfile;
        }

        private static Tag CreateTagFromXml(XmlElement tagElement)
        {
            string tagname = tagElement.GetAttribute(ELE_TAG_NAME);
            long created = long.Parse(tagElement.GetAttribute(ELE_TAG_CREATEDDATE));
            long lastupdated = long.Parse(tagElement.GetAttribute(ELE_TAG_UPDATEDDATE));
            bool isdeleted = bool.Parse(tagElement.GetAttribute(ELE_TAG_ISDELETED));
            long deleteddate = long.Parse(tagElement.GetAttribute(ELE_TAG_DELETEDDATE));
            Tag tag = new Tag(tagname, created);
            tag.LastUpdated = lastupdated;
            tag.IsDeleted = isdeleted;
            tag.DeletedDate = deleteddate;
            XmlElement foldersElement = (XmlElement)tagElement.GetElementsByTagName(ELE_FOLDER_ROOT).Item(0);
            tag.FilteredPathList = CreateFolders(foldersElement);
            XmlElement filters = (XmlElement)tagElement.GetElementsByTagName(ELE_FILTER_ROOT).Item(0);
            tag.Filters = LoadFilterList(filters);
            tag.FiltersUpdated = long.Parse(filters.GetAttribute(ELE_FILTER_UPDATED));
            XmlElement config = (XmlElement)tagElement.GetElementsByTagName(ELE_CONFIG_ROOT).Item(0);
            tag.Config = CreateTagConfig(config);
            return tag;
        }

        private static List<TaggedPath> CreateFolders(XmlElement foldersElement)
        {
            List<TaggedPath> pathList = new List<TaggedPath>();
            foreach (XmlElement path in foldersElement.GetElementsByTagName(ELE_TAGGED_FOLDER))
            {
                TaggedPath taggedPath = CreatePath(path);
                pathList.Add(taggedPath);
            }
            return pathList;
        }

        private static TaggedPath CreatePath(XmlElement path)
        {
            long pathcreated, pathlastupdated;
            string pathname;
            pathcreated = long.Parse(path.GetAttribute(ELE_TAGGED_FOLDER_CREATED));
            pathlastupdated = long.Parse(path.GetAttribute(ELE_TAGGED_FOLDER_UPDATED));
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

        private static List<Filter> LoadFilterList(XmlElement filterListNode)
        {
            List<Filter> filterList = new List<Filter>();
            XmlNodeList elementNodes = filterListNode.GetElementsByTagName(ELE_FILTER_CHILD_FILTER);
            foreach (XmlNode n in elementNodes)
            {
                Filter f = LoadFilter((XmlElement)n);
                filterList.Add(f);
            }
            return filterList;
        }

        private static Filter LoadFilter(XmlElement filterNode)
        {
            string type = filterNode.GetAttribute(ELE_FILTER_TYPE);
            string mode = filterNode.GetAttribute(ELE_FILTER_MODE);
            FilterMode fMode = FilterMode.INCLUDE;
            if (mode.Equals(ELE_FILTER_MODE_EXCLUDE))
            {
                fMode = FilterMode.EXCLUDE;
            }
            else if (mode.Equals(ELE_FILTER_MODE_INCLUDE))
            {
                fMode = FilterMode.INCLUDE;
            }
            else
            {
                //CANNOT HAPPEN!!!!!
            }
            if (type.Equals(ELE_FILTER_TYPE_EXT))
            {
                XmlNodeList list = filterNode.GetElementsByTagName(ELE_FILTER_TYPE_EXT_PATTERN);
                if (list.Count == 0)
                {
                    //THROW EXCEPTION
                }
                XmlElement ext = (XmlElement)list[0];

                ExtensionFilter filter = new ExtensionFilter(ext.InnerText, fMode);
                return filter;
            }
            return null;
        }

        private static TagConfig CreateTagConfig(XmlElement configNode)
        {
            TagConfig config = new TagConfig();
            XmlElement seamlessElement = (XmlElement)configNode.GetElementsByTagName(ELE_CONFIG_SEAMLESS).Item(0);
            XmlElement archiveElement = (XmlElement)configNode.GetElementsByTagName(ELE_CONFIG_ARCHIVE_ROOT).Item(0);
            config.IsSeamless = bool.Parse(seamlessElement.InnerText);
            config.ArchiveFolderName = archiveElement.GetAttribute(ELE_CONFIG_ARCHIVE_NAME);
            config.ArchiveCount = int.Parse(archiveElement.GetAttribute(ELE_CONFIG_ARCHIVE_COUNT));
            return config;
        }
        #endregion

        #region create xml document
        private static XmlElement CreateTagElement(XmlDocument xmlDoc, Tag tag)
        {
            XmlElement tagElement = xmlDoc.CreateElement(ELE_TAG_ROOT);
            tagElement.SetAttribute(ELE_TAG_NAME, tag.TagName);
            tagElement.SetAttribute(ELE_TAG_CREATEDDATE, tag.Created.ToString());
            tagElement.SetAttribute(ELE_TAG_UPDATEDDATE, tag.LastUpdated.ToString());
            tagElement.SetAttribute(ELE_TAG_ISDELETED, tag.IsDeleted.ToString());
            tagElement.SetAttribute(ELE_TAG_DELETEDDATE, tag.DeletedDate.ToString());
            tagElement.AppendChild(CreateFoldersElement(xmlDoc, tag));
            tagElement.AppendChild(CreateFilterElementList(xmlDoc, tag));
            tagElement.AppendChild(CreateConfigElement(xmlDoc, tag));
            return tagElement;
        }

        private static XmlElement CreateFoldersElement(XmlDocument xmlDoc, Tag tag)
        {
            List<TaggedPath> pathList = tag.FilteredPathList;
            XmlElement foldersElement = xmlDoc.CreateElement(ELE_FOLDER_ROOT);
            foreach (TaggedPath path in pathList)
            {
                XmlElement taggedFolderElement = CreateTaggedFolderElement(xmlDoc, path);
                foldersElement.AppendChild(taggedFolderElement);
            }
            return foldersElement;
        }

        private static XmlElement CreateTaggedFolderElement(XmlDocument xmlDoc, TaggedPath path)
        {
            XmlElement taggedFolderElement = xmlDoc.CreateElement(ELE_TAGGED_FOLDER);
            taggedFolderElement.SetAttribute(ELE_TAGGED_FOLDER_CREATED, path.Created.ToString());
            taggedFolderElement.SetAttribute(ELE_TAGGED_FOLDER_UPDATED, path.LastUpdated.ToString());
            XmlElement folderPathElement = xmlDoc.CreateElement(ELE_TAGGED_FOLDER_PATH);
            folderPathElement.InnerText = path.Path;
            taggedFolderElement.AppendChild(folderPathElement);
            return taggedFolderElement;
        }

        private static XmlElement CreateFilterElementList(XmlDocument xmlDoc , Tag tag){
            List<Filter> filters = tag.Filters;
            XmlElement filterRoot = xmlDoc.CreateElement(ELE_FILTER_ROOT);
            filterRoot.SetAttribute(ELE_FILTER_UPDATED, tag.FiltersUpdated.ToString());
            foreach (Filter f in filters)
            {
                XmlElement filterEle = CreateFilterElement(xmlDoc, f);
                filterRoot.AppendChild(filterEle);
            }
            return filterRoot;
        }

        private static XmlElement CreateFilterElement(XmlDocument xmlDoc , Filter filter){
            XmlElement filterElement = xmlDoc.CreateElement(ELE_FILTER_CHILD_FILTER);
            string mode = filter.Mode==FilterMode.INCLUDE?ELE_FILTER_MODE_INCLUDE:ELE_FILTER_MODE_EXCLUDE;
            filterElement.SetAttribute(ELE_FILTER_MODE, mode);
            if (filter is ExtensionFilter)
            {
                string type = ELE_FILTER_TYPE_EXT;
                filterElement.SetAttribute(ELE_FILTER_TYPE, type);
                XmlElement ext = xmlDoc.CreateElement(ELE_FILTER_TYPE_EXT_PATTERN);
                ext.InnerText = ((ExtensionFilter)filter).Pattern;
                filterElement.AppendChild(ext);
            }
            return filterElement;
        }

        private static XmlElement CreateConfigElement(XmlDocument xmlDoc, Tag tag)
        {
            TagConfig config = tag.Config;
            XmlElement configElement = xmlDoc.CreateElement(ELE_CONFIG_ROOT);
            XmlElement seamlessElement = xmlDoc.CreateElement(ELE_CONFIG_SEAMLESS);
            seamlessElement.InnerText = tag.IsSeamless.ToString();
            configElement.AppendChild(seamlessElement);
            XmlElement archiveElement = xmlDoc.CreateElement(ELE_CONFIG_ARCHIVE_ROOT);
            archiveElement.SetAttribute(ELE_CONFIG_ARCHIVE_NAME, config.ArchiveFolderName);
            archiveElement.SetAttribute(ELE_CONFIG_ARCHIVE_COUNT, config.ArchiveCount.ToString());
            configElement.AppendChild(archiveElement);
            return configElement;
        }
        #endregion
    }
}
