/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System;

namespace Syncless.CompareAndSync.Manual.CompareObject
{
    /// <summary>
    /// Similar to <see cref="FolderCompareObject"/>, but it stores the list of paths to synchronize in addition.
    /// </summary>
    public class RootCompareObject : FolderCompareObject
    {
        private readonly string[] _paths;

        /// <summary>
        /// Initializes a new instance of <c>RootCompareObject</c> given the list of paths to synchronize.
        /// </summary>
        /// <param name="paths">An <see cref="Array" /> of <see cref="string"/> containing the paths to synchronize.</param>
        public RootCompareObject(string[] paths)
            : base(null, paths.Length, null)
        {
            _paths = paths;
        }

        #region Properties

        /// <summary>
        /// Gets the current <see cref="Array"/> of paths.
        /// </summary>
        public string[] Paths
        {
            get { return _paths; }
        }

        #endregion

    }
}