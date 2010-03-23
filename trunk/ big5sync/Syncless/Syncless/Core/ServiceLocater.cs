using System;
using Syncless.Core.Exceptions;
using Syncless.Helper;
using Syncless.Logging;
using Syncless.Notification;

namespace Syncless.Core
{
    public class ServiceLocator
    {
        public const string USER_LOG = "user";
        public const string DEBUG_LOG = "debug";

        public static IUIControllerInterface GUI{
            get { 
                return SystemLogicLayer.Instance; 
                //return null;
            }
        } 
        public static IMonitorControllerInterface MonitorI
        {
            get { 
                return SystemLogicLayer.Instance;
                //return null;
            }
        }
        
        public static ICommandLineControllerInterface CommandLine
        {
            get { return SystemLogicLayer.Instance; }
        }
                
        public static Logger GetLogger(string type)
        {
            if (type.Equals(USER_LOG))
            {
                SystemLogicLayer sll = SystemLogicLayer.Instance;
                return sll.GetLogger(USER_LOG);
            }
            else if (type.Equals(DEBUG_LOG))
            {
                SystemLogicLayer sll = SystemLogicLayer.Instance;
                return sll.GetLogger(DEBUG_LOG);
            }
            else
            {
                throw new LoggerNotFoundException(ErrorMessage.LOGGER_NOT_FOUND);
            }
        }


        public static INotificationQueue UINotificationQueue()
        {
            return SystemLogicLayer.Instance.UiNotification;
        }
        public static INotificationQueue LogicLayerNotificationQueue()
        {
            return SystemLogicLayer.Instance.SllNotification;
        }
        public static INotificationQueue UIPriorityQueue()
        {
            return SystemLogicLayer.Instance.UiPriorityNotification;
        }


    }
}
