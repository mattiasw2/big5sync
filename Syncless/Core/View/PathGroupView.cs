/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System.Collections.Generic;

namespace Syncless.Core.View
{
    /// <summary>
    /// The class that group a list of path
    /// </summary>
    public class PathGroupView
    {
        /// <summary>
        /// Get and Set the name of the PathGroup
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Get and Set the list of path.
        /// </summary>
        public List<PathView> PathList { get; set; }
        /// <summary>
        /// Get the list of Path in String.
        /// </summary>
        public List<string> PathListString
        {
            get
            {
                List<string> pathListString = new List<string>();
                foreach (PathView p in PathList)
                {
                    pathListString.Add((p.Path));
                }
                return pathListString;
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">name of the group</param>
        public PathGroupView(string name)
        {
            Name = name;
            PathList = new List<PathView>();
        }
        /// <summary>
        /// Add a path to the group
        /// </summary>
        /// <param name="path">Path to add</param>
        public void AddPath(string path)
        {
            PathList.Add(new PathView(path));
        }
        /// <summary>
        /// Add a path to the group
        /// </summary>
        /// <param name="path">PathView To add</param>
        public void AddPath(PathView path)
        {
            PathList.Add(path);
        }
    }
}
