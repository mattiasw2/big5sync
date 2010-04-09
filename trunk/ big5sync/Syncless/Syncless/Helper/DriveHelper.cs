using System.Collections.Generic;
using System.Management;

namespace Syncless.Helper
{
    /// <summary>
    /// The class that help to handle Drive operation.
    /// </summary>
    public class DriveHelper
    {
        // Credits of Team 2: SyncButler. With this method, it enables us to find all USB drives regardless of whether they are removable or fixed.

        /// <summary>
        /// Returns a List of drive letters of USB storage devices attached to the computer.
        /// Drive letter format is of the format X:
        /// </summary>
        /// <returns>List of USB Drive letters</returns>
        public static List<string> GetUSBDriveLetters()
        {
            List<string> list = new List<string>();
            ManagementObjectSearcher ddMgmtObjSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");

            foreach (ManagementObject ddObj in ddMgmtObjSearcher.Get())
            {
                foreach (ManagementObject dpObj in ddObj.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementObject ldObj in dpObj.GetRelated("Win32_LogicalDisk"))
                    {
                        list.Add(ldObj["DeviceID"].ToString());
                    }
                }
            }

            return list;
        }
    }
}
