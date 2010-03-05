using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Syncless.Filters
{
    public class FilterNonExist : AbstractFilter
    {       

        /// <summary>
        /// Check if the path exist
        /// </summary>
        /// <param name="file">path</param>
        /// <returns>true if the path is a file or a folder, else false</returns>
        public override bool Match(string path)
        {
            if (path == null)
            {
                return false;
            }
            if (File.Exists(path) || Directory.Exists(path))
            {
                return false;
            }
            return true;
        }
        public FilterNonExist():base(Mode.EXCLUDE)
        {

        }
    }
}
