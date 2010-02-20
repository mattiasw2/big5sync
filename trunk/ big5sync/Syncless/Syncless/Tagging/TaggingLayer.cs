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

        public List<FolderTag> FolderTagList
        {
            get { return _taggingProfile.FolderTagList; }
        }

        public List<FileTag> FileTagList
        {
            get { return _taggingProfile.FileTagList; }
        }

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

        public TaggingProfile TaggingProfile
        {
            get { return _taggingProfile; }
            set { _taggingProfile = value; }
        }
        #endregion

        private TaggingLayer()
        {
            _taggingProfile = new TaggingProfile();
            _taggingProfile.Created = DateTime.Now.Ticks;
        }
        public void Init(string profileFilePath)
        {
            if (!File.Exists(profileFilePath))
            {
                _taggingProfile = new TaggingProfile();
            }
            else
            {
                _taggingProfile = LoadTaggingProfile(profileFilePath);
            }
        }

        #region FolderTag public implementations
        /// <summary>
        /// Create a Folder Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <param name="lastupdated">The last updated time of the Tag to be created</param>
        /// <returns>The created Tag, else raise TagAlreadyExistsException</returns>
        public FolderTag CreateFolderTag(string tagname)
        {
            if (!CheckFolderTagExists(tagname) && !CheckFileTagExists(tagname))
            {
                long created = DateTime.Now.Ticks;
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

        public void RenameFolderTag(string oldname, string newname)
        {
            if (CheckFolderTagExists(oldname))
            {
                if (!CheckFolderTagExists(newname))
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
                long updated = DateTime.Now.Ticks;
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
        /// <param name="path">The path to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FolderTag that contains the path, else raise PathAlreadyExistsException</returns>
        public FolderTag TagFolder(string path, string tagname)
        {
            long lastupdated = DateTime.Now.Ticks;
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
                    if (!CheckRecursiveDirectory((FolderTag)tag, path))
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
        /// <returns>1 if the path is removed, 0 if the path is not found in the Tag, else raise TagNotFoundException</returns>
        public int UntagFolder(string path, string tagname)
        {
            long lastupdated = DateTime.Now.Ticks;
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
        /// Retrieve the FolderTag with the particular tag name
        /// </summary>
        /// <param name="tagname">The name of the FolderTag</param>
        /// <returns>The FolderTag that is to be found, else null</returns>
        public FolderTag RetrieveFolderTag(string tagname)
        {
            return RetrieveFolderTag(tagname, false, 0);
        }
        #endregion

        #region FileTag public implementations
        /// <summary>
        /// Create a File Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <param name="lastupdated">The last updated time of the Tag to be created</param>
        /// <returns>The created Tag, else raise TagAlreadyExistsException</returns>
        public FileTag CreateFileTag(string tagname)
        {
            if (!CheckFileTagExists(tagname) && !CheckFolderTagExists(tagname))
            {
                long created = DateTime.Now.Ticks;
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

        public void RenameFileTag(string oldname, string newname)
        {
            if (CheckFileTagExists(oldname))
            {
                if (!CheckFileTagExists(newname))
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
                long updated = DateTime.Now.Ticks;
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
        /// Tag a folder with a tagname
        /// </summary>
        /// <param name="path">The path to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FileTag that contains the path, else raise PathAlreadyExistsException</returns>
        public FileTag TagFile(string path, string tagname)
        {
            long lastupdated = DateTime.Now.Ticks;
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
        /// Untag a Folder from a tagname
        /// </summary>
        /// <param name="path">The path to untag</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>1 if the path is removed, 0 if the path is not found in the Tag, else raise TagNotFoundException</returns>
        public int UntagFile(string path, string tagname)
        {
            long lastupdated = DateTime.Now.Ticks;
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
        /// Retrieve the FileTag with the particular tag name
        /// </summary>
        /// <param name="tagname">The name of the FileTag</param>
        /// <returns>The FileTag that is to be found, else null</returns>
        public FileTag RetrieveFileTag(string tagname)
        {
            return RetrieveFileTag(tagname, false, 0);
        }
        #endregion

        #region miscellaneous public implementations
        #region completed
        public TaggingProfile LoadFrom(string xmlFilePath)
        {
            TaggingProfile taggingProfile = new TaggingProfile();
            XmlDocument xml = new XmlDocument();
            if (File.Exists(xmlFilePath))
            {
                try
                {
                    xml.Load(xmlFilePath);
                    taggingProfile = ConvertXmlToTaggingProfile(xml);
                    return taggingProfile;
                }
                catch (IOException)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public bool SaveTo(string xmlFilePath)
        {
            XmlDocument xml = ConvertTaggingProfileToXml(_taggingProfile);
            return SaveTagging(xml, "tagging.xml");
        }

        private bool SaveTagging(XmlDocument xml, string path)
        {
            XmlTextWriter textWriter = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);
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

        private TaggingProfile LoadTaggingProfile(string profileFilePath)
        {
            TaggingProfile taggingProfile = new TaggingProfile();
            XmlDocument xml = new XmlDocument();
            if (File.Exists(profileFilePath))
            {
                xml.Load(profileFilePath);
                taggingProfile = ConvertXmlToTaggingProfile(xml);
            }
            return taggingProfile;
        }

        public Tag RetrieveTag(string tagname)
        {
            Tag tag = GetFolderTag(tagname);
            if (tag == null)
            {
                tag = GetFileTag(tagname);
            }
            return tag;
        }

        public Tag RemoveTag(string tagname)
        {
            //Tag toRemove = null;
            //if (CheckFolderTagExists(tagname))
            //{
            //    toRemove = GetFolderTag(tagname);
            //    _taggingProfile.FolderTagList.Remove((FolderTag)toRemove);
            //}
            //else if (CheckFileTagExists(tagname))
            //{
            //    toRemove = GetFileTag(tagname);
            //    _taggingProfile.FileTagList.Remove((FileTag)toRemove);
            //}
            //else
            //{
            //    throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, tagname);
            //}
            //return toRemove;
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

        public int Untag(string path, string tagname)
        {
            Tag tag;
            long lastupdated = DateTime.Now.Ticks;
            if (CheckFolderTagExists(tagname))
            {
                tag = GetFolderTag(tagname);
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
            else if (CheckFileTagExists(tagname))
            {
                tag = GetFileTag(tagname);
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
            else
            {
                throw new TagNotFoundException(ErrorMessage.TAG_NOT_FOUND_EXCEPTION, tagname);
            }
        }

        public int Untag(string path)
        {
            int noOfPath = 0;
            long lastupdated = 0;
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                if (folderTag.Contain(path))
                {
                    lastupdated = DateTime.Now.Ticks;
                    folderTag.RemovePath(path, lastupdated);
                    UpdateTaggingProfileDate(lastupdated);
                    noOfPath++;
                }
            }
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                if (fileTag.Contain(path))
                {
                    lastupdated = DateTime.Now.Ticks;
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
        /// 
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
        /// Find the Similar Path of a particular path. (*see me)
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>The List of Tagged Paths</returns>
        public List<string> FindSimilarPathForFolder(string folderPath)
        {
            string logicalid = folderPath.Split('\\')[0].TrimEnd(':');
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
                string trailingPath = folderTag.FindMatchedParentDirectory(folderPath);
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in folderTag.PathList)
                    {
                        appendedPath = p.Append(trailingPath) + "\\";
                        if (!folderPathList.Contains(appendedPath) && !appendedPath.Equals(folderPath))
                        {
                            folderPathList.Add(appendedPath);
                        }
                    }
                }
            }
            return folderPathList;
        }

        public List<string> FindSimilarPathForFile(string filePath)
        {
            string logicalid = filePath.Split('\\')[0].TrimEnd(':');
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
                string trailingPath = folderTag.FindMatchedParentDirectory(filePath);
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

        public void RenameFolder(string oldPath, string newPath)
        {
            foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
            {
                folderTag.Rename(oldPath, newPath);
            }
        }

        public void RenameFile(string oldPath, string newPath)
        {
            foreach (FileTag fileTag in _taggingProfile.FileTagList)
            {
                fileTag.Rename(oldPath, newPath);
            }
        }

        //public XmlDocument ConvertToXML(string profilename, long created, long lastupdated)
        //{
        //    XmlDocument TaggingDataDocument = new XmlDocument();
        //    XmlElement taggingElement = TaggingDataDocument.CreateElement("tagging");
        //    XmlElement profileElement = TaggingDataDocument.CreateElement("profile");
        //    profileElement.SetAttribute("name", profilename);
        //    profileElement.SetAttribute("createdDate", created.ToString());
        //    profileElement.SetAttribute("lastUpdated", lastupdated.ToString());
        //    foreach (FolderTag folderTag in _taggingProfile.FolderTagList)
        //    {
        //        profileElement.AppendChild(CreateFolderTagElement(TaggingDataDocument, folderTag));
        //    }
        //    foreach (FileTag fileTag in _taggingProfile.FileTagList)
        //    {
        //        profileElement.AppendChild(CreateFileTagElement(TaggingDataDocument, fileTag));
        //    }
        //    taggingElement.AppendChild(profileElement);
        //    TaggingDataDocument.AppendChild(taggingElement);
        //    return TaggingDataDocument;
        //}
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
                profileElement.AppendChild(CreateFolderTagElement(TaggingDataDocument, folderTag));
            }
            foreach (FileTag fileTag in taggingProfile.FileTagList)
            {
                profileElement.AppendChild(CreateFileTagElement(TaggingDataDocument, fileTag));
            }
            taggingElement.AppendChild(profileElement);
            TaggingDataDocument.AppendChild(taggingElement);
            return TaggingDataDocument;
        }

        private static TaggingProfile ConvertXmlToTaggingProfile(XmlDocument xml)
        {
            XmlElement profileElement = (XmlElement)xml.GetElementsByTagName("profile").Item(0);
            TaggingProfile taggingProfile = CreateTaggingProfile(profileElement);
            XmlNodeList tagList = profileElement.ChildNodes;
            foreach (XmlElement tag in tagList)
            {
                if (tag.Name.Equals("folderTag"))
                {
                    FolderTag folderTag = CreateFolderTagFromXml(tag);
                    taggingProfile.FolderTagList.Add(folderTag);
                }
                else
                {
                    FileTag fileTag = CreateFileTagFromXml(tag);
                    taggingProfile.FileTagList.Add(fileTag);
                }
            }
            return taggingProfile;
        }

        #region create tagging profile
        private static TaggingProfile CreateTaggingProfile(XmlElement profileElement)
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

        private static FolderTag CreateFolderTagFromXml(XmlElement tag)
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

        private static FileTag CreateFileTagFromXml(XmlElement tag)
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
            taggedPath.LogicalDriveId = pathname.Split('\\')[0].TrimEnd(':');
            return taggedPath;
        }
        #endregion

        #region create xml document
        private static XmlElement CreateFolderTagElement(XmlDocument TaggingDataDocument, FolderTag folderTag)
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

        private static XmlElement CreateFileTagElement(XmlDocument TaggingDataDocument, FileTag fileTag)
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

        private void UpdateTaggingProfileDate(long created)
        {
            _taggingProfile.LastUpdated = created;
        }

        private bool CheckRecursiveDirectory(FolderTag folderTag, string path)
        {
            foreach (TaggedPath p in folderTag.PathList)
            {
                if (p.Path.StartsWith(path) || path.StartsWith(p.Path))
                {
                    return true;
                }
            }
            return false;
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
