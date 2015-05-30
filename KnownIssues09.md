

These are the current known issues with Syncless 0.9, and will be handled at a later version. For a list of how Syncless handles certain unique sequence of actions, please look at [here](SpecialCases.md).

Note: As much as we try to keep this page updated, please take a look at the project's [issue tracker](http://code.google.com/p/big5sync/issues/list) before reporting any bugs, thank you.

# General #
## Non-Responsive When Manual Syncing A Lot of Large Files ##
When doing a manual synchronization, and at least one of the folders has a lot of large files, Syncless seems to not be doing anything. In actual fact, it is busy comparing and determining the files for you. We will implement a feedback system for the next release. [Issue #47](http://code.google.com/p/big5sync/issues/detail?id=47)

# Profiling #
## Corrupted/Non-Standard XML Format ##
If the XML is corrupted due to user interference, Syncless will have erratic behaviour. The next version will detect this and handle it properly. [Issue #33](http://code.google.com/p/big5sync/issues/detail?id=33)

## Unable to Load Profile ##
Sometime profile is not able to be load from some removable drive due to the read and write access. The next patch should fix this.
[Issue #39](http://code.google.com/p/big5sync/issues/detail?id=39)

# Tagging #
## Recursive Tagging ##
If Folder A is tagged with Folder D and Folder B is tagged with Folder C, a recursion will occur and folders will be created in them until they hit the maximum file path length.

![http://big5sync.googlecode.com/files/RecursiveTagging.jpg](http://big5sync.googlecode.com/files/RecursiveTagging.jpg)

## Tags Not Saved When Program Is Not Terminated Correctly ##
Currently, Syncless only saves on proper termination. We are aware of this and will fix it at the next release. [Issue #37](http://code.google.com/p/big5sync/issues/detail?id=37)

[Issue #6](http://code.google.com/p/big5sync/issues/detail?id=6)

## Tag Merging from Thumb Drive to Computer Does Not Happen Sometimes ##
Syncless has the ability to merge all the tags on the thumb drive with that on the computer. However, there are times when this does not happen. [Issue #30](http://code.google.com/p/big5sync/issues/detail?id=30)


# CompareAndSync #
## Long Time To Sync When Dealing With Large File Sizes In Manual Mode, Or On Program Startup ##
This is due to the MD5 hash used when building the tree for comparison and for writing to metadata, we will look into implementing CRC32 or Adler32 as well, although initial trials with Adler32 proved little to no improvement, as the main bottleneck is actually disk I/O. [Issue #34](http://code.google.com/p/big5sync/issues/detail?id=34)

## Path Too Long To Sync ##
If a user tags a folder which has a path that is too long, Syncless currently does not handle it. The main reason is because we have yet to implement a proper notification system. [Issue #8](http://code.google.com/p/big5sync/issues/detail?id=8)

## Re-creating of Deleted Tagged Folders (Manual) ##
If you have a folder that is tagged, and you delete it, Syncless will re-create it the next time you synchronize in manual mode. Again, this is because we have yet to implement a proper notification system. [Issue #19](http://code.google.com/p/big5sync/issues/detail?id=19)

## Deletion of Tagged Folders in Seamless Mode ##
If you have a few folders that are tagged in seamless mode, deleting one of these tagged folders will cause the contents of the other tagged folders to be deleted. [Issue #19](http://code.google.com/p/big5sync/issues/detail?id=19)

## Renamed Files Not Getting Propagated ##
This will only happen if you create, delete and then create again with the same filename, followed by a rename, all within a short span of time. This cannot be fixed as far as we know due to how Windows handles the file system. [Issue #22](http://code.google.com/p/big5sync/issues/detail?id=22)

## Renaming More Than 3 Levels In A Directory Structure ##
If you have the following C:\A\B\C\D, and you rename it to C:\A\E\F\G, the synchronization will be erratic. Similarly, if the renaming is too messy, the manual synchronization may go wrong. As a temporary workaround, doing a sync after the initial sync will cause all folders to be in sync again, and after which you can delete or move the files to your liking. [Issue #38](http://code.google.com/p/big5sync/issues/detail?id=38)

## Corrupted Meta Data Due to Termination of File Copying Process ##
When you try to copy a large file from a external hard disk to a local folder, the copying process might hang and corrupt the meta data XML document. [Issue #41](http://code.google.com/p/big5sync/issues/detail?id=41)

## Does not detect that the drive is full ##
During synchronization, if the drive is full, Syncless is unable to detect the lack of disk space.
[Issue #42](http://code.google.com/p/big5sync/issues/detail?id=42)

## File to Sync with Folder ##
If a file is named file.txt and a folder is named file.txt, Syncless does not handle this unique case.
[Issue #43](http://code.google.com/p/big5sync/issues/detail?id=43)

## Opening some file in a folder that is monitored ##
If a file is open in a folder that is tag and is in seamless mode , seamless mode will behave in a unexpected way.
[Issue #45](http://code.google.com/p/big5sync/issues/detail?id=45)

## Hash fail ##
The program will throw a error if a file that is being hash is currently locked.
[Issue #44](http://code.google.com/p/big5sync/issues/detail?id=44)

## Seamless Mode Issues ##
Please refer to the Monitor section.

# Monitor #
## Handling of File System Events ##
Due to the way FileSystemWatcher is implemented in C#, we are unable to handle certain cases by default. As such, we have added some delay and additional logic on top of FileSystemWatcher to ensure that files and folders are propagated properly. However, this results in "unpredictable" effects when a user does a sequence of actions too quickly. An example would be following:

  1. Tag at least 2 folders under the same tag, make sure seamless mode is on
  1. Copy a folder contain about 100 folders, at least 2MB to 3MB each to one of these tagged folders so that it will take a while to propagate over
  1. While propagation is still happening, delete the folder in one of the tagged folders.
  1. You will observe that the folder may be recreated and repopulated.

The current workaround for this is to wait about 10 seconds before doing the next action. We are currently looking into fixing it. [Issue #11](http://code.google.com/p/big5sync/issues/detail?id=11) [Issue #21](http://code.google.com/p/big5sync/issues/detail?id=21)


# User Interface #
## Shortcut Keys Do Not Work ##
Under certain circumstances, the shortcut keys will stop working. One guaranteed way to reproduce this is to do a "Ctrl + T", followed by any other shortcut key. After that, all shortcut keys will stop working until you click on one of the buttons. [Issue #26](http://code.google.com/p/big5sync/issues/detail?id=26)

## Folder Dialog Does Not Show Certain Folders ##
The folder dialog may not show certain folders such as "My Computer" in certain cases. We are still looking into this issue. So far, we have only observed this on Windows Vista. [Issue #32](http://code.google.com/p/big5sync/issues/detail?id=32)

## Removable Storage Drive Not Displayed For Removable ##
In Syncless, there is a button that allows for the safety removal of removable drives. In some cases, the drives will not be displayed. [Issue #31](http://code.google.com/p/big5sync/issues/detail?id=31)

## Eject Does Not Remove Drive From Application ##
The "Eject" button simply tells Syncless to stop actively monitoring the drive, so that you can proceed to use the "Safely Remove" feature in Windows. We are looking at ways to implement safely removal. [Issue #28](http://code.google.com/p/big5sync/issues/detail?id=28)

## Main Window Minimized to Tray After Tagging ##
Sometimes after the user tag a folder, the main window will be minimized to
tray. User may not notice it and will have problem figuring out where has the
main window gone to. [Issue #48](http://code.google.com/p/big5sync/issues/detail?id=48)