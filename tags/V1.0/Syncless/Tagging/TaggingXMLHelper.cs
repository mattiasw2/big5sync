using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Syncless.Filters;
using Syncless.Helper;

namespace Syncless.Tagging
{
    static class TaggingXMLHelper
    {
        #region tagging root
        private const string ELE_TAGGING_ROOT = "tagging";

        #region profile
        private const string ELE_PROFILE_ROOT = "profile";
        #region profile attributes
        private const string ATTR_PROFILE_NAME = "name";
        private const string ATTR_PROFILE_CREATEDDATE = "createdDate";
        private const string ATTR_PROFILE_LASTUPDATEDDATE = "lastUpdatedDate";
        #endregion

        #region profile child - tag list
        private const string ELE_TAG_ROOT = "tag";
        #region tag attributes
        private const string ATTR_TAG_NAME = "name";
        private const string ATTR_TAG_CREATEDDATE = "createdDate";
        private const string ATTR_TAG_LASTUPDATEDDATE = "lastUpdatedDate";
        private const string ATTR_TAG_ISDELETED = "isDeleted";
        private const string ATTR_TAG_DELETEDDATE = "deletedDate";
        #endregion

        #region tag child - taggedfolder list
        private const string ELE_FOLDER_ROOT = "folders";
        #region individual taggedfolder
        private const string ELE_TAGGED_FOLDER_ROOT = "taggedFolder";
        #region taggedfolder attributes
        private const string ATTR_TAGGED_FOLDER_CREATEDDATE = "createdDate";
        private const string ATTR_TAGGED_FOLDER_LASTUPDATEDDATE = "lastUpdatedDate";
        private const string ATTR_TAGGED_FOLDER_ISDELETED = "isDeleted";
        private const string ATTR_TAGGED_FOLDER_DELETEDDATE = "deletedDate";
        #endregion
        #region taggedfolder child - path
        private const string ELE_TAGGED_FOLDER_PATH = "path";
        #endregion
        #endregion
        #endregion

        #region tag child - filter list
        private const string ELE_FILTER_ROOT = "filters";
        #region individual filter
        private const string ELE_FILTER_CHILD_FILTER = "filter";
        #region filter attributes
        private const string ATTR_FILTER_LASTUPDATEDDATE = "lastUpdatedDate";
        #region filter type
        private const string ATTR_FILTER_TYPE = "type";
        private const string VALUE_FILTER_TYPE_EXT = "Extension";
        #endregion
        #region filter mode
        private const string ATTR_FILTER_MODE = "mode";
        private const string VALUE_FILTER_MODE_INCLUDE = "Include";
        private const string VALUE_FILTER_MODE_EXCLUDE = "Exclude";
        #endregion
        #endregion
        #region filter child - pattern
        private const string ELE_FILTER_TYPE_EXT_PATTERN = "pattern";
        #endregion
        #endregion
        #endregion

        #region tag child - config
        private const string ELE_CONFIG_ROOT = "config";
        #region config child - seamless
        private const string ELE_CONFIG_SEAMLESS = "seamless";
        #endregion
        #endregion
        #endregion
        #endregion
        #endregion

        #region save operations
        public static void SaveToLocations(TaggingProfile taggingProfile, List<string> xmlFilePaths)
        {
            foreach (string path in xmlFilePaths)
            {
                SaveTo(taggingProfile, path);
            }
        }

        public static void SaveTo(TaggingProfile taggingProfile, string xmlFilePath)
        {
            XmlDocument xmlDoc = ConvertTaggingProfileToXml(taggingProfile);
            CommonXmlHelper.SaveXml(xmlDoc, xmlFilePath);
        }

        private static XmlDocument ConvertTaggingProfileToXml(TaggingProfile taggingProfile)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement taggingElement = xmldoc.CreateElement(ELE_TAGGING_ROOT);
            XmlElement profileElement = CreateTaggingProfileElement(xmldoc, taggingProfile);
            taggingElement.AppendChild(profileElement);
            xmldoc.AppendChild(taggingElement);
            return xmldoc;
        }

