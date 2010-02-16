using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Syncless.Tagging.Exceptions;

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
        #endregion

        private TaggingLayer()
        {
            _folderTagList = LoadFolderTagList();
            _fileTagList = LoadFileTagList();
        }

        #region FolderTag public implementations
        /// <summary>
        /// Create a Folder Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <param name="lastupdated">The last updated time of the Tag to be created</param>
        /// <returns>The created Tag, else raise TagAlreadyExistsException</returns>
        public FolderTag CreateFolderTag(string tagname, long lastupdated)
        {
            if (!CheckFolderTagExists(tagname))
            {
                FolderTag tag = new FolderTag(tagname, lastupdated);
                _folderTagList.Add(tag);
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
                toRemove = GetFolderTag(tagname);
                _folderTagList.Remove(toRemove);
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
        public FolderTag TagFolder(string path, string tagname, long lastupdated)
        {
            FolderTag tag = RetrieveFolderTag(tagname, true, lastupdated);
            if (!tag.Contain(path))
            {
                tag.AddPath(path, lastupdated);
                AddFolderTag(tag);
                return tag;
            }
            else
            {
                throw new PathAlreadyExistsException();
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
            FolderTag tag = RetrieveFolderTag(tagname);
            if (tag != null)
            {
                if (tag.RemovePath(path))
                {
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
        public FileTag CreateFileTag(string tagname, long lastupdated)
        {
            if (!CheckFileTagExists(tagname))
            {
                FileTag tag = new FileTag(tagname, lastupdated);
                _fileTagList.Add(tag);
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
                toRemove = GetFileTag(tagname);
                _fileTagList.Remove(toRemove);
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
        public FileTag TagFile(string path, string tagname, long lastupdated)
        {
            FileTag tag = RetrieveFileTag(tagname, true, lastupdated);
            if (!tag.Contain(path))
            {
                tag.AddPath(path, lastupdated);
                AddFileTag(tag);
                return tag;
            }
            else
            {
                throw new PathAlreadyExistsException();
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
            FileTag tag = RetrieveFileTag(tagname);
            if (tag != null)
            {
                if (tag.RemovePath(path))
                {
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
            if (CheckFolderTagExists(tagname))
            {
                tag = GetFolderTag(tagname);
                if (tag.Contain(path))
                {
                    tag.RemovePath(path);
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
                    tag.RemovePath(path);
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
            foreach (FolderTag folderTag in _folderTagList)
            {
                if (folderTag.Contain(path))
                {
                    folderTag.RemovePath(path);
                    noOfPath++;
                }
            }
            foreach (FileTag fileTag in _fileTagList)
            {
                if (fileTag.Contain(path))
                {
                    fileTag.RemovePath(path);
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
        public List<String> FindSimilarPath(string path)
        {
            return null;
        }

        #region create xml document
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

        private static XmlElement CreateFolderTagElement(XmlDocument TaggingDataDocument, FolderTag folderTag)
        {
            XmlElement folderTagElement = TaggingDataDocument.CreateElement("folderTag");
            folderTagElement.SetAttribute("name", folderTag.TagName);
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
            taggedFileElement.SetAttribute("lastUpdated", path.LastUpdated.ToString());
            XmlElement filePathElement = TaggingDataDocument.CreateElement("path");
            filePathElement.InnerText = path.Path;
            taggedFileElement.AppendChild(filePathElement);
            return taggedFileElement;
        }
        #endregion
        #endregion

        #region private methods implementations
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
            return false;
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

        private List<FolderTag> LoadFolderTagList()
        {
            return new List<FolderTag>();
        }

        private List<FileTag> LoadFileTagList()
        {
            return new List<FileTag>();
        }

        public bool WriteXmlToFile()
        {
            XmlDocument xml = ConvertToXML("Khoon\'s sync", 1622010183523, 1622010183523);
            xml.Save(@"D:\My Homework\SEM2 AY0910\CS3215\Project\Syncless.SVN\Syncless.Tester\_tagging.xml");
            return true;
        }
        #endregion
    }
}
