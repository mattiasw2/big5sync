

These are the current known issues with Syncless 1.0, and will be handled at a later version. For a list of how Syncless handles certain unique sequence of actions, please look at [here](http://code.google.com/p/big5sync/wiki/SpecialCases).

Note: As much as we try to keep this page updated, please take a look at the project's [issue tracker](http://code.google.com/p/big5sync/issues/list) before reporting any bugs, thank you.

# General #

## Cannot Handle Tagging/Untagging from Context Menu During Synchronization ##

During synchronization, tagging/untagging a folder from context menu will cause program to stop responding.

## Errors Accessing Some Hard Disks ##

There might be errors accessing some hard disks. It could be due to the NTFS configuration issues.

## Unable to Synchronize After An Error Has Occurred Previously ##

Sometimes program does not perform synchronization after an error has occurred previously (in seamless mode). However, switching the synchronization mode can let program resume operation.

## Exception Thrown At Program Start Up Randomly ##

Sometimes unhandled exception is thrown at program start up. Apparently, it is due to multiple threads accessing the same tag list. Attempted to fix by returning a read-only tag list to methods which access it.

## Some Conflicting Filters Not Handled ##

As of 1.0, all conflicting filters are handled except "**.**". Looking into better ways
to implement it in 2.0.

## Cannot Sync File With Very Long Name ##

As of version 1.0, the file/folder with PathTooLongException will simply not be synced. No error will be thrown. Will try to implement a notification to inform user of such error in future releases.

## Unable to Handle External Disk With Not Enough Space ##

If an external disk runs out of space during synchronization in seamless mode, program will encounter problem.

# Profiling #

## Unable to Load Profile from Protected Removable Drive ##

Syncless cannot access some of the protected removable drives to retrieve guid.id or write metadata into the drive.

# Tagging #

## Recursive Tagging ##

If Folder A is tagged with Folder D and Folder B is tagged with Folder C, a recursion will occur and folders will be created in them until they hit the maximum file path length.
> ![http://big5sync.googlecode.com/files/RecursiveTagging.jpg](http://big5sync.googlecode.com/files/RecursiveTagging.jpg)

# CompareAndSync #

## Slow Progress During Finalizing ##

When CompareAndSync updates metadata files at the finalizing stage, it will take quite a while. Will optimize the algorithm later at version 1.5.

## UnauthorizedAccessException Encountered ##

Sometimes an UnauthorizedAccessException is thrown during a manual
sync/compare job. As of now, it is not handled since it occurs very rarely,
and are in the process of implementing a better system.

# Monitor #

## Errors Detecting Tagged Folder Deletion in Seamless Mode ##

The result of this is random. Sometimes UnauthorizedAccessException will be thrown while in some occasions program works fine.

## Drive Plugin Unstable ##

Most of the time, program can detect drive plugin event. Once in a while, such event is not detected.

## Seamless Synchronization Limitation ##

Due to limitation of the `FileSystemWatcher`, when performing mass filesystem operation within a short period of time in seamless synchronization, some files/folders may not be propagated.

## Additional Files Created ##

Due to a delay in receiving events, sometimes additional files will be created when performing filesystem operation within a short period of time in seamless synchronization.

For example, Folder A and Folder B both containing 1.txt file of size 3MB are tagged to the same tag. Rename 1.txt to 2.txt in Folder A. Before this change takes effect in Folder B, the system detects that 1.txt exists in Folder B but not in Folder A, so an event is queued to create 1.txt in Folder A. After creating 1.txt in Folder A, the system also detects that 1.txt does not exist in Folder B, so creates 1.txt in Folder B. As such, eventually, 1.txt and 2.txt will both exist in Folder A and Folder B.

# User Interface #

## Preview Button Shown During Synchronization ##

During synchronization in manual mode, the preview button is shown. Though it does not affect the flow of synchronization, may consider removing it in future releases.

## Manual Button Changed to Switching Button When Analyzing ##

This is a very rare case which cannot be reproduced subsequently. When program is analyzing tagged folders, the manual button is changed to switching button.

## Changes Made After Every Sync Not Shown ##

Currently, the program does not show a list of changes made to the filesystem after every synchronization.

## Tag Properties Panel ##

The tag properties panel only contains the filter property as of version 1.0. It may be confusing to the user at first when he tries to create filters. It is implemented this way so that it is easier to extend the properties panel in future releases.

## Tagging From Shell During Synchronizing ##
Tagging from the shell (Right-click on a folder) and tagging/untagging while any tag is synchronizing will cause the program to go into a hang state. After some time without interference when all the synchronization operations have been completed, the tagging/untagging window might be shown.

## Unable to detect U3 Partitions ##
Syncless is unable to detect U3 partitions on a thumbdrive. This means that features like tagging and unmonitoring will not be available for it.

## Datagrid Rendering Issue ##
Slight rendering issue on Log Window. The log table might automatically resize after scrolling the window, most likely due to a custom component bug.

## Tag Filter Not Applied After Tag List Refreshing ##
Tag Filter does not apply after the tag list has been refreshed. This means even if there is a filter, the tag list will not be filtered after refreshing.

## Indeterminate Synchronization Progress Updates ##
Sometimes, the status and progress bar might be stuck at finalizing during synchronization. Progress bar colour might also not correspond to the approriate percentage (eg. 100% is indicated at yellow when it is supposed to be green). This does not occur very often, and work has been started on fixing it.

## Seamless Mode Button Stuck At Switching ##

Sometimes the mode is stuck at switching due to some problem when dragging
multiple folders into program main window. It is very random and hard to reproduce.