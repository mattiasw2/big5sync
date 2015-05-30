

# Overview #
The **CompareAndSync** component in **Syncless** is the part of the system that handles the comparison and syncing of files and folders. From a logical point of view, it can be briefly split into 2 subcomponents; the manual subcomponent that handles all manual operations, and the seamless subcomponent that handles all the automated synchronization requests from another component, the **Monitor**.

Both sub-components are implemented very differently, as manual synchronization and automated synchronization are largely different. For the manual component, a **tree** is built containing the directory structure and files of all the folders to be kept in sync. Using the **Tree Visitor Pattern**, various **Visitors** will then "visit" the tree and do various tasks. A queue is used to ensure only one synchronization job can occur at any one time. On the other hand, each file/folder change is handled independently in the seamless subcomponent. Similarly, a queue is used to ensure only one synchronization job can occur at any one time.

## Conflicts ##
In Syncless, our vision is to as much as possible, discard the idea of conflicts. However, there are 2 cases which we cannot automatically resolve, and they are:

  1. Incompatible Type Conflict - This happens when you have a file and a folder with the same name in two or more different folders. For example, C:\A\Temp, where Temp is a file, and C:\B\Temp, where Temp is a folder, and you attempt to synchronize C:\A and C:\B.
  1. Date Time Conflict - This happens when you have 2 or more files of different hash or length, but with the same name and last modified time. In such a case, it is not possible to determine which to use.

For the **Incompatible Type Conflict**, Syncless will automatically prioritize the folder over the file, and all the files will be thrown into a "`_`synclessConflict" folder, while the folders will be synchronized as usual.

For **Date Time Conflict**, Syncless will automatically synchronize the first file it comes across, and all the others will be thrown into a "`_`synclessConflict" folder.

In both cases, we guarantee that no data will be lost.

