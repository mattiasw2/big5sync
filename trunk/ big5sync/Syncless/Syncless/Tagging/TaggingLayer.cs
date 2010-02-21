using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Syncless.Helper;
using Syncless.Tagging.Exceptions;
using System.Diagnostics;

namespace Syncless.Tagging
{
    public class TaggingLayer
    {
        public const string RELATIVE_TAGGING_ROOT_SAVE_PATH = "tagging.xml";
        #region attributes
        private static TaggingLayer _instance;

        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static TaggingLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TaggingLayer();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Contains a list of FolderTag objects
        /// </summary>
        public List<FolderTag> FolderTagList
        {
            get { return _taggingProfile.FolderTagList; }
        }

        /// <summary>
        /// Contains a list of FileTag object
        /// </summary>
        public List<FileTag> FileTagList
        {
            get { return _taggingProfile.FileTagList; }
        }

        /// <summary>
        /// Contains a list of all Tag objects
        /// </summary>
        public List<Tag> AllTagList
        {
            get
            {
                List<Tag> allTagList = new List<Tag>();
                foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
                {
                    allTagList.Add(folderTag);
                }
                foreach (FileTag fileTag in _taggingProfile.FileTagList)
                {
                    allTagList.Add(fileTag);
                }
                return allTagList;
            }
        }

        private TaggingProfile _taggingProfile;

        /// <summary>
        /// Contains information about the current profile name and the tag lists
        /// </summary>
        public TaggingProfile TaggingProfile
        {
            get { return _taggingProfile; }
            set { _taggingProfile = value; }
        }
        #endregion

        private TaggingLayer()
        {
            _taggingProfile = new TaggingProfile();
            _taggingProfile.Created = TaggingHelper.GetCurrentTime();
        }

        /// <summary>
        /// Initialize _taggingProfile object. If a tagging.xml file has already been created, load the information
        /// from the file, else, instantiate a new _taggingProfile object.
        /// </summary>
        /// <param name="profileFilePath">The path of the tagging.xml file to be loaded.</param>
        public void Init(string profileFilePath)
        {
            if (!File.Exists(profileFilePath))
            {
                _taggingProfile = new TaggingProfile();
            }
            else
            {
                Debug.Assert(LoadFrom(profileFilePath) != null);
                _taggingProfile = LoadFrom(profileFilePath);
            }
        }

        #region FolderTag public implementations
        /// <summary>
        /// Create a Folder Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <returns>The created Tag, else raise TagAlreadyExistsException</returns>
        public FolderTag CreateFolderTag(string tagname)
        {
            if (!CheckFolderTagExists(tagname) && !CheckFileTagExists(tagname))
            {
                long created = TaggingHelper.GetCurrentTime();
                FolderTag tag = new FolderTag(tagname, created);
                _taggingProfile.FolderTagList.Add(tag);
                UpdateTaggingProfileDate(created);
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException(ErrorMessage.TAG_ALREADY_EXISTS_EXCEPTION, tagname);
            }
        }

        /// <summary>
        /// Rename a Folder Tag of oldname to newname
        /// </summary>
        /// <param name="oldname">The original name of the Tag to be renamed</param>
        /// <param name="newname">The new name to be given to the Tag</param>
        /// <returns>If the oldname does not exist, raise TagNotFoundException, if newname is already used
        /// for another Tag, raise TagAlreadyExistsException</returns>
        public void RenameFolderTag(string oldname, string newname)
        {
            if (CheckFolderTagExists(oldname))
            {
                if (!CheckFolderTagExists(newname) && !CheckFileTagExists(newname))
                {
                    Debug.Assert(GetFolderTag(oldname) != null);
                    GetFolderTag(oldname).TagName = newname;
                }
                else
                {
                    throw new TagAlreadyExistsException(ErrorMessage.TAG_ALREADY_EXISTS_EXCEPTION, newname);
                }
            }
            else
            {
                throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, oldname);
            }
        }