        public static void AppendProfile(TaggingProfile profile, List<string> locations)
        {
            foreach (string location in locations)
            {
                try
                {
                    XmlDocument xmldoc = CommonXmlHelper.LoadXml(location);
                    if (xmldoc == null)
                    {
                        SaveTo(profile, location);
                    }
                    else
                    {
                        XmlElement newProfileElement = CreateTaggingProfileElement(xmldoc, profile);
                        XmlElement oldProfileElement = (XmlElement) xmldoc.SelectSingleNode(@"//" + ELE_PROFILE_ROOT + "[@" + ATTR_PROFILE_NAME + @"='" + profile.ProfileName + @"']");
                        if (oldProfileElement != null)
                        {
                            xmldoc.SelectSingleNode(@"//" + ELE_TAGGING_ROOT).RemoveChild(oldProfileElement);
                        }
                        xmldoc.SelectSingleNode(@"//" + ELE_TAGGING_ROOT).AppendChild(newProfileElement);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
        #endregion

        #region load operations
        public static TaggingProfile LoadFrom(string xmlFilePath)
        {
            TaggingProfile taggingProfile = null;
            XmlDocument xmlDoc = CommonXmlHelper.LoadXml(xmlFilePath);
            if (xmlDoc != null)
            {
                taggingProfile = ConvertXmlToTaggingProfile(xmlDoc);
                return taggingProfile;
            }
            else
            {
                throw new FileNotFoundException("tagging.xml not found.");
            }
        }

        private static TaggingProfile ConvertXmlToTaggingProfile(XmlDocument xmlDoc)
        {
            XmlElement profileElement = (XmlElement)xmlDoc.GetElementsByTagName(ELE_PROFILE_ROOT).Item(0);
            TaggingProfile taggingProfile = CreateTaggingProfile(profileElement);
            XmlNodeList tagList = profileElement.ChildNodes;
            foreach (XmlElement tagElement in tagList)
            {
                if (tagElement.Name.Equals(ELE_TAG_ROOT))
                {
                    Tag tag = CreateTagFromXml(tagElement);
                    lock (taggingProfile.TagList)
                    {
                        taggingProfile.TagList.Add(tag);
                    }
                }
            }
            return taggingProfile;
        }
        #endregion

        #region create tagging profile
        private static TaggingProfile CreateTaggingProfile(XmlElement profileElement)
        {
            string profilename = profileElement.GetAttribute(ATTR_PROFILE_NAME);
            long profilecreated = long.Parse(profileElement.GetAttribute(ATTR_PROFILE_CREATEDDATE));
            long profilelastupdated = long.Parse(profileElement.GetAttribute(ATTR_PROFILE_LASTUPDATEDDATE));
            TaggingProfile taggingProfile = new TaggingProfile(profilecreated);
            taggingProfile.ProfileName = profilename;
            taggingProfile.CreatedDate = profilecreated;
            taggingProfile.LastUpdatedDate = profilelastupdated;
            return taggingProfile;
        }

        private static Tag CreateTagFromXml(XmlElement tagElement)
        {
            string tagname = tagElement.GetAttribute(ATTR_TAG_NAME);
            long created = long.Parse(tagElement.GetAttribute(ATTR_TAG_CREATEDDATE));
            long lastupdated = long.Parse(tagElement.GetAttribute(ATTR_TAG_LASTUPDATEDDATE));
            bool isdeleted = bool.Parse(tagElement.GetAttribute(ATTR_TAG_ISDELETED));
            long deleteddate = long.Parse(tagElement.GetAttribute(ATTR_TAG_DELETEDDATE));
            Tag tag = new Tag(tagname, created);
            tag.LastUpdatedDate = lastupdated;
            tag.IsDeleted = isdeleted;
            tag.DeletedDate = deleteddate;
            XmlNodeList tagChildren = tagElement.ChildNodes;
            foreach (XmlElement tagChild in tagChildren)
            {
                if (tagChild.Name.Equals(ELE_FOLDER_ROOT))
                {
                    tag.FilteredPathList = CreateFolders(tagChild);
                }
                else if (tagChild.Name.Equals(ELE_FILTER_ROOT))
                {
                    tag.Filters = LoadFilterList(tagChild);
                    tag.FiltersUpdatedDate = long.Parse(tagChild.GetAttribute(ATTR_FILTER_LASTUPDATEDDATE));
                }
                else if (tagChild.Name.Equals(ELE_CONFIG_ROOT))
                {
                    tag.Config = CreateTagConfig(tagChild);
                }
            }
            return tag;
        }

        private static List<TaggedPath> CreateFolders(XmlElement foldersElement)
        {
            List<TaggedPath> pathList = new List<TaggedPath>();
            foreach (XmlElement taggedFolder in foldersElement.GetElementsByTagName(ELE_TAGGED_FOLDER_ROOT))
            {
                TaggedPath taggedPath = CreatePath(taggedFolder);
                pathList.Add(taggedPath);
            }
            return pathList;
        }

        private static TaggedPath CreatePath(XmlElement taggedFolder)
        {
            long pathcreated = long.Parse(taggedFolder.GetAttribute(ATTR_TAGGED_FOLDER_CREATEDDATE));
            long pathlastupdated = long.Parse(taggedFolder.GetAttribute(ATTR_TAGGED_FOLDER_LASTUPDATEDDATE));
            XmlElement path = (XmlElement)taggedFolder.GetElementsByTagName(ELE_TAGGED_FOLDER_PATH).Item(0);
            string pathname = path.InnerText;
            bool isDeleted = bool.Parse(taggedFolder.GetAttribute(ATTR_TAGGED_FOLDER_ISDELETED));
            long deletedDate = long.Parse(taggedFolder.GetAttribute(ATTR_TAGGED_FOLDER_DELETEDDATE));
            TaggedPath taggedPath = CreateTaggedPath(pathcreated, pathlastupdated, pathname, isDeleted, deletedDate);
            return taggedPath;
        }

        private static TaggedPath CreateTaggedPath(long pathcreated, long pathlastupdated, string pathname, bool isDeleted, long deletedDate)
        {
            TaggedPath taggedPath = new TaggedPath(pathname, pathcreated);
            taggedPath.LastUpdatedDate = pathlastupdated;
            taggedPath.LogicalDriveId = TaggingHelper.GetLogicalID(pathname);
            taggedPath.IsDeleted = isDeleted;
            taggedPath.DeletedDate = deletedDate;
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
            string type = filterNode.GetAttribute(ATTR_FILTER_TYPE);
            string mode = filterNode.GetAttribute(ATTR_FILTER_MODE);
            FilterMode fMode = FilterMode.INCLUDE;
            if (mode.Equals(VALUE_FILTER_MODE_EXCLUDE))
            {
                fMode = FilterMode.EXCLUDE;
            }
            else if (mode.Equals(VALUE_FILTER_MODE_INCLUDE))
            {
                fMode = FilterMode.INCLUDE;
            }
            else
            {
                //CANNOT HAPPEN!!!!!
            }
            if (type.Equals(VALUE_FILTER_TYPE_EXT))
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
            foreach (XmlElement configChild in configNode.ChildNodes)
            {
                if (configChild.Name.Equals(ELE_CONFIG_SEAMLESS))
                {
                    config.IsSeamless = bool.Parse(configChild.InnerText);
                }
            }
            return config;
        }
        #endregion

        #region create xml document
        private static XmlElement CreateTaggingProfileElement(XmlDocument xmldoc, TaggingProfile taggingProfile)
        {
            XmlElement profileElement = xmldoc.CreateElement(ELE_PROFILE_ROOT);
            profileElement.SetAttribute(ATTR_PROFILE_NAME, taggingProfile.ProfileName);
            profileElement.SetAttribute(ATTR_PROFILE_CREATEDDATE, taggingProfile.CreatedDate.ToString());
            profileElement.SetAttribute(ATTR_PROFILE_LASTUPDATEDDATE, taggingProfile.LastUpdatedDate.ToString());
            foreach (Tag tag in taggingProfile.ReadOnlyTagList)
            {
                profileElement.AppendChild(CreateTagElement(xmldoc, tag));
            }
            return profileElement;
        }

        private static XmlElement CreateTagElement(XmlDocument xmlDoc, Tag tag)
        {
            XmlElement tagElement = xmlDoc.CreateElement(ELE_TAG_ROOT);
            tagElement.SetAttribute(ATTR_TAG_NAME, tag.TagName);
            tagElement.SetAttribute(ATTR_TAG_CREATEDDATE, tag.CreatedDate.ToString());
            tagElement.SetAttribute(ATTR_TAG_LASTUPDATEDDATE, tag.LastUpdatedDate.ToString());
            tagElement.SetAttribute(ATTR_TAG_ISDELETED, tag.IsDeleted.ToString());
            tagElement.SetAttribute(ATTR_TAG_DELETEDDATE, tag.DeletedDate.ToString());
            tagElement.AppendChild(CreateFoldersElement(xmlDoc, tag));
            tagElement.AppendChild(CreateFilterElementList(xmlDoc, tag));
            tagElement.AppendChild(CreateConfigElement(xmlDoc, tag));
            return tagElement;
        }

        private static XmlElement CreateFoldersElement(XmlDocument xmlDoc, Tag tag)
        {
            List<TaggedPath> pathList = tag.UnfilteredPathList;
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
            XmlElement taggedFolderElement = xmlDoc.CreateElement(ELE_TAGGED_FOLDER_ROOT);
            taggedFolderElement.SetAttribute(ATTR_TAGGED_FOLDER_CREATEDDATE, path.CreatedDate.ToString());
            taggedFolderElement.SetAttribute(ATTR_TAGGED_FOLDER_LASTUPDATEDDATE, path.LastUpdatedDate.ToString());
            taggedFolderElement.SetAttribute(ATTR_TAGGED_FOLDER_ISDELETED, path.IsDeleted.ToString());
            taggedFolderElement.SetAttribute(ATTR_TAGGED_FOLDER_DELETEDDATE, path.DeletedDate.ToString());
            XmlElement folderPathElement = xmlDoc.CreateElement(ELE_TAGGED_FOLDER_PATH);
            folderPathElement.InnerText = path.PathName;
            taggedFolderElement.AppendChild(folderPathElement);
            return taggedFolderElement;
        }

        private static XmlElement CreateFilterElementList(XmlDocument xmlDoc, Tag tag)
        {
            List<Filter> filters = tag.Filters;
            XmlElement filterRoot = xmlDoc.CreateElement(ELE_FILTER_ROOT);
            filterRoot.SetAttribute(ATTR_FILTER_LASTUPDATEDDATE, tag.FiltersUpdatedDate.ToString());
            foreach (Filter f in filters)
            {
                XmlElement filterEle = CreateFilterElement(xmlDoc, f);
                filterRoot.AppendChild(filterEle);
            }
            return filterRoot;
        }

        private static XmlElement CreateFilterElement(XmlDocument xmlDoc, Filter filter)
        {
            XmlElement filterElement = xmlDoc.CreateElement(ELE_FILTER_CHILD_FILTER);
            string mode = filter.Mode == FilterMode.INCLUDE ? VALUE_FILTER_MODE_INCLUDE : VALUE_FILTER_MODE_EXCLUDE;
            filterElement.SetAttribute(ATTR_FILTER_MODE, mode);
            if (filter is ExtensionFilter)
            {
                string type = VALUE_FILTER_TYPE_EXT;
                filterElement.SetAttribute(ATTR_FILTER_TYPE, type);
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
            return configElement;
        }
        #endregion
    }
}
