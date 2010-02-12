using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Monitor;
using Syncless.CompareAndSync;
namespace Syncless.Core
{
    public interface IMonitorControllerInterface
    {
        
        void HandleFileChange(FileInfo info, FileChangeType type);
        void HandleFolderChange(DirectoryInfo info, FileChangeType type);
        void HandleDriveChange(DriveInfo info, DriveChangeType type);
    }
}
