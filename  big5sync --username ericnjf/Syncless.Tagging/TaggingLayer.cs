using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        private TaggingLayer()
        {

        }
        /// <summary>
        /// Tag a Folder with a tagname. If the tagname does not exist , create it. If a Folder is tagged with a tagname of a file, raise an exception.
        /// </summary>
        /// <param name="path">The path to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FolderTag that contain the path.</returns>
        public FolderTag TagFolder(string path, string tagname)
        {
            return null;
        }
        /// <summary>
        /// Untag a Folder from a tagname. If the Folder is not tagged with the tagname , raise an exception. 
        /// </summary>
        /// <param name="path">The path to untag</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FolderTag that contain the path</returns>
        public FolderTag UntagFolder(string path, string tagname)
        {
            return null;
        }
        /// <summary>
        /// Tag a File with a tagname. If the tagname does not exist , create it. If a File is tagged with a tagname of a folder, raise an exception.
        /// </summary>
        /// <param name="path">The path to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FileTag that contain the path.</returns>
        public FileTag TagFile(string path, string tagname)
        {
            return null;
        }
        /// <summary>
        /// Untag a File from a tagname. If the File is not tagged with the tagname , raise an exception. 
        /// </summary>
        /// <param name="path">The path to untag</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The FileTag that contain the path</returns>
        public FileTag UntagFile(string path, string tagname)
        {
            return null;
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
            return null;
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
            return null;
        }
        /// <summary>
        /// Retrieve all the tag that have path in a logical drive.
        /// </summary>
        /// <param name="logicalId">The Logical Id</param>
        /// <returns>The list of Tag</returns>
        public List<Tag> RetrieveTagByLogicalId(string logicalId)
        {
            return null;
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
        

    }
}
