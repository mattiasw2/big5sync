

# Overview #
The **Monitor** component provides a set of classes to detect file system changes. There are 2 roles for the Monitor component. The first role is to detect the insertion and removal of USB removable drives, such as external hard disks and flash drives, and the second role is to capture any changes to a file or folder within a tagged folder.

# Classes #
![![](http://big5sync.googlecode.com/files/Tagging%20-%20v2%20-%20thumb.png)](http://big5sync.googlecode.com/files/Monitor%20-%20v2.png)

# Description of Design #

## Detecting USB Removable Drive Events ##
This is implemented using Windows Management Instrumentation. The code for the detection of USB devices is referenced from:  [here](http://dotnetslackers.com/community/blogs/basharkokash/archive/2008/03/15/USB-Detection-source-code.aspx). When the application starts, all the currently connected drives are recorded. Whenever a USB event is triggered, we will find out the new list of connected drives and compare them with the last records. From the result, we will be able to tell which drives are being inserted or removed. `DeviceWatcher` will handle the detection of USB devices.

## Capturing File/Folder Change Events ##
This is implemented using `FileSystemWatcher` from the C# library. The `FileSystemWatcher` is extended to handle extra cases that the original `FileSystemWatcher` cannot handle.

## Problems with `FileSystemWatcher` ##
`FileSystemWatcher` has the following problems:
  1. When using the `FileSystemWatcher` to monitor a path, any operation on the folder which the path points to, will not be reported by the `FileSystemWatcher`.
  1. When creating a large file, the file creation event is triggered before the file has finished the actual creation. Exceptions will occur when we attempt to perform some operations on the new file before it finishes creation.
  1. When `FileSystemWatcher` is working with large volumes of file, it is unreliable. The reason is that there is a fixed buffer allocated to each `FileSystemWatcher` which is used to store information on the events raised. When a large number of files raises an event, this buffer gets full and some events might not be captured.
  1. Different applications have different ways of handling a file/folder operation. Temporary files are sometimes used and this will cause some problems when we do a sync. Exception will occur when trying to sync a temporary file when the file has already been deleted.

# Explanation of Algorithms #
For monitoring path, the algorithm will ensure that only the top most parent path will be assigned a `FileSystemWatcher`. All existing watcher for the children path will be removed. This is to prevent receiving duplicated operation events.

To solve problem 1, an additional watcher will be created and assigned to monitor its parent folder but only operations on those folders assigned to be monitored will be reported. The algorithm for this additional watcher will also ensure that only the top most parent path will be monitored.

To solve problem 2, we reference the codes from [here](http://geekswithblogs.net/thibbard/articles/ExtendingFileSystemWatcher.aspx). This is done by extending the `FileSystemwatcher` and adding new events to trigger a complete creation event.

To solve problem 3, we reference the codes from [here](http://csharp-codesamples.com/2009/02/file-system-watcher-and-large-file-volumes/). Instead of writing our logic in the handler, we will dispatch the sequence of events to a queue which is processed by a separate thread. As such, the only work done by the handler is to dispatch the events to the queue, so that it can release the buffer and start monitoring more events.

To solve problem 4, we thought of an algorithm to transfer all the events currently in the waiting queue to another process queue if there is an idle time of 1 second with no new events added to the waiting queue. This process queue will look through all the events and produce the simplest form of events to be synchronized. The delay in the algorithm is necessary due to the way `FileSystemWatcher` works.

If there is any operation on a locked file, the algorithm will skip this file and process the next file and then try to sync the locked file again. If there is no next file to sync, the algorithm will instead wait for some time and then attempt to sync again.

# Extending Monitor #
If there are other custom events which require `FileSystemWatcher` to trigger, simply extend `ExtendedFileSystemWatcher` and replace the current `ExtendedFileSystemWatcher` calls to the newly extended class.