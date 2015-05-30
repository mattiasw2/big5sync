`[`**[Prev - Syncless.Logging](DeveloperAPILogging.md)** `|` **[Next - Syncless.Monitor.DTO](DeveloperAPIMonitorDTO.md)**`]`

Provides classes which perform monitoring of real-time events in file system.

# Classes Summary #

| | **Class** | **Description** |
|:|:----------|:----------------|
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [ExtendedFileSystemWatcher](#ExtendedFileSystemWatcher.md) | An Extension to System.IO.FileSystemWatcher.  Added a CreateComplete event to inform the user when a file has completed creating.  Reference from http://geekswithblogs.net/thibbard/articles/ExtendingFileSystemWatcher.aspx |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [FileSystemEventDispatcher](#FileSystemEventDispatcher.md) | The holding place for all the events from the System.IO.FileSystemWatcher and Syncless.Monitor.ExtendedFileSystemWatcher.  This will help to keep the code for handling an event fired from the 2 Watcher short, so as to prevent their internal buffer from overflowing.  The events will then be dispatched to Syncless.Monitor.FileSystemEventProcessor to process after an idle time of 1000 milliseconds.  Reference from http://csharp-codesamples.com/2009/02/file-system-watcher-and-large-file-volumes/ |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [FileSystemEventExecutor](#FileSystemEventExecutor.md) | Pushes all received events for execution thru Syncless.Core.IMonitorControllerInterface. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [FileSystemEventProcessor](#FileSystemEventProcessor.md) | Attempts to combine a series of events into a single event as far as possible.  The events will then be dispatched to Syncless.Monitor.FileSystemEventExecutor to request for execution of the events thru Syncless.Core.IMonitorControllerInterface. |

## ExtendedFileSystemWatcher ##

[Back to top](#Classes_Summary.md)

## FileSystemEventDispatcher ##

[Back to top](#Classes_Summary.md)

## FileSystemEventExecutor ##

[Back to top](#Classes_Summary.md)

## FileSystemEventProcessor ##

[Back to top](#Classes_Summary.md)

## MonitorLayer ##

[Back to top](#Classes_Summary.md)

`[`**[Prev - Syncless.Logging](DeveloperAPILogging.md)** `|` **[Next - Syncless.Monitor.DTO](DeveloperAPIMonitorDTO.md)**`]`