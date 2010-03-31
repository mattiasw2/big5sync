using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Threading;
using Syncless.Monitor;
using Syncless.Monitor.DTO;
namespace Syncless.Core
{
    public class DeviceWatcher
    {
        private static DeviceWatcher _instance;
        /// <summary>
        /// The singleton instance of DeviceWatcher object.
        /// Reference from http://dotnetslackers.com/community/blogs/basharkokash/archive/2008/03/15/USB-Detection-source-code.aspx
        /// </summary>
        public static DeviceWatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeviceWatcher();
                }
                return _instance;
            }
        }
        private Dictionary<string, DriveInfo> connectedDrives;
        private ManagementEventWatcher insertUSBWatcher;
        private ManagementEventWatcher removeUSBWatcher;
        private const int DELAY_TIME = 10000;

        private DeviceWatcher()
        {
            connectedDrives = RetrieveAllDrives();
            try
            {
                AddInsertUSBHandler();
                AddRemoveUSBHandler();
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
            }
        }

        /// <summary>
        /// Retrieve all fixed and removable connected drives.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, DriveInfo> RetrieveAllDrives()
        {
            Dictionary<string, DriveInfo> drives = new Dictionary<string, DriveInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
            {
                if (driveInfo.DriveType == DriveType.Fixed || driveInfo.DriveType == DriveType.Removable)
                {
                    drives.Add(driveInfo.Name.ToLower(), driveInfo);
                }
            }
            return drives;
        }

        private void AddInsertUSBHandler()
        {
            WqlEventQuery query;
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;
            insertUSBWatcher = null;
            try
            {
                query = new WqlEventQuery();
                query.EventClassName = "__InstanceCreationEvent";
                query.WithinInterval = new TimeSpan(0, 0, 3);
                query.Condition = "TargetInstance ISA 'Win32_USBControllerdevice'";
                insertUSBWatcher = new ManagementEventWatcher(scope, query);
                insertUSBWatcher.EventArrived += USBInserted;
                insertUSBWatcher.Start();
            }
            catch(Exception e)
            {
                if (insertUSBWatcher != null)
                {
                    insertUSBWatcher.Stop();
                }
                throw e;
            }
        }

        private void USBInserted(object sender, EventArrivedEventArgs e)
        {
            Thread.Sleep(DELAY_TIME);
            Dictionary<string, DriveInfo> newConnectedDrives = RetrieveAllDrives();
            foreach (KeyValuePair<string, DriveInfo> kvp in newConnectedDrives)
            {
                if (!connectedDrives.ContainsKey(kvp.Key))
                {
                    DriveInfo drive = kvp.Value;
                    DriveChangeEvent dce = new DriveChangeEvent(DriveChangeType.DRIVE_IN, drive);
                    ServiceLocator.MonitorI.HandleDriveChange(dce);
                    ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(drive.Name + " inserted.");
                    //Console.WriteLine(drive.Name + " inserted.");
                }
            }
            connectedDrives = newConnectedDrives;
        }

        private void AddRemoveUSBHandler()
        {
            WqlEventQuery query;
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;
            removeUSBWatcher = null;
            try
            {
                query = new WqlEventQuery();
                query.EventClassName = "__InstanceDeletionEvent";
                query.WithinInterval = new TimeSpan(0, 0, 1);
                query.Condition = "TargetInstance ISA 'Win32_USBControllerdevice'";
                removeUSBWatcher = new ManagementEventWatcher(scope, query);
                removeUSBWatcher.EventArrived += USBRemoved;
                removeUSBWatcher.Start();
            }
            catch (Exception e)
            {
                if (removeUSBWatcher != null)
                {
                    removeUSBWatcher.Stop();
                }
                throw e;
            }
        }

        private void USBRemoved(object sender, EventArrivedEventArgs e)
        {
            Dictionary<string, DriveInfo> newConnectedDrives = RetrieveAllDrives();
            foreach (KeyValuePair<string, DriveInfo> kvp in connectedDrives)
            {
                if (!newConnectedDrives.ContainsKey(kvp.Key))
                {
                    DriveInfo drive = kvp.Value;
                    DriveChangeEvent dce = new DriveChangeEvent(DriveChangeType.DRIVE_OUT, drive);
                    ServiceLocator.MonitorI.HandleDriveChange(dce);
                    ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(drive.Name + " removed.");
                    //Console.WriteLine(drive.Name + " removed.");
                }
            }
            connectedDrives = newConnectedDrives;
        }

        public void Terminate()
        {
            if (insertUSBWatcher != null)
            {
                insertUSBWatcher.Stop();
            }
            if (removeUSBWatcher != null)
            {
                removeUSBWatcher.Stop();
            }
        }
    }
}
