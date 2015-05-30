

# Overview #

The **Notification** component provides a low coupling medium for the control layer to pass information to the **User Interface**. It also allows the lower facade layer to pass information to the Logic layer.

The main reason for this implementation is to have the message come in a particular order for processing and at the same time, allow the other functions in the system to work asynchronously.

# Classes #

The main classes for Notification include `NotificationQueue` and `INotificationQueue`. Many notifications are implemented in both UI and **System Logic Layer**. At UI layer, it needs to implement watchers to watch all notifications which belong to the `Notification.UINotification` namespace.

## Notifications for User Interface ##
  1. `AutoSyncCompleteNotification` - Informs the User Interface that a seamless synchronization request is completed.
  1. `CancelSyncNotification` - Informs the User Interface that a synchronization request is cancelled.
  1. `NothingToSyncNotification` - Informs the User Interface that there is nothing to synchronize.
  1. `SyncCompleteNotification` - Informs the User Interface that the synchronization is completed.
  1. `SyncStartNotification` - Informs the User Interface that the synchronization is started.

## Notifications for Logic Layer ##
  1. `AddTagNotification` - Informs the Logic Layer to add a tag. Used during merging of tagging profiles.
  1. `MonitorPathNotification` - Informs the Logic Layer to start monitoring a path. Called after switching from manual to seamless mode.
  1. `MonitorTagNotification` - Informs the Logic Layer to start monitoring a tag. Used during merging of tags.
  1. `RemoveTagNotification` - Informs the Logic Layer to remove a tag. Used during removing of a tag.
  1. `SaveNotification` - Informs the Logic Layer to perform a save on Xml files. Used when `SystemLogicLayer` is initiated, terminated or when there are changes made to the file system.
  1. `TaggedFolderDeletedNotification` - Informs the Logic Layer that a tagged folder is deleted. Used during merging of tagging profiles.
  1. `TaggedPathDeletedNotification` - Informs the Logic Layer that a tagged path is deleted. Used during merging of tagging profiles.
  1. `UnMonitorPathNotification` - Informs the Logic Layer to start unmonitoring a path. Called after switching from manual to seamless mode.

# Description of Design #

## Observer Pattern ##
The Notification component makes use of the observer pattern to notify its watchers to resume their threads if new notifications are added to the queue.

## Factory Pattern ##
Factory pattern is also used to create notification objects. A `NotificationFactory` class provides methods to create such notification objects such that whichever method that performs the call does not need to know the exact class that is used to create the object.

# Extending Notification #
To include more notifications, developers can add more enumerators to the `NotificationCode` enumeration and extend the `AbstractNotification` class. The developer should also add a Create method for the notification inside `NotificationFactory`.