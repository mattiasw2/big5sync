`[`**[Prev - Syncless.CompareAndSync.Manual.CompareObject](DeveloperAPICompareAndSyncManualCompareObject.md)** `|` **[Next - Syncless.CompareAndSync.Request](DeveloperAPICompareAndSyncRequest.md)**`]`

Provides visitor classes needed to perform comparison and synchronization of files and folders.

# Interfaces Summary #

| | **Interface** | **Description** |
|:|:--------------|:----------------|
| ![http://big5sync.googlecode.com/files/pubinterface.gif](http://big5sync.googlecode.com/files/pubinterface.gif) | [IVisitor](#IVisitor.md) | Interface for all Visitors to implement. |

## IVisitor ##

[Back to top](#Interfaces_Summary.md)

# Classes Summary #

| | **Class** | **Description** |
|:|:----------|:----------------|
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [BuilderVisitor](#BuilderVisitor.md) | Responsible for creating the tree. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [ComparerVisitor](#ComparerVisitor.md) | Responsible for comparing and updating the states of the files and folders. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [ConflictVisitor](#ConflictVisitor.md) | Responsible for handling conflicted types (when file and folder have the same name, including extension). |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [FolderRenameVisitor](#FolderRenameVisitor.md) | Responsible for handling folder renames, and merging the contents of the renamed folders with their previous names. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [ProcessMetadataVisitor](#ProcessMetadataVisitor.md) | Responsible for processing the metadata and comparing it against the actual file or folder, as well as handle whether or not to rehash a file. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [SyncerVisitor](#SyncerVisitor.md) | Responsible for visiting the tree and synchronizing files after ComparerVisitor has updated the state of the tree. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [XMLMetadataVisitor](#XMLMetadataVisitor.md) | Based on the trees built by the BuilderVistor , XMLMetadataVistor will populate the data based on the meta data documents or the last known state. |
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [XMLWriterVisitor](#XMLWriterVisitor.md) | Based on each node after the SyncerVisitor , it will write the values in the nodes of the tree to the xml documents based on the FinalState. |

## BuilderVisitor ##

[Back to top](#Classes_Summary.md)

## ComparerVisitor ##

[Back to top](#Classes_Summary.md)

## ConflictVisitor ##

[Back to top](#Classes_Summary.md)

## FolderRenameVisitor ##

[Back to top](#Classes_Summary.md)

## ProcessMetadataVisitor ##

[Back to top](#Classes_Summary.md)

## SyncerVisitor ##

[Back to top](#Classes_Summary.md)

## XMLMetadataVisitor ##

[Back to top](#Classes_Summary.md)

## XMLWriterVisitor ##

[Back to top](#Classes_Summary.md)

`[`**[Prev - Syncless.CompareAndSync.Manual.CompareObject](DeveloperAPICompareAndSyncManualCompareObject.md)** `|` **[Next - Syncless.CompareAndSync.Request](DeveloperAPICompareAndSyncRequest.md)**`]`