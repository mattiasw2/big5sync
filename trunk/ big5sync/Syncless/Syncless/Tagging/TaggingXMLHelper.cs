using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Syncless.Filters;
using Syncless.Helper;

namespace Syncless.Tagging
{
    /// <summary>
    /// TaggingXMLHelper class provides XML-related operations to load and save tagging.xml file.
    /// </summary>
    internal static class TaggingXMLHelper
    {
        #region tagging root
        /// <summary>
        /// The string value that represents the tag name for tagging.xml root element
        /// </summary>
        private const string ELE_TAGGING_ROOT = "tagging";

        #region profile
        /// <summary>
        /// The string value that represents the tag name for profile root element
        /// </summary>
        private const string ELE_PROFILE_ROOT = "profile";
        #region profile attributes
        /// <summary>
        /// The string value that represents the attribute name for profile name attribute
        /// </summary>
        private const string ATTR_PROFILE_NAME = "name";

        /// <summary>
        /// The string value that represents the attribtue name for profile created date attribute
        /// </summary>
        private const string ATTR_PROFILE_CREATEDDATE = "createdDate";

        /// <summary>
        /// The string value that represents the attribute name for profile last updated date attribute
        /// </summary>
        private const string ATTR_PROFILE_LASTUPDATEDDATE = "lastUpdatedDate";
        #endregion

        #region profile child - tag list
        /// <summary>
        /// The string value that represents the tag name for tag root element
        /// </summary>
        private const string ELE_TAG_ROOT = "tag";
        #region tag attributes
        /// <summary>
        /// The string value that represents the attribute name for tag name attribute
        /// </summary>
        private const string ATTR_TAG_NAME = "name";

        /// <summary>
        /// The string value that represents the attribute name for tag created date attribute
        /// </summary>
        private const string ATTR_TAG_CREATEDDATE = "createdDate";

        /// <summary>
        /// The string value that represents the attribute name for tag last updated date attribute
        /// </summary>
        private const string ATTR_TAG_LASTUPDATEDDATE = "lastUpdatedDate";

        /// <summary>
        /// The string value that represents the attribute name for tag is deleted attribute
        /// </summary>
        private const string ATTR_TAG_ISDELETED = "isDeleted";

        /// <summary>
        /// The string value that represents the attribute name for tag deleted date attribute
        /// </summary>
        private const string ATTR_TAG_DELETEDDATE = "deletedDate";
        #endregion

        #region tag child - taggedfolder list
        /// <summary>
        /// The string value that represents the tag name for folders root element
        /// </summary>
        private const string ELE_FOLDER_ROOT = "folders";
        #region individual taggedfolder
        /// <summary>
        /// The string value that represents the tag name for tagged folder root element
        /// </summary>
        private const string ELE_TAGGED_FOLDER_ROOT = "taggedFolder";
        #region taggedfolder attributes
        /// <summary>
        /// The string value that represents the attribute name for tagged folder created date attribute
        /// </summary>
        private const string ATTR_TAGGED_FOLDER_CREATEDDATE = "createdDate";

        /// <summary>
        /// The string value that represents the attribute name for tagged folder last updated date attribute
        /// </summary>
        private const string ATTR_TAGGED_FOLDER_LASTUPDATEDDATE = "lastUpdatedDate";

        /// <summary>
        /// The string value that represents the attribute name for tagged folder is deleted date attribute
        /// </summary>
        private const string ATTR_TAGGED_FOLDER_ISDELETED = "isDeleted";

        /// <summary>
        /// The string value that represents the attribute name for tagged folder deleted date attribute
        /// </summary>
        private const string ATTR_TAGGED_FOLDER_DELETEDDATE = "deletedDate";
        #endregion
        #region taggedfolder child - path
        /// <summary>
        /// The string value that represents the tag name for tagged folder path element
        /// </summary>
        private const string ELE_TAGGED_FOLDER_PATH = "path";
        #endregion
        #endregion
        #endregion