        /// <summary>
        /// Remove the Folder Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be removed</param>
        /// <returns>The Tag that is removed successfully, else raise TagNotFoundException</returns>
        public FolderTag RemoveFolderTag(string tagname)
        {
            FolderTag toRemove;
            if (CheckFolderTagExists(tagname))
            {
                long updated = TaggingHelper.GetCurrentTime();
                toRemove = GetFolderTag(tagname);
                _taggingProfile.FolderTagList.Remove(toRemove);
                UpdateTaggingProfileDate(updated);
                return toRemove;
            }
            else
            {
                throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, tagname);
            }
        }

        /// <summary>
        /// Tag a folder with a tagname
        /// </summary>
        /// <param name="path">The path of the folder to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FolderTag that contains the path, if path already exists raise PathAlreadyExistsException
        /// if the given tagname belongs to a File Tag raise TagTypeConflictException, if the given path has
        /// sub-directory or parent directory already tagged raise RecursiveDirectoryException</returns>
        public FolderTag TagFolder(string path, string tagname)
        {
            long lastupdated = TaggingHelper.GetCurrentTime();
            Tag tag = FindTag(tagname);
            if (tag == null)
            {
                tag = new FolderTag(tagname, lastupdated);
            }
            Debug.Assert(tag != null);
            if (tag is FileTag)
            {
                throw new TagTypeConflictException(ErrorMessage.TAG_TYPE_CONFLICT_EXCEPTION, path, tagname, false);
            }
            else
            {
                Debug.Assert(tag is FolderTag);
                if (!tag.Contain(path))
                {
                    if (!TaggingHelper.CheckRecursiveDirectory((FolderTag)tag, path))
                    {
                        tag.AddPath(path, lastupdated);
                        AddFolderTag((FolderTag)tag);
                        UpdateTaggingProfileDate(lastupdated);
                        return (FolderTag)tag;
                    }
                    else
                    {
                        throw new RecursiveDirectoryException(ErrorMessage.RECURSIVE_DIRECTORY_EXCEPTION, path, tagname);
                    }
                }
                else
                {
                    throw new PathAlreadyExistsException(ErrorMessage.PATH_ALREADY_EXISTS_EXCEPTION, path);
                }
            }
        }

        /// <summary>
        /// Untag a Folder from a tagname
        /// </summary>
        /// <param name="path">The path to untag</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>1 if the path is removed, 0 if the path is not found in the Tag, else raise 
        /// TagNotFoundException</returns>
        public int UntagFolder(string path, string tagname)
        {
            long lastupdated = TaggingHelper.GetCurrentTime();
            FolderTag tag = RetrieveFolderTag(tagname);
            if (tag != null)
            {
                if (tag.RemovePath(path, lastupdated))
                {
                    UpdateTaggingProfileDate(lastupdated);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, tagname);
            }
        }

        /// <summary>
        /// Rename a path in all the FolderTags it is tagged to
        /// </summary>
        /// <param name="oldPath">The original path of the folder</param>
        /// <param name="newPath">The new path of the folder</param>
        public void RenameFolder(string oldPath, string newPath)
        {
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                folderTag.Rename(oldPath, newPath);
            }
        }

        /// <summary>
        /// Retrieve the FolderTag with the particular tag name
        /// </summary>
        /// <param name="tagname">The name of the FolderTag</param>
        /// <returns>The FolderTag that is to be found, else null</returns>
        public FolderTag RetrieveFolderTag(string tagname)
        {
            return RetrieveFolderTag(tagname, false, 0);
        }

        /// <summary>
        /// Retrieve a list of FolderTags where a given path is tagged to
        /// </summary>
        /// <param name="path">The path to find the FolderTags it is tagged to</param>
        /// <returns>The list of FolderTags containing the given path</returns>
        public List<FolderTag> RetrieveFolderTagByPath(string path)
        {
            List<FolderTag> folderTagList = new List<FolderTag>();
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                if (folderTag.Contain(path))
                {
                    folderTagList.Add(folderTag);
                }
            }
            return folderTagList;
        }

        /// <summary>
        /// Find a list of paths of folders or sub-folders which share the same FolderTag as folderPath
        /// </summary>
        /// <param name="folderPath">The path to search</param>
        /// <returns>The list of similar paths</returns>
        public List<string> FindSimilarPathForFolder(string folderPath)
        {
            string logicalid = TaggingHelper.GetLogicalID(folderPath);
            List<string> folderPathList = new List<string>();
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                if (folderTag.Contain(folderPath))
                {
                    foreach (TaggedPath p in folderTag.PathList)
                    {
                        if (!folderPathList.Contains(p.Path) && !p.Path.Equals(folderPath))
                        {
                            folderPathList.Add(p.Path);
                        }
                    }
                }
            }
            List<FolderTag> matchingFolderTag = RetrieveFolderTagById(logicalid);
            foreach (FolderTag folderTag in matchingFolderTag)
            {
                string appendedPath;
                string trailingPath = folderTag.FindMatchedParentDirectory(folderPath, true);
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in folderTag.PathList)
                    {
                        appendedPath = p.Append(trailingPath);
                        if (!folderPathList.Contains(appendedPath) && !appendedPath.Equals(folderPath))
                        {
                            folderPathList.Add(appendedPath);
                        }
                    }
                }
            }
            return folderPathList;
        }
        #endregion

        #region FileTag public implementations
        /// <summary>
        /// Create a File Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <returns>The created Tag, else raise TagAlreadyExistsException</returns>
        public FileTag CreateFileTag(string tagname)
        {
            if (!CheckFileTagExists(tagname) && !CheckFolderTagExists(tagname))
            {
                long created = TaggingHelper.GetCurrentTime();
                FileTag tag = new FileTag(tagname, created);
                _taggingProfile.FileTagList.Add(tag);
                UpdateTaggingProfileDate(created);
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException(ErrorMessage.TAG_ALREADY_EXISTS_EXCEPTION, tagname);
            }
        }

        /// <summary>
        /// Rename a File Tag of oldname to newname
        /// </summary>
        /// <param name="oldname">The original name of the Tag to be renamed</param>
        /// <param name="newname">The new name to be given to the Tag</param>
        /// <returns>If the oldname does not exist, raise TagNotFoundException, if newname is already used
        /// for another Tag, raise TagAlreadyExistsException</returns>
        public void RenameFileTag(string oldname, string newname)
        {
            if (CheckFileTagExists(oldname))
            {
                if (!CheckFileTagExists(newname) && !CheckFolderTagExists(newname))
                {
                    Debug.Assert(GetFileTag(oldname) != null);
                    GetFileTag(oldname).TagName = newname;
                }
                else
                {
                    throw new TagAlreadyExistsException(ErrorMessage.TAG_ALREADY_EXISTS_EXCEPTION, newname);
                }
            }
            else
            {
                throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, oldname);
            }
        }

        /// <summary>
        /// Remove the File Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be removed</param>
        /// <returns>The Tag that is removed successfully, else raise TagNotFoundException</returns>
        public FileTag RemoveFileTag(string tagname)
        {
            FileTag toRemove;
            if (CheckFileTagExists(tagname))
            {
                long updated = TaggingHelper.GetCurrentTime();
                toRemove = GetFileTag(tagname);
                _taggingProfile.FileTagList.Remove(toRemove);
                UpdateTaggingProfileDate(updated);
                return toRemove;
            }
            else
            {
                throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, tagname);
            }
        }

        /// <summary>
        /// Tag a file with a tagname
        /// </summary>
        /// <param name="path">The path of the file to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FileTag that contains the path, if path already exists raise PathAlreadyExistsException
        /// if the given tagname belongs to a Folder Tag raise TagTypeConflictException</returns>
        public FileTag TagFile(string path, string tagname)
        {
            long lastupdated = TaggingHelper.GetCurrentTime();
            Tag tag = FindTag(tagname);
            if (tag == null)
            {
                tag = new FileTag(tagname, lastupdated);
            }
            Debug.Assert(tag != null);
            if (tag is FolderTag)
            {
                throw new TagTypeConflictException(ErrorMessage.TAG_TYPE_CONFLICT_EXCEPTION, path, tagname, true);
            }
            else
            {
                Debug.Assert(tag is FileTag);
                if (!tag.Contain(path))
                {
                    tag.AddPath(path, lastupdated);
                    AddFileTag((FileTag)tag);
                    UpdateTaggingProfileDate(lastupdated);
                    return (FileTag)tag;
                }
                else
                {
                    throw new PathAlreadyExistsException(ErrorMessage.PATH_ALREADY_EXISTS_EXCEPTION, path);
                }
            }
        }

        /// <summary>
        /// Untag a File from a tagname
        /// </summary>
        /// <param name="path">The path to untag</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>1 if the path is removed, 0 if the path is not found in the Tag, else raise 
        /// TagNotFoundException</returns>
        public int UntagFile(string path, string tagname)
        {
            long lastupdated = TaggingHelper.GetCurrentTime();
            FileTag tag = RetrieveFileTag(tagname);
            if (tag != null)
            {
                if (tag.RemovePath(path, lastupdated))
                {
                    UpdateTaggingProfileDate(lastupdated);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, tagname);
            }
        }

        /// <summary>
        /// Rename a path in all the FileTags it is tagged to
        /// </summary>
        /// <param name="oldPath">The original path of the file</param>
        /// <param name="newPath">The new path of the file</param>
        public void RenameFile(string oldPath, string newPath)
        {
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                fileTag.Rename(oldPath, newPath);
            }
        }

        /// <summary>
        /// Retrieve the FileTag with the particular tag name
        /// </summary>
        /// <param name="tagname">The name of the FileTag</param>
        /// <returns>The FileTag that is to be found, else null</returns>
        public FileTag RetrieveFileTag(string tagname)
        {
            return RetrieveFileTag(tagname, false, 0);
        }

        /// <summary>
        /// Retrieve a list of FileTags where a given path is tagged to
        /// </summary>
        /// <param name="path">The path to find the FileTags it is tagged to</param>
        /// <returns>The list of FileTags containing the given path</returns>
        public List<FileTag> RetrieveFileTagByPath(string path)
        {
            List<FileTag> fileTagList = new List<FileTag>();
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                if (fileTag.Contain(path))
                {
                    fileTagList.Add(fileTag);
                }
            }
            return fileTagList;
        }

        /// <summary>
        /// Find a list of paths of files which share the same FileTag or parent directories as filePath
        /// </summary>
        /// <param name="filePath">The path to search</param>
        /// <returns>The list of similar paths</returns>
        public List<string> FindSimilarPathForFile(string filePath)
        {
            string logicalid = TaggingHelper.GetLogicalID(filePath);
            List<string> filePathList = new List<string>();
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                if (fileTag.Contain(filePath))
                {
                    foreach (TaggedPath p in fileTag.PathList)
                    {
                        if (!filePathList.Contains(p.Path) && !p.Path.Equals(filePath))
                        {
                            filePathList.Add(p.Path);
                        }
                    }
                }
            }
            List<FolderTag> matchingFolderTag = RetrieveFolderTagById(logicalid);
            foreach (FolderTag folderTag in matchingFolderTag)
            {
                string appendedPath;
                string trailingPath = folderTag.FindMatchedParentDirectory(filePath, false);
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in folderTag.PathList)
                    {
                        appendedPath = p.Append(trailingPath);
                        if (!filePathList.Contains(appendedPath) && !appendedPath.Equals(filePath))
                        {
                            filePathList.Add(appendedPath);
                        }
                    }
                }
            }
            return filePathList;
        }
        #endregion

        #region miscellaneous public implementations
        #region completed
        /// <summary>
        /// Retrieve information of a profile given the xml data
        /// </summary>
        /// <param name="xmlFilePath">The path of the tagging.xml file to be lodaed</param>
        /// <returns>The tagging profile that is loaded from the given xml data, else null</returns>
        public TaggingProfile LoadFrom(string xmlFilePath)
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

        /// <summary>
        /// Save information of a profile to a xml file
        /// </summary>
        /// <param name="xmlFilePath">The path of the tagging.xml file to be saved to</param>
        /// <returns>True if the save is successful, else false</returns>
        public bool SaveTo(string xmlFilePath)
        {
            XmlDocument xml = ConvertTaggingProfileToXml(_taggingProfile);
            return TaggingXMLHelper.SaveXml(xml, "tagging.xml");
        }

        /// <summary>
        /// Retrieve the Tag by tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be retrieved</param>
        /// <returns>The Tag if it exists, else null</returns>
        public Tag RetrieveTag(string tagname)
        {
            Tag tag = GetFolderTag(tagname);
            if (tag == null)
            {
                tag = GetFileTag(tagname);
            }
            return tag;
        }

        /// <summary>
        /// Remove the Tag by tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be removed</param>
        /// <returns>The Tag that is removed, else raise TagNotFoundException</returns>
        public Tag RemoveTag(string tagname)
        {
            Tag toRemove = null;
            toRemove = GetFolderTag(tagname);
            if (toRemove != null)
            {
                _taggingProfile.FolderTagList.Remove((FolderTag)toRemove);
                return toRemove;
            }
            toRemove = GetFileTag(tagname);
            if (toRemove !=null)
            {
                _taggingProfile.FileTagList.Remove((FileTag)toRemove);
                return toRemove;
            }
            throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, tagname);
        }

        /// <summary>
        /// Untag a path from the Tag by tagname
        /// </summary>
        /// <param name="path">The name of the path to be untagged</param>
        /// <param name="tagname">The name of the Tag that the path is tagged to</param>
        /// <returns>1 if path is untagged successfully, 0 if the path does not exist, else raise 
        /// TagNotFoundException</returns>
        public int Untag(string path, string tagname)
        {
            Tag tag;
            long lastupdated = TaggingHelper.GetCurrentTime();
            tag = GetFolderTag(tagname);
            if (tag != null)
            {
                if (tag.Contain(path))
                {
                    tag.RemovePath(path, lastupdated);
                    UpdateTaggingProfileDate(lastupdated);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            tag = GetFileTag(tagname);
            if (tag != null)
            {
                if (tag.Contain(path))
                {
                    tag.RemovePath(path, lastupdated);
                    UpdateTaggingProfileDate(lastupdated);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, tagname);
        }

        /// <summary>
        /// Untag the path in all the Tags it is tagged to
        /// </summary>
        /// <param name="path">The name of the path to be untagged</param>
        /// <returns>The number of Tags the path is untagged from</returns>
        public int Untag(string path)
        {
            int noOfPath = 0;
            long lastupdated = 0;
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                if (folderTag.Contain(path))
                {
                    lastupdated = TaggingHelper.GetCurrentTime();
                    folderTag.RemovePath(path, lastupdated);
                    UpdateTaggingProfileDate(lastupdated);
                    noOfPath++;
                }
            }
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                if (fileTag.Contain(path))
                {
                    lastupdated = TaggingHelper.GetCurrentTime();
                    fileTag.RemovePath(path, lastupdated);
                    UpdateTaggingProfileDate(lastupdated);
                    noOfPath++;
                }
            }
            return noOfPath;
        }

        /// <summary>
        /// Retrieve all the tags that have path in a logical drive having logicalid.
        /// </summary>
        /// <param name="logicalId">The Logical Id</param>
        /// <returns>The list of Tags</returns>
        public List<Tag> RetrieveTagByLogicalId(string logicalid)
        {
            bool found;
            List<Tag> tagList = new List<Tag>();
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                found = CheckFolderID(folderTag, logicalid);
                if (found)
                {
                    tagList.Add(folderTag);
                }
            }
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                found = CheckFileID(fileTag, logicalid);
                if (found)
                {
                    tagList.Add(fileTag);
                }
            }
            return tagList;
        }

        /// <summary>
        /// Retrieve all paths having logicalid
        /// </summary>
        /// <param name="logicalid">The logical ID</param>
        /// <returns>The list of paths having logicalid</returns>
        public List<string> RetrievePathByLogicalId(string logicalid)
        {
            List<string> pathList = new List<string>();
            List<Tag> tagList = RetrieveTagByLogicalId(logicalid);
            foreach (Tag tag in tagList)
            {
                foreach (TaggedPath path in tag.PathList)
                {
                    if (path.LogicalDriveId.Equals(logicalid))
                    {
                        if (!pathList.Contains(path.Path))
                        {
                            pathList.Add(path.Path);
                        }
                    }
                }
            }
            return pathList;
        }

        /// <summary>
        /// Retrieve the list of parent directories of a given path
        /// </summary>
        /// <param name="path">The path of which the parent directories to be found</param>
        /// <returns>The list of parent directories</returns>
        public List<string> RetrieveParentByPath(string path)
        {
            List<string> parentPathList = new List<string>();
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                foreach (TaggedPath p in folderTag.PathList)
                {
                    if (path.StartsWith(p.Path))
                    {
                        if (!path.Equals(p.Path))
                        {
                            if (!parentPathList.Contains(p.Path))
                            {
                                parentPathList.Add(p.Path);
                                break;
                            }
                        }
                    }
                }
            }
            return parentPathList;
        }
        #endregion
        #endregion

        #region private methods implementations
        #region completed
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

        private void UpdateTaggingProfileDate(long created)
        {
            _taggingProfile.LastUpdated = created;
        }

        private Tag FindTag(string tagname)
        {
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                if (folderTag.TagName.Equals(tagname))
                {
                    return folderTag;
                }
            }
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                if (fileTag.TagName.Equals(tagname))
                {
                    return fileTag;
                }
            }
            return null;
        }

        private FolderTag RetrieveFolderTag(string tagname, bool create, long lastupdated)
        {
            FolderTag tag = GetFolderTag(tagname);
            if (tag == null)
            {
                if (create)
                {
                    tag = new FolderTag(tagname, lastupdated);
                }
            }
            return tag;
        }

        private FileTag RetrieveFileTag(string tagname, bool create, long lastupdated)
        {
            FileTag tag = GetFileTag(tagname);
            if (tag == null)
            {
                if (create)
                {
                    tag = new FileTag(tagname, lastupdated);
                }
            }
            return tag;
        }

        private void AddFolderTag(FolderTag tag)
        {
            if (!CheckFolderTagExists(tag.TagName))
            {
                _taggingProfile.FolderTagList.Add(tag);
            }
        }

        private void AddFileTag(FileTag tag)
        {
            if (!CheckFileTagExists(tag.TagName))
            {
                _taggingProfile.FileTagList.Add(tag);
            }
        }

        private FolderTag GetFolderTag(string tagname)
        {
            if (CheckFolderTagExists(tagname))
            {
                foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
                {
                    if (folderTag.TagName.Equals(tagname))
                    {
                        return folderTag;
                    }
                }
            }
            return null;
        }

        private FileTag GetFileTag(string tagname)
        {
            if (CheckFileTagExists(tagname))
            {
                foreach (FileTag fileTag in _taggingProfile.FileTagList)
                {
                    if (fileTag.TagName.Equals(tagname))
                    {
                        return fileTag;
                    }
                }
            }
            return null;
        }

        private bool CheckFolderTagExists(string tagname)
        {
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                if (folderTag.TagName.Equals(tagname))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckFileTagExists(string tagname)
        {
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                if (fileTag.TagName.Equals(tagname))
                {
                    return true;
                }
            }
            return false;
        }

        private List<FolderTag> RetrieveFolderTagById(string logicalid)
        {
            bool found;
            List<FolderTag> tagList = new List<FolderTag>();
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                found = CheckFolderID(folderTag, logicalid);
                if (found)
                {
                    tagList.Add(folderTag);
                }
            }
            return tagList;
        }

        private bool CheckFolderID(FolderTag tag, string ID)
        {
            foreach (TaggedPath path in tag.PathList)
            {
                if (path.LogicalDriveId.Equals(ID))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckFileID(FileTag tag, string ID)
        {
            foreach (TaggedPath path in tag.PathList)
            {
                if (path.LogicalDriveId.Equals(ID))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        #endregion
    }
}
