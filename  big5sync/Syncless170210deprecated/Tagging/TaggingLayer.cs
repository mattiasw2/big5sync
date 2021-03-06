﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Syncless.Tagging.Exceptions;
using System.Diagnostics;

namespace Syncless.Tagging
{
    public class TaggingLayer
    {
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

        private List<FolderTag> _folderTagList;

        public List<FolderTag> FolderTagList
        {
            get { return _folderTagList; }
            set { _folderTagList = value; }
        }

        private List<FileTag> _fileTagList;

        public List<FileTag> FileTagList
        {
            get { return _fileTagList; }
            set { _fileTagList = value; }
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
            _folderTagList = new List<FolderTag>();
            _fileTagList = new List<FileTag>();
            _taggingProfile = new TaggingProfile();
        }

        public void Init(string profileFilePath)
        {
            _folderTagList = LoadFolderTagList(profileFilePath);
            _fileTagList = LoadFileTagList(profileFilePath);
            _taggingProfile = LoadTaggingProfile(profileFilePath);
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
            if (!CheckFolderTagExists(tagname))
            {
                long created = DateTime.Now.Ticks;
                FolderTag tag = new FolderTag(tagname, created);
                _folderTagList.Add(tag);
                UpdateTaggingProfileDate(created);
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException();
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
                _folderTagList.Remove(toRemove);
                UpdateTaggingProfileDate(updated);
                return toRemove;
            }
            else
            {
                throw new TagNotFoundException();
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
                throw new TagTypeConflictException();
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
                        throw new RecursiveDirectoryException();
                    }
                }
                else
                {
                    throw new PathAlreadyExistsException();
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
                throw new TagNotFoundException();
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
            if (!CheckFileTagExists(tagname))
            {
                long created = DateTime.Now.Ticks;
                FileTag tag = new FileTag(tagname, created);
                _fileTagList.Add(tag);
                UpdateTaggingProfileDate(created);
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException();
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
                _fileTagList.Remove(toRemove);
                UpdateTaggingProfileDate(updated);
                return toRemove;
            }
            else
            {
                throw new TagNotFoundException();
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
                throw new TagTypeConflictException();
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
                    throw new PathAlreadyExistsException();
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
                throw new TagNotFoundException();
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
        #region incomplete
        public TaggingProfile LoadTaggingProfile(string profileFilePath)
        {
            throw new NotImplementedException();
        }

        private List<FileTag> LoadFileTagList(string profileFilePath)
        {
            throw new NotImplementedException();
        }

        private List<FolderTag> LoadFolderTagList(string profileFilePath)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region completed
        public bool WriteXmlToFile()
        {
            XmlDocument xml = ConvertToXML("Khoon\'s sync", 1622010183523, 1622010183523);
            xml.Save(@"_tagging.xml");
            return true;
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
            Tag toRemove;
            if (CheckFolderTagExists(tagname))
            {
                toRemove = GetFolderTag(tagname);
                _folderTagList.Remove((FolderTag)toRemove);
            }
            else if (CheckFileTagExists(tagname))
            {
                toRemove = GetFileTag(tagname);
                _fileTagList.Remove((FileTag)toRemove);
            }
            else
            {
                throw new TagNotFoundException();
            }
            return toRemove;
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
                throw new TagNotFoundException();
            }
        }

        public int Untag(string path)
        {
            int noOfPath = 0;
            long lastupdated = 0;
            foreach (FolderTag folderTag in _folderTagList)
            {
                if (folderTag.Contain(path))
                {
                    lastupdated = DateTime.Now.Ticks;
                    folderTag.RemovePath(path, lastupdated);
                    UpdateTaggingProfileDate(lastupdated);
                    noOfPath++;
                }
            }
            foreach (FileTag fileTag in _fileTagList)
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
        public List<Tag> RetrieveTagByLogicalId(string logicalid)
        {
            bool found;
            List<Tag> tagList = new List<Tag>();
            foreach (FolderTag folderTag in _folderTagList)
            {
                found = CheckFolderID(folderTag, logicalid);
                if (found)
                {
                    tagList.Add(folderTag);
                }
            }
            foreach (FileTag fileTag in _fileTagList)
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
        /// Find the Similar Path of a particular path. (*see me)
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>The List of Tagged Paths</returns>
        public List<string> FindSimilarPathForFolder(string folderPath)
        {
            List<string> folderPathList = new List<string>();
            foreach (FolderTag folderTag in _folderTagList)
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
            return folderPathList;
        }

        public List<string> FindSimilarPathForFile(string filePath)
        {
            string logicalid = filePath.Split('\\')[0].TrimEnd(':');
            List<string> filePathList = new List<string>();
            foreach (FileTag fileTag in _fileTagList)
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

        public XmlDocument ConvertToXML(string profilename, long created, long lastupdated)
        {
            XmlDocument TaggingDataDocument = new XmlDocument();
            XmlElement taggingElement = TaggingDataDocument.CreateElement("tagging");
            XmlElement profileElement = TaggingDataDocument.CreateElement("profile");
            profileElement.SetAttribute("name", profilename);
            profileElement.SetAttribute("createdDate", created.ToString());
            profileElement.SetAttribute("lastUpdated", lastupdated.ToString());
            foreach (FolderTag folderTag in _folderTagList)
            {
                profileElement.AppendChild(CreateFolderTagElement(TaggingDataDocument, folderTag));
            }
            foreach (FileTag fileTag in _fileTagList)
            {
                profileElement.AppendChild(CreateFileTagElement(TaggingDataDocument, fileTag));
            }
            taggingElement.AppendChild(profileElement);
            TaggingDataDocument.AppendChild(taggingElement);
            return TaggingDataDocument;
        }
        #endregion
        #endregion

        #region private methods implementations
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
            long profilecreated = long.Parse(profileElement.GetAttribute("created"));
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

        #region completed
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
            foreach (FolderTag folderTag in _folderTagList)
            {
                if (folderTag.TagName.Equals(tagname))
                {
                    return folderTag;
                }
            }
            foreach (FileTag fileTag in _fileTagList)
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
                _folderTagList.Add(tag);
            }
        }

        private void AddFileTag(FileTag tag)
        {
            if (!CheckFileTagExists(tag.TagName))
            {
                _fileTagList.Add(tag);
            }
        }

        private FolderTag GetFolderTag(string tagname)
        {
            if (CheckFolderTagExists(tagname))
            {
                foreach (FolderTag folderTag in _folderTagList)
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
                foreach (FileTag fileTag in _fileTagList)
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
            foreach (FolderTag folderTag in _folderTagList)
            {
                if (folderTag.TagName.Equals(tagname))
                {
                    return true;
                }
            }
            foreach (FileTag fileTag in _fileTagList)
            {
                if (fileTag.TagName.Equals(tagname))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckFileTagExists(string tagname)
        {
            foreach (FileTag fileTag in _fileTagList)
            {
                if (fileTag.TagName.Equals(tagname))
                {
                    return true;
                }
            }
            foreach (FolderTag folderTag in _folderTagList)
            {
                if (folderTag.TagName.Equals(tagname))
                {
                    return false;
                }
            }
            return false;
        }

        private List<FolderTag> RetrieveFolderTagById(string logicalid)
        {
            bool found;
            List<FolderTag> tagList = new List<FolderTag>();
            foreach (FolderTag folderTag in _folderTagList)
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
        //private List<FolderTag> LoadFolderTagList()
        //{
        //    XmlDocument xml = new XmlDocument();
        //    return new List<FolderTag>();
        //}

        //private List<FileTag> LoadFileTagList()
        //{
        //    return new List<FileTag>();
        //}
        #endregion
    }
}