        #region tag child - filter list
        /// <summary>
        /// The string value that represents the tag name for filter root element
        /// </summary>
        private const string ELE_FILTER_ROOT = "filters";
        #region individual filter
        /// <summary>
        /// The string value that represents the tag name for filter child element
        /// </summary>
        private const string ELE_FILTER_CHILD_FILTER = "filter";
        #region filter attributes
        /// <summary>
        /// The string value that represents the attribute name for filter last updated date attribute
        /// </summary>
        private const string ATTR_FILTER_LASTUPDATEDDATE = "lastUpdatedDate";
        #region filter type
        /// <summary>
        /// The string value that represents the attribute name for filter type attribute
        /// </summary>
        private const string ATTR_FILTER_TYPE = "type";
        
        /// <summary>
        /// The string value that represents the attribute value for filter type extension attribute
        /// </summary>
        private const string VALUE_FILTER_TYPE_EXT = "Extension";
        #endregion
        #region filter mode
        /// <summary>
        /// The string value that represents the attribute name for filter mode attribute
        /// </summary>
        private const string ATTR_FILTER_MODE = "mode";

        /// <summary>
        /// The string value that represents the attribute value for include filter mode attribute
        /// </summary>
        private const string VALUE_FILTER_MODE_INCLUDE = "Include";

        /// <summary>
        /// The string value that represents the attribute value for exclude filter mode attribute
        /// </summary>
        private const string VALUE_FILTER_MODE_EXCLUDE = "Exclude";
        #endregion
        #endregion
        #region filter child - pattern
        /// <summary>
        /// The string value that represents the tag name for filter type extension pattern element
        /// </summary>
        private const string ELE_FILTER_TYPE_EXT_PATTERN = "pattern";
        #endregion
        #endregion
        #endregion

        #region tag child - config
        /// <summary>
        /// The string value that represents the tag name for config root element
        /// </summary>
        private const string ELE_CONFIG_ROOT = "config";
        #region config child - seamless
        /// <summary>
        /// The string value that represents the tag name for config seamless element
        /// </summary>
        private const string ELE_CONFIG_SEAMLESS = "seamless";
        #endregion
        #endregion
        #endregion
        #endregion
        #endregion

        #region save operations
        /// <summary>
        /// Saves a tagging profile to a list of paths that is passed as paramater
        /// </summary>
        /// <param name="taggingProfile">The <see cref="TaggingProfile">TaggingProfile</see> object
        /// that represents the tagging profile to be saved</param>
        /// <param name="xmlFilePaths">The list of strings that represents the list of paths where
        /// the tagging profile is to be saved to</param>
        public static void SaveToLocations(TaggingProfile taggingProfile, List<string> xmlFilePaths)
        {
            foreach (string path in xmlFilePaths)
            {
                SaveTo(taggingProfile, path);
            }
        }

        /// <summary>
        /// Saves a tagging profile to a path that is passed as parameter
        /// </summary>
        /// <param name="taggingProfile">The <see cref="TaggingProfile">TaggingProfile</see> object
        /// that represents the tagging profile to be saved</param>
        /// <param name="xmlFilePath">The string value that represents the path where the tagging profile
        /// is to be saved to</param>
        public static void SaveTo(TaggingProfile taggingProfile, string xmlFilePath)
        {
            XmlDocument xmlDoc = ConvertTaggingProfileToXml(taggingProfile);
            CommonXmlHelper.SaveXml(xmlDoc, xmlFilePath);
        }

        /// <summary>
        /// Converts a tagging profile to a Xml document
        /// </summary>
        /// <param name="taggingProfile">The <see cref="TaggingProfile">TaggingProfile</see> object
        /// that represents the tagging profile to be converted</param>
        /// <returns>the Xml document that is created from the tagging profile that is passed as parameter
        /// </returns>
        private static XmlDocument ConvertTaggingProfileToXml(TaggingProfile taggingProfile)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement taggingElement = xmldoc.CreateElement(ELE_TAGGING_ROOT);
            XmlElement profileElement = CreateTaggingProfileElement(xmldoc, taggingProfile);
            taggingElement.AppendChild(profileElement);
            xmldoc.AppendChild(taggingElement);
            return xmldoc;
        }

