

These are the current known issues with Syncless 2.0, and will be handled at a later version. For a list of how Syncless handles certain unique sequence of actions, please look at [here](http://code.google.com/p/big5sync/wiki/SpecialCases).

Note: As much as we try to keep this page updated, please take a look at the project's [issue tracker](http://code.google.com/p/big5sync/issues/list) before reporting any bugs, thank you.

# General #

## Errors Accessing Some Hard Disks ##

There might be errors accessing some hard disks. See [Issue 40](http://code.google.com/p/big5sync/issues/detail?id=40) for more information.

## Exception Thrown At Program Start Up ##

Sometimes unhandled exception is thrown at program start up. We are still looking into it. Refer to [Issue 185](http://code.google.com/p/big5sync/issues/detail?id=185).

## Cannot Sync File With Very Long Name ##

As of version 2.0, the file/folder with PathTooLongException will simply not be synced. No error will be thrown. Will try to implement a notification to inform user of such error in future releases. The user may view the logs to see what is happening. Refer to [Issue 8](http://code.google.com/p/big5sync/issues/detail?id=8).

## Unable To Handle Disk With Not Enough Space ##

As of version 2.0, when a disk runs out of space, synchronizing will complete the synchronization, without hanging or crashing. The user may view the logs to see what is happening. Refer to [Issue 8](http://code.google.com/p/big5sync/issues/detail?id=8).

# Tagging #

## Recursive Tagging ##
If Folder A is tagged with Folder D and Folder B is tagged with Folder C, a recursion will occur and folders will be created in them until they hit the maximum file path length.
> ![http://big5sync.googlecode.com/files/RecursiveTagging.jpg](http://big5sync.googlecode.com/files/RecursiveTagging.jpg)

Refer to [Issue 6](http://code.google.com/p/big5sync/issues/detail?id=6).

# CompareAndSync #
## Finalizing Can Be Optimized ##
Finalizing can be optimized further. Refer to [Issue 178](http://code.google.com/p/big5sync/issues/detail?id=178).

# Monitor #
## Drive Plugin May Not Be Detected ##
Syncless will detect drive plugin events most of the time. However, from time to time, Syncless will not be able to catch such an event. Although it is very rare, it happens. If it happens , we suggest the user restarts Syncless, or remove the drive and plug in again. Refer to [Issue 192](http://code.google.com/p/big5sync/issues/detail?id=192) for more information.

## Removable Drive ##
Removing a drive while Syncless is synchronizing to it will cause finalizing to take a very long time. Also, Syncless may end unexpectedly. Refer to [Issue 204](http://code.google.com/p/big5sync/issues/detail?id=204).

## Seamless Synchronization Limitation ##
Due to limitation of the `FileSystemWatcher`, when performing mass filesystem operation within a short period of time in seamless synchronization, some files/folders may not be propagated. Refer to [Issue 179](http://code.google.com/p/big5sync/issues/detail?id=179).

# User Interface #
## Progress Bar Remains On Screen After Tags Are Removed ##
Sometime progress bar still remains on screen after tags are removed. Refer to [Issue 201](http://code.google.com/p/big5sync/issues/detail?id=201).

## Changes Made After Every Sync Not Shown ##
Currently, the program does not show a list of changes made to the filesystem after every synchronization. Refer to [Issue 148](http://code.google.com/p/big5sync/issues/detail?id=148).

## Datagrid Rendering Issue ##
Slight rendering issue on Log Window. The log table might automatically resize after scrolling the window, most likely due to a custom component bug.

## No Notification When Cancelling/Switching To Seamless Mode ##
Refer to [Issue 203](http://code.google.com/p/big5sync/issues/detail?id=203).