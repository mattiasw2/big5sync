`[`**[Prev - Syncless.Monitor.Exceptions](DeveloperAPIMonitorExceptions.md)** `|` **[Next - Syncless.Profiling](DeveloperAPIProfiling.md)**`]`

Provides classes which enclose information needed to send notification among classes and between main logic layer and user interface layer.

# Interfaces Summary #

| | **Interface** | **Description** |
|:|:--------------|:----------------|
| ![http://big5sync.googlecode.com/files/pubinterface.gif](http://big5sync.googlecode.com/files/pubinterface.gif) | [INotificationQueue](#INotificationQueue.md) | Interface for implementing a notification queue. Classes which implement the interface must implement Enqueue, Dequeue, AddObserver and HasNotification methods. |
| ![http://big5sync.googlecode.com/files/pubinterface.gif](http://big5sync.googlecode.com/files/pubinterface.gif) | [IQueueObserver](#IQueueObserver.md) | Interface which provides a mechanism for receiving notifications from classes implementing INotificationQueue interface. |
| ![http://big5sync.googlecode.com/files/pubinterface.gif](http://big5sync.googlecode.com/files/pubinterface.gif) | [ISyncProgressObserver](#ISyncProgressObserver.md) | Interface to observe a synchronization progress. |

## INotificationQueue ##

[Back to top](#Interfaces_Summary.md)

## IQueueObserver ##

[Back to top](#Interfaces_Summary.md)

## ISyncProgressObserver ##

[Back to top](#Interfaces_Summary.md)

# Classes Summary #

| | **Class** | **Description** |
|:|:----------|:----------------|
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [AbstractNotification](#AbstractNotification.md) | The abstract parent file for all notification. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [AddTagNotification](#AddTagNotification.md) | The notification for adding a Tag. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [AutoSyncCompleteNotification](#AutoSyncCompleteNotification.md) | The notification for automatic synchronization completed. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [CancelSyncNotification](#CancelSyncNotification.md) | The notification for cancelling a sync request. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [MessageNotification](#MessageNotification.md) | The notification for sending message. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [MonitorPathNotification](#MonitorPathNotification.md) | The notification to inform logic to monitor a path. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [MonitorTagNotification](#MonitorTagNotification.md) | The notification to inform SLL to monitor a tag. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [NothingToSyncNotification](#NothingToSyncNotification.md) | The notification to inform the User Interface that there is nothing to synchronize. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) ![http://big5sync.googlecode.com/files/static.gif](http://big5sync.googlecode.com/files/static.gif) | [NotificationFactory](#NotificationFactory.md) | Provides abstraction to create instances of classes which extend the AbstractNotification class. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [NotificationQueue](#NotificationQueue.md) | Provides implementation for Enqueuing and DeQueuing of Notification. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [PreviewProgress](#PreviewProgress.md) | The class representing a preview progress. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [Progress](#Progress.md) | The abstract class for Progress. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [RemoveTagNotification](#RemoveTagNotification.md) | The notification for removing tag. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [SaveNotification](#SaveNotification.md) | The nofication for saving all the data. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [SyncCompleteNotification](#SyncCompleteNotification.md) | The nofication for Sync Complete. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [SyncProgress](#SyncProgress.md) | The class representing a synchronization progress |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [SyncStartNotification](#SyncStartNotification.md) | The notification to the User Interface that a Sync Request have been accepted and started. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [TaggedFolderDeletedNotification](#TaggedFolderDeletedNotification.md) | The notification for a tagged folder deleted. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [TaggedPathDeletedNotification](#TaggedPathDeletedNotification.md) | The notification for a tagged path deleted. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [UnMonitorPathNotification](#UnMonitorPathNotification.md) | The notification for unmonitor path. |

## AbstractNotification ##

[Back to top](#Classes_Summary.md)

## AddTagNotification ##

[Back to top](#Classes_Summary.md)

## AutoSyncCompleteNotification ##

[Back to top](#Classes_Summary.md)

## CancelSyncNotification ##

[Back to top](#Classes_Summary.md)

## MessageNotification ##

[Back to top](#Classes_Summary.md)

## MonitorPathNotification ##

[Back to top](#Classes_Summary.md)

## MonitorTagNotification ##

[Back to top](#Classes_Summary.md)

## NothingToSyncNotification ##

[Back to top](#Classes_Summary.md)

## NotificationFactory ##

[Back to top](#Classes_Summary.md)

## NotificationQueue ##

[Back to top](#Classes_Summary.md)

## PreviewProgress ##

[Back to top](#Classes_Summary.md)

## Progress ##

[Back to top](#Classes_Summary.md)

## RemoveTagNotification ##

[Back to top](#Classes_Summary.md)

## SaveNotification ##

[Back to top](#Classes_Summary.md)

## SyncCompleteNotification ##

[Back to top](#Classes_Summary.md)

## SyncProgress ##

[Back to top](#Classes_Summary.md)

## SyncStartNotification ##

[Back to top](#Classes_Summary.md)

## TaggedFolderDeletedNotification ##

[Back to top](#Classes_Summary.md)

## TaggedPathDeletedNotification ##

[Back to top](#Classes_Summary.md)

## UnMonitorPathNotification ##

[Back to top](#Classes_Summary.md)

# Enumerations Summary #

| | **Enumeration** | **Description** |
|:|:----------------|:----------------|
| ![http://big5sync.googlecode.com/files/pubenum.gif](http://big5sync.googlecode.com/files/pubenum.gif) | NotificationCode | Provides the enumerations of notification type. |
| ![http://big5sync.googlecode.com/files/pubenum.gif](http://big5sync.googlecode.com/files/pubenum.gif) | SyncState       | Provides the enumerations of sync state. |

`[`**[Prev - Syncless.Monitor.Exceptions](DeveloperAPIMonitorExceptions.md)** `|` **[Next - Syncless.Profiling](DeveloperAPIProfiling.md)**`]`