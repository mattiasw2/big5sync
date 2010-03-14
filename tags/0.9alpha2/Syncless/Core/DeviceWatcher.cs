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
        private List<DriveInfo> connectedDrives;
        private ManagementEventWatcher insertUSBWatcher;
        private ManagementEventWatcher removeUSBWatcher;
        private const int DELAY_TIME = 10000;

        private DeviceWatcher()
        {
            connectedDrives = RetrieveAllDrives();
            AddInsertUSBHandler();
            AddRemoveUSBHandler();
        }

        /// <summary>
        /// Retrieve all fixed and removable connected drives.
        /// </summary>
        /// <returns></returns>
        private List<DriveInfo> RetrieveAllDrives()
        {
            List<DriveInfo> drives = new List<DriveInfo>();
            foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
            {
                if (driveInfo.DriveType == DriveType.Fixed || driveInfo.DriveType == DriveType.Removable || driveInfo.DriveType == DriveType.Unknown)
                {
                    drives.Add(driveInfo);
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
            IMonitorControllerInterface control = ServiceLocator.MonitorI;
            List<DriveInfo> newConnectedDrives = RetrieveAllDrives();
            int i = 0;
            foreach (DriveInfo drive in newConnectedDrives)
            {
                bool inserted = false;
                if (i >= connectedDrives.Count)
                {
                    inserted = true;
                }
                else if (!drive.Name.Equals(connectedDrives[i].Name))
                {
                    inserted = true;
                    i--;
                }
                if (inserted)
                {
                    DriveChangeEvent dce = new DriveChangeEvent(DriveChangeType.DRIVE_IN, drive);
                    control.HandleDriveChange(dce);
                    Console.WriteLine(drive.Name + " inserted.");
                }
                i++;
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
            IMonitorControllerInterface control = ServiceLocator.MonitorI;
            List<DriveInfo> newConnectedDrives = RetrieveAllDrives();
            int i = 0;
            foreach (DriveInfo drive in connectedDrives)
            {
                bool removed = false;
                if (i >= newConnectedDrives.Count)
                {
                    removed = true;
                }
                else if (!drive.Name.Equals(newConnectedDrives[i].Name))
                {
                    removed = true;
                    i--;
                }
                if (removed)
                {
                    DriveChangeEvent dce = new DriveChangeEvent(DriveChangeType.DRIVE_OUT, drive);
                    control.HandleDriveChange(dce);
                    Console.WriteLine(drive.Name + " removed.");
                }
                i++;
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
