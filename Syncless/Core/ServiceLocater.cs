/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using Syncless.Core.Exceptions;
using Syncless.Helper;
using Syncless.Logging;
using Syncless.Notification;

namespace Syncless.Core
{
    /// <summary>
    /// The class provide the interface to get all the services provided by Syncless Logic Layer.
    /// </summary>
    public class ServiceLocator
    {
        /// <summary>
        /// User Log
        /// </summary>
        public const string USER_LOG = "user";
        /// <summary>
        /// Debug Log
        /// </summary>
        public const string DEBUG_LOG = "debug";
        /// <summary>
        /// Developer log
        /// </summary>
        public const string DEVELOPER_LOG = "developer";

        /// <summary>
        /// Return IUIControllerInterface
        /// </summary>
        public static IUIControllerInterface GUI{
            get { 
                return SystemLogicLayer.Instance; 
                //return null;
            }
        } 
        /// <summary>
        /// Return the IMonitorControllerInterface
        /// </summary>
        public static IMonitorControllerInterface MonitorI
        {
            get { 
                return SystemLogicLayer.Instance;
            }
        }
        /// <summary>
        /// Return ICommandLineControllerInterface
        /// </summary>
        public static ICommandLineControllerInterface CommandLine
        {
            get { return SystemLogicLayer.Instance; }
        }
        /// <summary>
        /// Return the Logger of a particular type
        /// </summary>
        /// <param name="type">The type of logger.</param>
        /// <exception cref="LoggerNotFoundException">The type of logger is not found.</exception>
        /// <returns>The logger of the particular type</returns>
        public static Logger GetLogger(string type)
        {
            Logger log = SystemLogicLayer.Instance.GetLogger(type);
            if (log != null)
            {
                return log;
            }
            else
            {
                throw new LoggerNotFoundException(ErrorMessage.LOGGER_NOT_FOUND);
            }
        }
        /// <summary>
        /// Get the UINotification Queue
        /// </summary>
        /// <returns>UINotiification Queue</returns>
        public static INotificationQueue UINotificationQueue()
        {
            return SystemLogicLayer.Instance.UiNotification;
        }
        /// <summary>
        /// Get the Logic Layer Notification Queue
        /// </summary>
        /// <returns>System Logic Layer Queue</returns>
        public static INotificationQueue LogicLayerNotificationQueue()
        {
            return SystemLogicLayer.Instance.SllNotification;
        }
        /// <summary>
        /// Get the UIPriority Queue
        /// </summary>
        /// <returns>UIPriorityQueue</returns>
        public static INotificationQueue UIPriorityQueue()
        {
            return SystemLogicLayer.Instance.UiPriorityNotification;
        }


    }
}
