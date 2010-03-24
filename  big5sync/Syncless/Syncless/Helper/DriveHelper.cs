using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace Syncless.Helper
{
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
            ManagementObjectSearcher DDMgmtObjSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");

            foreach (ManagementObject DDObj in DDMgmtObjSearcher.Get())
            {
                foreach (ManagementObject DPObj in DDObj.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementObject LDObj in DPObj.GetRelated("Win32_LogicalDisk"))
                    {
                        list.Add(LDObj["DeviceID"].ToString());
                    }
                }
            }

            return list;
        }
    }
}
