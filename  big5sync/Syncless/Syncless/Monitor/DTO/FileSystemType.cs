using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Monitor.DTO
{
    /// <summary>
    /// This enum specifies the type of file system change.
    /// </summary>
    public enum FileSystemType
    {
        /// <summary>
        /// A File.
        /// </summary>
        FILE,
        /// <summary>
        /// A Folder.
        /// </summary>
        FOLDER,
        /// <summary>
        /// Either a File or a Folder.
        /// </summary>
        UNKNOWN
    }
}
