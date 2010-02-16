using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
namespace Syncless.CompareAndSync
{
    public class CompareSyncController
    {
        private static CompareSyncController _instance;
        public static CompareSyncController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CompareSyncController();
                }
                return _instance;
            }
        }
        private CompareSyncController()
        {

        }
        /// <summary>
        /// Manually Sync a Tag. 
        /// </summary>
        /// <param name="tag">The tag to be synchronized</param>
        /// <returns>The list of Sync Results</returns>
        public List<SyncResult> SyncTag(Tag tag)
        {
            return null;
        }
        /// <summary>
        /// Sync a source Path to a list of  destination paths. The path can be File or Folder.
        /// 
        /// </summary>
        /// <param name="sourcePath">The source path</param>
        /// <param name="destinationPath">The list of destination path</param>
        /// <param name="changeType">The Change that was detected on source path</param>
        /// <returns>The list of Sync Results</returns>
        public List<SyncResult> SyncPath(string sourcePath, List<string> destinationPath, FileChangeType changeType)
        {
            return null;
        }
        /// <summary>
        /// Preview the result of a foldertag synchronization
        /// </summary>
        /// <param name="tag">The Folder Tag to preview</param>
        /// <returns>The list of Compare Result</returns>
        public List<CompareResult> CompareFolder(string tagName, List<string> paths)
        {
            return new Comparer().CompareFolder(tagName, paths);
        }
        /// <summary>
        /// Preview the result of a foldertag synchronization
        /// </summary>
        /// <param name="tag">The Folder Tag to preview</param>
        /// <returns>The list of Compare Result</returns>
        public List<CompareResult> PreviewFileCompare(FileTag tag)
        {
            return null;
        }
    }
}
