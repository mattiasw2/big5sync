/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System.Collections.Generic;
using System.IO;
namespace Syncless.Filters
{
    /// <summary>
    /// The abstract class for FileFilter
    /// </summary>
    public abstract class FileFilter:Filter
    {
        /// <summary>
        /// Initialize a File Filter
        /// </summary>
        /// <param name="mode">The <see cref="FilterMode"/></param>
        public FileFilter(FilterMode mode):base(mode)
        {
        }
        /// <summary>
        /// Override the Exclude method. By default , it will include directory.
        /// </summary>
        /// <param name="patterns">The list of patterns to exclude</param>
        /// <returns>The list of paths that is not excluded</returns>
        protected override List<string> Exclude(List<string> patterns)
        {
            List<string> outputList = new List<string>();
            foreach (string pattern in patterns)
            {
                if (Directory.Exists(pattern))
                {
                    //is directory
                    outputList.Add(pattern);
                    continue;
                }
                FileInfo file = new FileInfo(pattern);
                
                if (!Match(file.Name))
                {
                    outputList.Add(pattern);
                }
            }

            return outputList;
        }
        /// <summary>
        /// Override the Include method. By default, it will include directory
        /// </summary>
        /// <param name="patterns">The list of patterns to include</param>
        /// <returns>The list of paths that is included</returns>
        protected override List<string> Include(List<string> patterns)
        {
            List<string> outputList = new List<string>();
            foreach (string pattern in patterns)
            {
                if (Directory.Exists(pattern))
                {
                    //is directory
                    outputList.Add(pattern);
                    continue;
                }
                FileInfo file = new FileInfo(pattern);
                
                if (Match(file.Name))
                {
                    if (!outputList.Contains(pattern))
                    {
                        outputList.Add(pattern);
                    }
                }
            }
            return outputList;
        }
    }
}
