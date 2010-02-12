using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Tagging.Exceptions;

namespace Syncless.Tagging
{
    public class TaggingLayer
    {
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

        private List<Tag> _tagList;

        public List<Tag> TagList
        {
            get { return _tagList; }
            set { _tagList = value; }
        }
        
        private TaggingLayer()
        {
            _tagList = new List<Tag>();
        }

        public FolderTag CreateFolderTag(string tagname)
        {
            if (!CheckTagExists(tagname))
            {
                FolderTag tag = new FolderTag(tagname);
                _tagList.Add(tag);
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException();
            }
        }

        public bool RemoveFolderTag(string tagname)
        {
            if (CheckTagExists(tagname))
            {
                _tagList.Remove((FolderTag)GetTag(tagname));
                return true;
            }
            else
            {
                throw new TagNotFoundException();
            }
        }

        /// <summary>
        /// Tag a Folder with a tagname. If the tagname does not exist , create it. If a Folder is tagged with a tagname of a file, raise an exception.
        /// </summary>
        /// <param name="path">The path to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FolderTag that contain the path.</returns>
        public FolderTag TagFolder(string path, string tagname)
        {
            FolderTag tag = RetrieveFolderTag(tagname, true);
            if (tag != null)
            {
                tag.AddPath(path);
                AddTag(tag);
                return tag;
            }
            else
            {
                throw new TagNotFoundException();
            }
        }
        
        /// <summary>
        /// Untag a Folder from a tagname. If the Folder is not tagged with the tagname , raise an exception. 
        /// </summary>
        /// <param name="path">The path to untag</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FolderTag that contain the path</returns>
        public FolderTag UntagFolder(string path, string tagname)
        {
            FolderTag tag = RetrieveFolderTag(tagname);
            if (tag != null)
            {
                bool success = tag.RemovePath(path);
                if (success)
                {
                    return tag;
                }
                else
                {
                    throw new PathNotFoundException();
                }
            }
            else
            {
                throw new TagNotFoundException();
            }
        }

        public FileTag CreateFileTag(string tagname)
        {
            if (!CheckTagExists(tagname))
            {
                FileTag tag = new FileTag(tagname);
                _tagList.Add(tag);
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException();
            }
        }

        public bool RemoveFileTag(string tagname)
        {
            if (CheckTagExists(tagname))
            {
                _tagList.Remove((FileTag)GetTag(tagname));
                return true;
            }
            else
            {
                throw new TagNotFoundException();
            }
        }

        /// <summary>
        /// Tag a File with a tagname. If the tagname does not exist , create it. If a File is tagged with a tagname of a folder, raise an exception.
        /// </summary>
        /// <param name="path">The path to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FileTag that contain the path.</returns>
        public FileTag TagFile(string path, string tagname)
        {
            FileTag tag = RetrieveFileTag(tagname, true);
            if (tag != null)
            {
                tag.AddPath(path);
                AddTag(tag);
                return tag;
            }
            else
            {
                throw new TagNotFoundException();
            }
        }
        
        /// <summary>
        /// Untag a File from a tagname. If the File is not tagged with the tagname , raise an exception. 
        /// </summary>
        /// <param name="path">The path to untag</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FileTag that contain the path</returns>
        public FileTag UntagFile(string path, string tagname)
        {
            FileTag tag = RetrieveFileTag(tagname);
            if (tag != null)
            {
                bool success = tag.RemovePath(path);
                if (success)
                {
                    return tag;
                }
                else
                {
                    throw new PathNotFoundException();
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
        /// <returns>The FolderTag</returns>
        public FolderTag RetrieveFolderTag(string tagname)
        {
            return RetrieveFolderTag(tagname, false);
        }

        /// <summary>
        /// Retrieve the FolderTag with the particular tag name. If create is set to true, if the FolderTag does not exist, a new FolderTag will be created.
        /// </summary>
        /// <param name="tagname">The name of the FolderTag</param>
        /// <param name="create">flag to determinte whether a FolderTag should be created if it is not found.</param>
        /// <returns></returns>
        public FolderTag RetrieveFolderTag(string tagname, bool create)
        {
            return (FolderTag)RetrieveTag(tagname, create, true);
        }
        
        /// <summary>
        /// Retrieve the FileTag with the particular tag name
        /// </summary>
        /// <param name="tagname">The name of the FileTag</param>
        /// <returns>The FileTag</returns>
        public FileTag RetrieveFileTag(string tagname)
        {
            return RetrieveFileTag(tagname, false);
        }
        
        /// <summary>
        /// Retrieve the FileTag with the particular tag name. If create is set to true, if the FileTag does not exist, a new FileTag will be created.
        /// </summary>
        /// <param name="tagname">The name of the FileTag</param>
        /// <param name="create">flag to determinte whether a FileTag should be created if it is not found.</param>
        /// <returns></returns>
        public FileTag RetrieveFileTag(string tagname, bool create)
        {
            return (FileTag)RetrieveTag(tagname, create, false);
        }

        /// <summary>
        /// Retrieve all the tag that have path in a logical drive.
        /// </summary>
        /// <param name="logicalId">The Logical Id</param>
        /// <returns>The list of Tag</returns>
        public List<Tag> RetrieveTagByLogicalId(string logicalId)
        {
            bool found;
            List<Tag> tagList = new List<Tag>();
            foreach (Tag tag in _tagList)
            {
                if (tag is FolderTag)
                {
                    found = CheckID(tag, logicalId, true);
                }
                else
                {
                    found = CheckID(tag, logicalId, false);
                }
                if (found)
                {
                    tagList.Add(tag);
                }
            }
            return tagList;
        }
        
        /// <summary>
        /// Find the Similar Path of a particular path. (*see me)
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>The List of Tagged Paths</returns>
        public List<TaggedPath> FindSimilarPath(string path)
        {
            return null;
        }
        
        /*Private implementation*/
        private Tag RetrieveTag(string tagName, bool toCreate, bool isFolder)
        {
            if (CheckTagExists(tagName))
            {
                return GetTag(tagName);
            }
            else
            {
                if (toCreate)
                {
                    if (isFolder)
                    {
                        FolderTag tag = new FolderTag(tagName);
                        return tag;
                    }
                    else
                    {
                        FileTag tag = new FileTag(tagName);
                        return tag;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        private void AddTag(Tag tag)
        {
            if (!CheckTagExists(tag.TagName))
            {
                _tagList.Add(tag);
            }
        }

        private Tag GetTag(string tagName)
        {
            foreach (Tag tag in _tagList)
            {
                if (tag.TagName.Equals(tagName))
                {
                    return tag;
                }
            }
            return null;
        }

        private bool CheckTagExists(string tagName)
        {
            foreach (Tag tag in _tagList)
            {
                if (tag.TagName.Equals(tagName))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckID(Tag tag, string ID, bool isFolder)
        {
            if (isFolder)
            {
                foreach (TaggedPath path in ((FolderTag)tag).FolderPaths)
                {
                    if (path.LogicalDriveId.Equals(ID))
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (TaggedPath path in ((FileTag)tag).FilePaths)
                {
                    if (path.LogicalDriveId.Equals(ID))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