# Classes #
![![](http://big5sync.googlecode.com/files/CompareAndSync%20-%20v2%20-%20thumb.png)](http://big5sync.googlecode.com/files/CompareAndSync%20-%20v2.png)


We will now take a look at the main components of CompareAndSync, which can be broadly divided to the controller layer, the manual component and the seamless component.


## Controller ##
**CompareAndSyncController** is a facade for the other layers to interact with, hiding the underlying implementations and exposing only the necessary methods for syncing.

### Enums, Exceptions, Data Transfer Objects (DTOs) and Entities ###
Enums, exceptions, DTOs and entities will not be discussed in detail in this guide. Please refer to the comments in the source code for more details. However, let us take a brief look at **Requests**, which are DTOs used in requesting for a manual synchronization or comparison job, or an auto synchronization job.

  * Request Namespace
    * AutoSyncRequest
    * CancelSyncRequest
    * ManualCompareRequest
    * ManualRequest
    * ManualSyncRequest
    * Request

**AutoSyncRequest** is sent when an automatic synchronization job is to be performed. **ManualCompareRequest** and **ManualSyncRequest** both inherit from the abstract class **ManualRequest**, and are sent when a manual comparison or synchronization is to be performed respectively. **CancelSyncRequest** is sent when a **ManualSyncRequest** is to be cancelled. **Request** is an abstract class from which all the other request classes inherit from.

## Manual Components ##
As stated in the overview, the manual portion of the CompareAndSync component is done using the Tree Visitor Pattern. The usage of this pattern in our implementation of CompareAndSync greatly enhances extensibility, as you will see later.

The classes of concern to us are:

  * CompareObject Namespace
    * BaseCompareObject
    * FileCompareObject
    * FolderCompareObject
    * RootCompareObject
    * XMLCompareObject
  * Visitor Namespace
    * BuilderVisitor
    * ComparerVisitor
    * ConflictVisitor
    * FolderRenameVisitor
    * IVisitor
    * ProcessMetadataVisitor
    * SyncerVisitor
    * XMLMetadataVisitor
    * XMLWriterVisitor
  * CompareObjectHelper
  * ManualQueueControl
  * ManualSyncer


### Entities ###
CompareObjects are entities which are used during manual comparison and synchronization. There are basically 2 main kinds of CompareObjects, the **FileCompareObject** and the **FolderCompareObject**. **RootCompareObject** is a specific kind of FolderCompareObject used to represent the root paths to be compared. There is also the **XMLCompareObject** which is used to store the file metadata and then used to populate the main CompareObjects.

### Details ###
When a request for synchronization is received, it will be enqueued into the single-instance **ManualQueueControl**. Once that is done, only one job will be dequeued at any one time, and processed. When a request for comparison or previewing is received, it will immediately be processed, since by default, only one comparison can be done at a time. In both cases, **ManualSyncer** will be called. ManualSyncer contains all the methods needed for comparison or synchronization of manual requests.

**CompareObjectHelper** is the class that contains the various traversal methods, namely, pre-traversal, post-traversal, and level-order traversal for traversing the tree. All the Visitor classes implement IVisitor interface.

#### Comparing ####
This portion is in charge of analyzing and generating data, without affecting any files or folders.

  1. **BuilderVisitor** takes in a RootCompareObject, and builds the directory trees for it under the given root paths.
  1. **XMLMetadataVisitor** will then visit this built tree, and populate it with metadata about each file, if the file exists.
  1. **ProcessMetadataVisitor** will then visit the tree and compare each file against its metadata, and update certain states if necessary.
  1. **FolderRenameVisitor** is a very specific class that will then check for folder renames.
  1. Next, **ComparerVisitor** visits the tree and updates the state of each CompareObject based on certain attributes like the last time the file was modified.

#### Syncing ####
From this portion onwards, actual changes will be made to the files and folders.

  1. Once all that is done, `HandleBuildConflicts`, a method in **ManualSyncer** will be called to handle build conflicts encountered during building of tree. **ConflictVisitor** will also be called to handle datetime conflicts.
  1. **SyncerVisitor** will then traverse the tree and carry out the necessary file or folder actions based on the states updated by ComparerVisitor, as well as update the final state of each folder and file.
  1. Finally, **XMLWriterVisitor** will traverse the tree, and write the updated information of each folder and file to XML.

Also, most of the the Visitors use pre-traversal, since it makes sense to build, populate and synchronize the CompareObjects from top-down. However, ComparerVisitor uses post-traversal because it is necessary to update the state of each folder if a file under it has changed. Also, FolderRenameVisitor uses level-order traversal, since it is necessary for folder merging to work properly.

Assuming a user wants to compare and analyze the results, but not do any actual synchronization, we can simply use the comparing without the syncing portion. It is also important to note that the Tree Visitor Pattern allows for very good modularity, and we can simply add on extra visitors to perform other tasks if necessary. Similarly, we can remove any visitors (except for BuilderVisitor, because it is required to build the tree), if necessary. A good example would be to remove FolderRenameVisitor if the developer does not want to handle folder renames.


## Seamless Components ##
CompareAndSync is made such that the seamless component is completely separated from the manual component. There are various design and implementation issues for this, with one of them being that the seamless component is largely depending on receiving information from other objects. Moreover, in seamless mode, the file to sync is already known, with little to no need of doing a comparison. As such, there is no need to do the tedious job of building a tree.

The classes of concern to us for the seamless portion are as follows:

  * XMLWriteObject Namespace
    * BaseXMLWriteObject
    * XMLWriteFileObject
    * XMLWriteFolderObject
  * SeamlessQueueControl
  * SeamlessSyncer
  * SeamlessXMLHelper

**SeamlessQueueControl** is a class that contains a single queue to ensure the requests are handled in order, so as to prevent any possible conflicts. **SeamlessSyncer** contains the necessary methods to handle seamless file synchronization, and upon each successful synchronization, **SeamlessXMLHelper** will be called to update the metadata.

# Explanation of Algorithms #
## Manual Mode ##
Using pre-traversal, BuilderVisitor will build a tree based on the given root paths. However, instead of a node for each and every folder, it groups files and folders of the same name at each level into single node. A list of string to store conflicts is also passed into the constructor for BuilderVisitor. The purpose of this list is to store [incompatible type conflicts](DeveloperComponentCompareAndSync#Conflicts.md). If we want to synchronize "Lectures", "Notes" and "School", and the content of the folders are as follows:

**Lectures**
  * SWEN.pdf
  * Extra Notes
    * Principles of Software Engineering.pdf

**Notes**
  * BizComm.ppt

**School**
  * Extra Notes
    * Principles of Software Engineering.pdf

From the above, we can see that "SWEN.pdf" exists only in Lectures, "BizComm.ppt" only in Notes, and "Extra Notes" folder exists in both Lectures and School. With the above, we will generate the following tree:

![http://big5sync.googlecode.com/files/DeveloperCompareAndSyncDiagTreeSmall.png](http://big5sync.googlecode.com/files/DeveloperCompareAndSyncDiagTreeSmall.png)

With the above tree, it is very easy to compare as we only need to concern ourselves with a single node for every file or folder. Now that the tree is built and populated with files, the XMLMetadataVisitor will populate each node with metadata, as well as create nodes if the file exists in the metadata but not in the folder. The metadata is very important in detecting if a file or folder has been renamed or deleted, so that the changes will be propagated across.

The FolderRenameVisitor will now visit the tree in level-order, and detect for folder renames. The reason why it has to occur before any file comparison can take place is due to the fact that the algorithm attempts to merge any renamed folder with folders that have the old name, so that files can still be compared. There are two main methods in FolderRenameVisitor, namely `DetectFolderRename` and  `MergeRenamedFolder`, that handles this. The code for `DetectFolderRename` is as follows:

```
        // Detect folder renames, if any.
        private void DetectFolderRename(FolderCompareObject folder, int numOfPaths)
        {
            List<int> deleteIndexes = new List<int>(); // Keeps a list of deleted indexes
            List<int> unchangedIndexes = new List<int>(); // Keeps a list of unchanged indexes

            for (int i = 0; i < numOfPaths; i++)
            {
                switch (folder.ChangeType[i])
                {
                    case MetaChangeType.Delete:
                        deleteIndexes.Add(i);
                        break;
                    case MetaChangeType.NoChange:
                        unchangedIndexes.Add(i);
                        break;
                }
            }

            // 1. If there exists a folder for which meta exists is true and exists is false, it is (aka changeType.delete)
            //    highly probable that it is a folder rename
            // 2. We check all folders which has the same meta name but different name as the non-existent folder
            // 3. If the count is 1, we shall proceed to rename
            FolderCompareObject folderObject;

            if (deleteIndexes.Count > 0)
            {
                int renameCount;
                folderObject = folder.Parent.GetRenamedFolder(folder.Name, out renameCount);

                if (renameCount > 1) // Multiple renames detected, set all unchanged to New so they will be propagated again
                {
                    foreach (int j in unchangedIndexes)
                        folder.ChangeType[j] = MetaChangeType.New;
                    return; // Exit
                }

                if (folderObject != null) // If folderObject != null and we reach here implies renameCounter is 1.
                {
                    for (int i = 0; i < numOfPaths; i++)
                    {
                        if (!folderObject.Exists[i]) // Remove all delete indexes if folder object does not exist at specified index
                            deleteIndexes.Remove(i); // so that only those that exist will be merged
                    }

                    MergeRenamedFolder(folder, folderObject, deleteIndexes);
                }

            }
        }
```

If it is determined that a folder rename has occurred, we will merge it by calling `MergeRenamedFolder`. The main purpose of this method is to combine the contents in the renamed folders with the contents on the un-renamed folders. The code for `MergedRenamedFolder` is as follows:

```
        // Merge renamed folder
        private void MergeRenamedFolder(FolderCompareObject actualFolder, FolderCompareObject renamedFolder, List<int> deleteIndexes)
        {
            Dictionary<string, BaseCompareObject>.KeyCollection renamedFolderContents = renamedFolder.Contents.Keys;
            BaseCompareObject o;
            FolderCompareObject actualFldrObj;
            FolderCompareObject renamedFolderObj;
            FileCompareObject actualFileObj;
            FileCompareObject renamedFileObj;

            // Set new name of actual folder to the found renamed folder
            actualFolder.NewName = renamedFolder.Name;

            // Set each of the delete indexes to rename instead
            foreach (int i in deleteIndexes)
                actualFolder.ChangeType[i] = MetaChangeType.Rename;

            foreach (string name in renamedFolderContents)
            {
                // If name is found in contents
                if (actualFolder.Contents.TryGetValue(name, out o))
                {
                    // If actualFldrObj is a folder
                    if ((actualFldrObj = o as FolderCompareObject) != null)
                    {
                        renamedFolderObj = renamedFolder.Contents[name] as FolderCompareObject; // Set renamedFolderObj to the one found in renamedFolder
                        MergeFolder(actualFldrObj, renamedFolderObj, deleteIndexes);
                    }
                    else
                    {
                        actualFileObj = o as FileCompareObject; // Assign o as a FileCompareObject
                        renamedFileObj = renamedFolder.Contents[name] as FileCompareObject; // Set renamedFileObj to the one found in renamed folder object
                        MergeFile(actualFileObj, renamedFileObj, deleteIndexes);
                    }
                }
                else // If not found, add as child to renamedFolder
                {
                    actualFolder.AddChild(renamedFolder.Contents[name]);
                    renamedFolder.Contents[name].Parent = actualFolder;
                }
            }

            actualFolder.Parent.Dirty = true; // Set parent of actual folder as Dirty
            renamedFolder.Contents = new Dictionary<string, BaseCompareObject>(); // "Clear" the contents
            renamedFolder.Invalid = true; // Set renamed folder to invalid
        }
```

`MergeFile` and `MergeFolder` are helper methods to assist with the merging of content. After FolderRenameVisitor is done with its job, ComparerVisitor will visit the tree in post-order. It is essential that it visits in post-order since any changes to a file or folder will imply that the parent folder is dirty, and thus we have to visit the children before the parent. There are various methods in ComparerVisitor that handles the detection of renamed files, as well as the comparing of files and folders. Let us take a look at the main function, `CompareFiles`.

```
        private void CompareFiles(FileCompareObject file, string[] currentPaths)
        {
            //Some code for handling deleted and renamed files commented out
            
            //Keeps track of the index of the most updated file position
            int mostUpdatedPos = 0;

            //Set the most updated file position to the first file that exists
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.Exists[i])
                {
                    mostUpdatedPos = i;
                    break;
                }
            }

            //Set the priority of the file with the most update position to 1.
            file.Priority[mostUpdatedPos] = 1;


            for (int i = mostUpdatedPos + 1; i < currentPaths.Length; i++)
            {
                //Set all non-existent files to priority -1
                if (!file.Exists[i])
                {
                    file.Priority[i] = -1;
                    continue;
                }

                 //If the length or hash of the file at the most updated position is different from the current index,
                //a change has occurred
                if (file.Length[mostUpdatedPos] != file.Length[i] || file.Hash[mostUpdatedPos] != file.Hash[i])
                {
                    //Update the value of most updated position if the file at index i is more updated
                    if (file.LastWriteTime[i] > file.LastWriteTime[mostUpdatedPos])
                    {
                        file.Priority[i] = file.Priority[mostUpdatedPos] + 1;
                        mostUpdatedPos = i;
                    }
                }
                else
                {   //Set the priority of index i to be same as that of the most updated position if there is no change
                    file.Priority[i] = file.Priority[mostUpdatedPos];
                }
            }

            for (int i = 0; i < currentPaths.Length; i++)
            {
                //Set parent folder to be dirty if the file has a lower priority than the one with the most updated position
                if (file.Exists[i] && file.Priority[i] != file.Priority[mostUpdatedPos])
                {
                    file.Parent.Dirty = true;
                    break;
                }
            }
        }
```

To summarize, ComparerVisitor simply assigns a priority of 1 to the first file that it determines exist. Subsequently, it will iterate through, and files which are less updated than it will be untouched (thus, 0, since that is the default int value), and when it comes across a file which is more updated, the most updated position will be set to this file, with the priority incremented by one. Files which are determined to be equal to the one with the most updated position will be given the same priority. In other words, if all files in the given FileCompareObject are equal, then all files will have priority 1. Let us further illustrate with the following example, using just the time of the day to represent how updated the file is:

| A\Note.txt (10PM) | B\Note.txt (10PM) | C\Note.txt (9PM) | D\Note.txt (11PM) | E\Note.txt (8AM) |
|:------------------|:------------------|:-----------------|:------------------|:-----------------|

After applying the above algorithm, we should get the following, with the priorities in brackets now.

| A\Note.txt (1) | B\Note.txt (1) | C\Note.txt (0) | D\Note.txt (2) | E\Note.txt(0) |
|:---------------|:---------------|:---------------|:---------------|:--------------|

Clearly, D\Note.txt is the most updated file. To end it off, SyncerVisitor will then look for the file or folder with the highest priority at each node, and execute the necessary action base on the ChangeType. Once that is done, XMLWriterVisitor will visit it and write the updated values, such as the hash and last modified date, back to the metadata. For a more detailed overview of how the tree and the visitors work, please take a look at this [presentation](http://big5sync.googlecode.com/files/Merged_Tree_Overview_V2.0.pps).

## Seamless Mode ##
Seamless mode is much more simpler than manual mode. It simply receives **AutoSyncRequest** objects, and processes them accordingly.

# XML Metadata #
There are 2 XML files that are stored in a ".syncless" folder (hidden attribute) inside each folder that is being synchronized. They are "syncless.xml" that contains all the files and folders within it (only one level down, it does not contain the contents of the folders), and "lastknownstate.xml" that stores whether a file has been deleted recently or not. Both kind of metadata are used by XMLMetadataVisitor to populate the various nodes in the tree. A more detailed description of them are as follows:


## syncless.xml ##
### Sample XML ###
```
<?xml version="1.0"?>
<meta-data>
  <last_modified_utc>634068154959990000</last_modified_utc>
  <name>jdk-6u18-docs</name>
  <folder>
    <name>docs</name>
    <last_updated_utc>634068153616650000</last_updated_utc>
  </folder>
  <file>
    <name>15 - Master Plan (Bonus Track).mp3</name>
    <size>5640045</size>
    <hash>0AA40E362C9509774E3CA3E84F8F3E63</hash>
    <last_modified_utc>633940776114069966</last_modified_utc>
    <last_created_utc>634068153616460000</last_created_utc>
    <last_updated_utc>634068153616650000</last_updated_utc>
  </file>
</meta-data>
```

### Description of XML ###
From the root node, there are the following:

  * **last\_modified\_utc**: The time the metadata file was updated, only one element of `last_modified_utc` will be in root.
  * **name**: The name of the folder the metadata belongs to, only one element of `name` will be in root.
  * **folder**: Each folder element stores information about a folder, multiple elements to represent multiple folders.
  * **file**: Each file element stores information about a file, multiple elements to represent multiple files.

For a folder node, it will contain the following child nodes:

  * **name:** The name of the folder
  * **last\_updated\_utc**: The time the metadata was written for this particular `folder` element

For a file node, it will contain the following child nodes:

  * **name:** The name of the file
  * **size:** The size of the file in bytes
  * **hash:** The hash of the file in MD5
  * **last\_modified\_utc** : The last modified/written time of the file
  * **last\_created\_utc**: The creation time of the file
  * **last\_updated\_utc**: The time the metadata was written for this particular `file` element

## lastknownstate.xml ##
### Sample XML ###
```
<?xml version="1.0"?>
<last_known_state>
  <file>
    <name>04 - Strut.mp3</name>
    <action>deleted</action>
    <hash>95B8C750C0D1F662ACAC2517A38954E7</hash>
    <last_modified_utc>633939988740000000</last_modified_utc>
    <last_updated_utc>634068503767260000</last_updated_utc>
  </file>
  <folder>
    <name>New folder</name>
    <action>deleted</action>
    <last_updated_utc>634068504345600000</last_updated_utc>
  </folder>
</last_known_state>
```

### Description of XML ###
This XML simply stores the last known state of a file or folder when it no longer exists, such as when they are deleted or renamed. For now, it only handles deletions since we are able to detect renames using that. It contains multiple file or folder elements, and their description are as follows:

For a folder node, it will contain the following child nodes:

  * **name:** The name of the folder
  * **action:** Specifies whether it is a deleted or renamed action. Only deleted is used for now.
  * **last\_updated\_utc**: The time the metadata was written for this particular `file` element

For a file node, it will contain the following child nodes:

  * **name:** The name of the file
  * **action:** Specifies whether it is a deleted or renamed action. Only deleted is used for now.
  * **hash:** The hash of the file in MD5
  * **last\_modified\_utc** : The last modified/written time of the file
  * **last\_updated\_utc**: The time the metadata was written for this particular `file` element

# Extending CompareAndSync #
Further functionality can be added into CompareAndSync should a developer wish to. He can simply create his own visitor that visits each node and performs the action he decides. A good example would be to generate a report after a synchronization. A developer can simply code a visitor to create a report based on the attributes of each node.