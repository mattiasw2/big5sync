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
            _tagList = Load();
        }

        /// <summary>
        /// Create a Folder Tag of tagname, raise a TagAlreadyExistsException if the tag has been created
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <returns>The created Tag</returns>
        public FolderTag CreateFolderTag(string tagname)
        {
            if (!CheckTagExists(tagname))
            {
                FolderTag tag = new FolderTag(tagname);
                _tagList.Add(tag);
                Save();
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException();
            }
        }

        /// <summary>
        /// Remove the Folder Tag of tagname, raise TagNotFoundException if the tag does not exist
        /// </summary>
        /// <param name="tagname">The name of the Tag to be removed</param>
        /// <returns>True if the tag is removed successfully, else raise an exception</returns>
        public bool RemoveFolderTag(string tagname)
        {
            if (CheckTagExists(tagname))
            {
                _tagList.Remove((FolderTag)GetTag(tagname));
                Save();
                return true;
            }
            else
            {
                throw new TagNotFoundException();
            }
        }

        /// <summary>
        /// Tag a Folder with a tagname. If the tagname does not exist , create it. If a Folder is tagged with a tagname of a file, 
        /// raise an exception.
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
                Save();
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
                    Save();
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
        /// Create a File Tag of tagname, raise a TagAlreadyExistsException if the tag has been created
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <returns>The created Tag</returns>
        public FileTag CreateFileTag(string tagname)
        {
            if (!CheckTagExists(tagname))
            {
                FileTag tag = new FileTag(tagname);
                _tagList.Add(tag);
                Save();
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException();
            }
        }

        /// <summary>
        /// Remove the File Tag of tagname, raise TagNotFoundException if the tag does not exist
        /// </summary>
        /// <param name="tagname">The name of the Tag to be removed</param>
        /// <returns>True if the tag is removed successfully, else raise an exception</returns>
        public bool RemoveFileTag(string tagname)
        {
            if (CheckTagExists(tagname))
            {
                _tagList.Remove((FileTag)GetTag(tagname));
                Save();
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
                Save();
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
                    Save();
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
        /// <summary>
        /// Get the tag of tagname. If toCreate is true, if the Tag has not been created, then create the Tag.
        /// If isFolder is true, a Folder Tag is created, else a Folder Tag is created.
        /// </summary>
        /// <param name="tagname">The name of the tag to be retrieved</param>
        /// <param name="toCreate">Indicate whether to create the Tag if not found</param>
        /// <param name="isFolder">Indicate whether to create a Folder Tag or a File Tag</param>
        /// <returns>The Tag that has been created/found</returns>
        private Tag RetrieveTag(string tagname, bool toCreate, bool isFolder)
        {
            if (CheckTagExists(tagname))
            {
                return GetTag(tagname);
            }
            else
            {
                if (toCreate)
                {
                    if (isFolder)
                    {
                        FolderTag tag = new FolderTag(tagname);
                        return tag;
                    }
                    else
                    {
                        FileTag tag = new FileTag(tagname);
                        return tag;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Add a Tag to the tag list if it does not exist in the list
        /// </summary>
        /// <param name="tag">The Tag object to be added to the tag list</param>
        private void AddTag(Tag tag)
        {
            if (!CheckTagExists(tag.TagName))
            {
                _tagList.Add(tag);
            }
        }

        /// <summary>
        /// Get the Tag of tagname.
        /// </summary>
        /// <param name="tagname">The name of the Tag to be retrieved</param>
        /// <returns>If the Tag exists in the tag list, return the Tag, else return null</returns>
        private Tag GetTag(string tagname)
        {
            foreach (Tag tag in _tagList)
            {
                if (tag.TagName.Equals(tagname))
                {
                    return tag;
                }
            }
            return null;
        }

        /// <summary>
        /// Check whether the Tag of tagname exist in the tag list.
        /// </summary>
        /// <param name="tagname">The name of the Tag to check</param>
        /// <returns>True if the Tag is found in the list, else return false</returns>
        private bool CheckTagExists(string tagname)
        {
            foreach (Tag tag in _tagList)
            {
                if (tag.TagName.Equals(tagname))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Check whether a Tag contains paths of logical drive ID same as ID.
        /// </summary>
        /// <param name="tag">The Tag object to be checked</param>
        /// <param name="ID">The ID to be searched</param>
        /// <param name="isFolder">Indicate whether the given Tag is a Folder Tag or File Tag</param>
        /// <returns>True if the Tag contains paths with ID, else return false</returns>
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

        /// <summary>
        /// Write to _tagging.xml the updated tag list
        /// TODO: write the logic to write the current tag list to xml
        /// </summary>
        private void Save()
        {
        }

        /// <summary>
        /// Load from _tagging.xml to create the tag list
        /// TODO: write the logic to read from xml and create tag list
        /// </summary>
        /// <returns>A list of Tags</returns>
        private List<Tag> Load()
        {
            return new List<Tag>();
        }
    }
}
