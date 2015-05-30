

# Introduction #

Syncless will, as far as we can, never throw a synchronization conflict since that contradicts our vision of seamless synchronization. This page details the unusual combination of actions as well as how Syncless will handle each case. For all cases, please assume that at least 3 folders are tagged under the same tag.

For a list of known issues with certain special cases, please look [here](http://code.google.com/p/big5sync/wiki/KnownIssues09).

# Details #

## Manual Mode ##

### Renaming A File In One Of The Tagged Folders ###
If a user renames a file, the rename will be propagated across all the folders.

### Renaming A File In More Than One of The Tagged Folders ###
Assuming a user has the file "Lecture15.pdf" in all of the tagged folders. If he renames it to "Comm Skills.pdf" in one and "Communication and Skills.pdf" in another, Syncless will not propagate the rename, and instead, will treat each of them as new files and propagate them across all the tagged folders. The end result will be all the tagged folders having all 3 files ("Lecture15.pdf", "Comm Skills.pdf", "Communication and Skills.pdf").

### Renaming A Folder In One Of The Tagged Folders ###
If a user renames a folder within one of the tagged folders, Syncless will propagate this folder rename to the other folders. Most of the commercial tools currently available will handle this as a "Delete" followed by a "Copy" event, which is infeasible if the folder being renamed has updates or contain large amount of files.

### Renaming A Folder In More Than One Of The Tagged Folders, And Adding Or Updating Files In It ###
Similar to the above case, Syncless will propagate this folder rename across the other folders. At the same time, it is also able to merge the contents of these differently-named folders, and the end result will be all the tagged folders having the new folder name, as well as the most updated version of the files.

### Renaming A Folder In More Than One Of The Tagged Folders ###
Assume the user has a "Music" folder in all the folders you have tagged "Media". If a user renames "Music" to "Songs" in one of these tagged folders and "Music" to "Mp3" in another of these folders, Syncless will handle this by propagating all 3 folders ("Media", "Music", "Songs") to all other folders tagged "Media".

The reason for doing so is because it is in actual fact, a rename conflict, but more importantly, it is highly probable that the user wants to keep different things in them, and thus the conflicting renames.

### Renaming Multiple Levels ###
Assume the initial folder structure is "A\B\C", and the user renames it to "1\2\3". Syncless is able to detect N-levels of rename will attempt to merge and rename accordingly at each level.

### Conflicts ###
In Syncless, our vision is to as much as possible, discard the idea of conflicts. However, there are 2 cases which we cannot automatically resolve, and they are:

  1. Incompatible Type Conflict - This happens when you have a file and a folder with the same name in two or more different folders. For example, C:\A\Temp, where Temp is a file, and C:\B\Temp, where Temp is a folder, and you attempt to synchronize C:\A and C:\B.
  1. Date Time Conflict - This happens when you have 2 or more files of different hash or length, but with the same name and last modified time. In such a case, it is not possible to determine which to use.

For the **Incompatible Type Conflict**, Syncless will automatically prioritize the folder over the file, and all the files will be thrown into a "`_`synclessConflict" folder, while the folders will be synchronized as usual.

For **Date Time Conflict**, Syncless will automatically synchronize the first file it comes across, and all the others will be thrown into a "`_`synclessConflict" folder.

In both cases, we guarantee that no data will be lost.