        /// <summary>
        /// Appends a tagging profile to the tagging.xml saved in the list of locations passed as
        /// parameter
        /// </summary>
        /// <param name="profile">The <see cref="TaggingProfile">TaggingProfile</see> object that
        /// represents the tagging profile to be saved to the list of locations</param>
        /// <param name="locations">The list of locations containing tagging.xml where the profile
        /// that is passed as parameter is to be saved to</param>
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
        /// <summary>
        /// Loads a tagging profile from a path that is passed as parameter
        /// </summary>
        /// <param name="xmlFilePath">The string value that represents the path where the tagging profile
        /// is to be loaded from</param>
        /// <returns>the tagging profile that is loaded from the path that is passed as parameter if the
        /// xml document at the path exists; otherwise, a new tagging profile object</returns>
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
                return new TaggingProfile(TaggingHelper.GetCurrentTime());
            }
        }

        /// <summary>
        /// Converts a Xml document to a tagging profile
        /// </summary>
        /// <param name="xmlDoc">The XmlDocument object that represents the Xml document to be
        /// converted</param>
        /// <returns>the tagging profile that is created from the Xml document that is passed as
        /// parameter if it exists; otherwise, a newly created tagging profile</returns>
        private static TaggingProfile ConvertXmlToTaggingProfile(XmlDocument xmlDoc)
        {
            XmlNodeList profileElementList = xmlDoc.GetElementsByTagName(ELE_PROFILE_ROOT);
            if (profileElementList.Count == 0)
            {
                return new TaggingProfile(TaggingHelper.GetCurrentTime());
            }
            else
            {
                XmlElement profileElement = (XmlElement) profileElementList.Item(0);
                TaggingProfile taggingProfile = CreateTaggingProfile(profileElement);
                if (taggingProfile != null)
                {
                    XmlNodeList tagList = profileElement.ChildNodes;
                    foreach (XmlElement tagElement in tagList)
                    {
                        if (tagElement.Name.Equals(ELE_TAG_ROOT))
                        {
                            Tag tag = CreateTagFromXml(tagElement);
                            if (tag != null)
                            {
                                lock (taggingProfile.TagList)
                                {
                                    taggingProfile.TagList.Add(tag);
                                }
                            }
                            else
                            {
                                return new TaggingProfile(TaggingHelper.GetCurrentTime());
                            }
                        }
                    }
                    return taggingProfile; 
                }
                else
                {
                    return new TaggingProfile(TaggingHelper.GetCurrentTime());
                }
            }
        }
        #endregion

        #region create tagging profile
        /// <summary>
        /// Creates a tagging profile from the Xml element that is passed as parameter
        /// </summary>
        /// <param name="profileElement">The XmlElement object that represents the Xml element that is to
        /// be used to create the tagging profile</param>
        /// <returns>the tagging profile that is created if there is no FormatException thrown; 
        /// otherwise, null</returns>
        private static TaggingProfile CreateTaggingProfile(XmlElement profileElement)
        {
            try
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
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a tag from the Xml element that is passed as parameter
        /// </summary>
        /// <param name="tagElement">The XmlElement object that represents the Xml element that is to
        /// be used to create the tag</param>
        /// <returns>the tag that is created if there is no FormatException thrown or the attributes 
        /// of tag are not null; otherwise, null</returns>
        private static Tag CreateTagFromXml(XmlElement tagElement)
        {
            try
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
                        if (tag.FilteredPathList == null)
                        {
                            return null;
                        }
                    }
                    else if (tagChild.Name.Equals(ELE_FILTER_ROOT))
                    {
                        tag.Filters = LoadFilterList(tagChild);
                        if (tag.Filters == null)
                        {
                            return null;
                        }
                        else
                        {
                            try
                            {
                                tag.FiltersUpdatedDate = long.Parse(tagChild.GetAttribute(ATTR_FILTER_LASTUPDATEDDATE));
                            }
                            catch (FormatException)
                            {
                                return null;
                            }
                        }
                    }
                    else if (tagChild.Name.Equals(ELE_CONFIG_ROOT))
                    {
                        tag.Config = CreateTagConfig(tagChild);
                        if (tag.Config == null)
                        {
                            return null;
                        }
                    }
                }
                return tag;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a list of tagged paths from the Xml element that is passed as parameter
        /// </summary>
        /// <param name="foldersElement">The XmlElement object that represents the Xml element that is to
        /// be used to create the list of tagged paths</param>
        /// <returns>the list of tagged paths if the no tagged path is null; otherwise, null</returns>
        private static List<TaggedPath> CreateFolders(XmlElement foldersElement)
        {
            List<TaggedPath> pathList = new List<TaggedPath>();
            foreach (XmlElement taggedFolder in foldersElement.GetElementsByTagName(ELE_TAGGED_FOLDER_ROOT))
            {
                TaggedPath taggedPath = CreatePath(taggedFolder);
                if (taggedPath != null)
                {
                    pathList.Add(taggedPath);
                }
                else
                {
                    return null;
                }
            }
            return pathList;
        }

        /// <summary>
        /// Creates a tagged path from the Xml element that is passed as parameter
        /// </summary>
        /// <param name="taggedFolder">The XmlElement object that represents the Xml element that is to
        /// be used to create the tagged path</param>
        /// <returns>the tagged path that is created if no FormatException thrown or the path string value exists;
        /// otherwise, null</returns>
        private static TaggedPath CreatePath(XmlElement taggedFolder)
        {
            try
            {
                long pathcreated = long.Parse(taggedFolder.GetAttribute(ATTR_TAGGED_FOLDER_CREATEDDATE));
                long pathlastupdated = long.Parse(taggedFolder.GetAttribute(ATTR_TAGGED_FOLDER_LASTUPDATEDDATE));
                XmlNodeList pathList = taggedFolder.GetElementsByTagName(ELE_TAGGED_FOLDER_PATH);
                if (pathList.Count == 0)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        XmlElement path = (XmlElement)pathList.Item(0);
                        string pathname = path.InnerText;
                        bool isDeleted = bool.Parse(taggedFolder.GetAttribute(ATTR_TAGGED_FOLDER_ISDELETED));
                        long deletedDate = long.Parse(taggedFolder.GetAttribute(ATTR_TAGGED_FOLDER_DELETEDDATE));
                        TaggedPath taggedPath = CreateTaggedPath(pathcreated, pathlastupdated, pathname, isDeleted, deletedDate);
                        return taggedPath;
                    }
                    catch (FormatException)
                    {
                        return null;
                    }
                }
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a tagged path from the created date, last updated date, path name, is deleted and deleted
        /// date attributes that are passed as parameters
        /// </summary>
        /// <param name="pathcreated">The long value that represents the created date of the tagged path</param>
        /// <param name="pathlastupdated">The long value that represents the last updated date of 
        /// the tagged path</param>
        /// <param name="pathname">The string value that represents the full path name of the 
        /// tagged path</param>
        /// <param name="isDeleted">The boolean value that represents whether the tagged path is deleted</param>
        /// <param name="deletedDate">The long value that represents the deleted date of the tagged path</param>
        /// <returns>the tagged path that is created</returns>
        private static TaggedPath CreateTaggedPath(long pathcreated, long pathlastupdated, string pathname, bool isDeleted, long deletedDate)
        {
            TaggedPath taggedPath = new TaggedPath(pathname, pathcreated);
            taggedPath.LastUpdatedDate = pathlastupdated;
            taggedPath.LogicalDriveId = TaggingHelper.GetLogicalID(pathname);
            taggedPath.IsDeleted = isDeleted;
            taggedPath.DeletedDate = deletedDate;
            return taggedPath;
        }

        /// <summary>
        /// Creates a list of filters from the Xml element that is passed as parameter
        /// </summary>
        /// <param name="filterListNode">The XmlElement object that represents the Xml element that is to
        /// be used to create the list of filters</param>
        /// <returns>the list of filters if no filter is null; otherwise, null</returns>
        private static List<Filter> LoadFilterList(XmlElement filterListNode)
        {
            List<Filter> filterList = new List<Filter>();
            XmlNodeList elementNodes = filterListNode.GetElementsByTagName(ELE_FILTER_CHILD_FILTER);
            if (elementNodes.Count == 0)
            {
                return null;
            }
            else
            {
                foreach (XmlNode n in elementNodes)
                {
                    Filter f = LoadFilter((XmlElement)n);
                    if (f != null)
                    {
                        filterList.Add(f);
                    }
                    else
                    {
                        return null;
                    }
                }
                return filterList;
            }
        }

        /// <summary>
        /// Creates a filter from the Xml element that is passed as parameter
        /// </summary>
        /// <param name="filterNode">The XmlElement object that represents the Xml element that is to
        /// be used to create the filter</param>
        /// <returns>the filter that is created if no attribute value is empty; otherwise, null</returns>
        private static Filter LoadFilter(XmlElement filterNode)
        {
            string type = filterNode.GetAttribute(ATTR_FILTER_TYPE);
            if (type.Equals(string.Empty))
            {
                return null;
            }
            string mode = filterNode.GetAttribute(ATTR_FILTER_MODE);
            if (mode.Equals(string.Empty))
            {
                return null;
            }
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
                return null;
            }
            if (type.Equals(VALUE_FILTER_TYPE_EXT))
            {
                XmlNodeList list = filterNode.GetElementsByTagName(ELE_FILTER_TYPE_EXT_PATTERN);
                if (list.Count == 0)
                {
                    return null;
                }
                else
                {
                    XmlElement ext = (XmlElement)list[0];
                    if (ext.InnerText.Equals(string.Empty))
                    {
                        return null;
                    }
                    else
                    {
                        ExtensionFilter filter = new ExtensionFilter(ext.InnerText, fMode);
                        return filter;  
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a tag config from the Xml element that is passed as parameter
        /// </summary>
        /// <param name="configNode">The XmlElement object that represents the Xml element that is to
        /// be used to create the tag config</param>
        /// <returns>the tag config that is created if no FormatException thrown; otherwise, null</returns>
        private static TagConfig CreateTagConfig(XmlElement configNode)
        {
            TagConfig config = new TagConfig();
            foreach (XmlElement configChild in configNode.ChildNodes)
            {
                if (configChild.Name.Equals(ELE_CONFIG_SEAMLESS))
                {
                    try
                    {
                        config.IsSeamless = bool.Parse(configChild.InnerText);
                    }
                    catch (FormatException)
                    {
                        return null;
                    }
                }
            }
            return config;
        }
        #endregion

        #region create xml document
        /// <summary>
        /// Creates a tagging profile Xml element from the tagging profile that is passed as parameter
        /// </summary>
        /// <param name="xmldoc">The Xmldocument object that represents the Xml document that the
        /// Xml element to be created belongs to</param>
        /// <param name="taggingProfile">The <see cref="TaggingProfile">TaggingProfile</see> object that
        /// represents the tagging profile to be used to create the Xml element</param>
        /// <returns>the tagging profile Xml element that is created</returns>
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

        /// <summary>
        /// Creates a tag Xml element from the tag that is passed as parameter
        /// </summary>
        /// <param name="xmlDoc">The Xmldocument object that represents the Xml document that the
        /// Xml element to be created belongs to</param>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that
        /// represents the tag to be used to create the Xml element</param>
        /// <returns>the tag Xml element that is created</returns>
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

        /// <summary>
        /// Creates a folder list Xml element from the tag that is passed as parameter
        /// </summary>
        /// <param name="xmlDoc">The Xmldocument object that represents the Xml document that the
        /// Xml element to be created belongs to</param>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that
        /// represents the tag to be used to create the Xml element</param>
        /// <returns>the folder list Xml element that is created</returns>
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

        /// <summary>
        /// Creates a tagged path Xml element from the tagged path that is passed as parameter
        /// </summary>
        /// <param name="xmlDoc">The Xmldocument object that represents the Xml document that the
        /// Xml element to be created belongs to</param>
        /// <param name="path">The <see cref="TaggedPath">TaggedPath</see> object that
        /// represents the tagged path to be used to create the Xml element</param>
        /// <returns>the tagged path Xml element that is created</returns>
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

        /// <summary>
        /// Creates a filter list Xml element from the tag that is passed as parameter
        /// </summary>
        /// <param name="xmlDoc">The Xmldocument object that represents the Xml document that the
        /// Xml element to be created belongs to</param>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that
        /// represents the tag to be used to create the Xml element</param>
        /// <returns>the filter list Xml element that is created</returns>
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

        /// <summary>
        /// Creates a filter Xml element from the filter that is passed as parameter
        /// </summary>
        /// <param name="xmlDoc">The Xmldocument object that represents the Xml document that the
        /// Xml element to be created belongs to</param>
        /// <param name="filter">The <see cref="Filter">Filter</see> object that
        /// represents the filter to be used to create the Xml element</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a tag config Xml element from the tag that is passed as parameter
        /// </summary>
        /// <param name="xmlDoc">The Xmldocument object that represents the Xml document that the
        /// Xml element to be created belongs to</param>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that
        /// represents the tag to be used to create the Xml element</param>
        /// <returns>the tag config Xml element that is created</returns>
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
