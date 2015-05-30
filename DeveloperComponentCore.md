

# Overview #
The **Core** component provides the main functionality of the system and provides an interface for the **User Interface** component to interact with the system.

# Classes #

## SystemLogicLayer ##

The main class that provides the implementation for the `IUIControllerInterface`. It provides most of the functionality that is required and delegates most of the specific jobs to the respective components.

## PathTable ##

The table is in charge of keeping a history of seamless Sync Requests, to reduce the number of unnecessary requests sent to the `CompareAndSyncController` (see below).

## LogicQueueObserver ##

The observer for the `SystemLogicNotificationQueue`. Processes the notifications and informs the `SystemLogicLayer` about the notifications received.

## SaveLoadHelper ##

The helper is in charge of saving the profiling data and tagging data to the various locations (see below).

## ServiceLocator ##

The class that allows different components to locate the different functions provided. It also allows the different components to access the message channel.



# Explanation of Algorithms/Implementation #

## PathTable ##

The main reason for the implementation of the path table is due to how `FileSystemWatcher` works and how our seamless works.

Take 3 Folders for example , Folder A , B and C.

If a new file is added to Folder A , the following events will be generated
  * Folder A(new) -> Folder B
  * Folder A(new) -> Folder C

Due to how `FileSystemWatcher` works, after the file is copied, Folder B will also detect a change and at almost the same time , Folder C will also detect a change.

The changes detected in the 2 folders may not come in any particular order.

The changes in Folder B and C will therefore trigger the following events

  * Folder B(New) -> Folder A
  * Folder B(New) -> Folder C

  * Folder C(New) -> Folder A
  * Folder C(New) -> Folder B

However, at this time, Folder A, B ,C will have the same file and do not need to be updated. It is clear that we need to filter these events out.

Here is the pseudo code for the adding of path table per event.
```
If File A is changed,
  for each File B "similar" to A (B!=A)
     add a entry in the pathtable , source B -> A
     for each File C "similar" to A (C!=B && C!=A)
          add a entry in the pathtable , source B -> C
```

This is to permute all the possible "expected" events generate from this event and add it to the pathtable.

Therefore, when an event is detected, all possible expected events need to be added to the path table.
When a event occur, the system will check if the event is a expected events from a previous events. If it is , the event will be discarded, and the respective entry will be remove from the pathtable. If it is not in the path table, the normal sequence of action happen as per normal.

This greatly reduces the number of events `CompareAndSyncController` receives.

However, this also brings in a number of issues. One of which is when a series of events happens too fast, some of the entries in the path table may not be cleared. To clear the path table, we will clear the path table after a certain time of inactivity.

## SaveLoadHelper ##

Saves the data into different locations. The locations to save to include the following :
  1. The Directory containing the executable.
  1. All local drives if the local drive contains a GUID attached to it.
  1. All external drives if the drive contains a GUID attached to it.