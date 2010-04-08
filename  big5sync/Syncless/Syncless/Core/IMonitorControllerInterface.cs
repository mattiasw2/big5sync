using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Monitor.DTO;
using Syncless.CompareAndSync;
namespace Syncless.Core
{
    /// <summary>
    /// Defines methods for the Monitor Component to call.
    /// </summary>
    public interface IMonitorControllerInterface
    {
        /// <summary>
        /// Handle a file change (New,Update,Rename)
        /// </summary>
        /// <param name="fe">File Change Event with all the information for the file change</param>
        void HandleFileChange(FileChangeEvent fe);
        /// <summary>
        /// Handling Folder Change (New,Rename)
        /// </summary>
        /// <param name="fe">Folder Change Event with all the information for the file change</param>
        void HandleFolderChange(FolderChangeEvent fe);
        /// <summary>
        /// Handling drive change ( plug in and plug out)
        /// </summary>
        /// <param name="dce">Drive Change Event with all the objects</param>
        void HandleDriveChange(DriveChangeEvent dce);
        /// <summary>
        /// Handling delete change (For both file and folder) 
        ///   Unable to detect what type of the file is deleted as the file/folder no longer exist.
        /// </summary>
        /// <param name="dce">Delete Change Event with all the information for the file change</param>
        void HandleDeleteChange(DeleteChangeEvent dce);
        /// <summary>
        /// A method for Monitor Interface to clear the path table.
        /// </summary>
        void ClearPathHash();
    }
}
