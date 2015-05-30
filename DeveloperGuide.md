![http://big5sync.googlecode.com/files/mainpagelogosmall.png](http://big5sync.googlecode.com/files/mainpagelogosmall.png)


---





---


# System Overview #

## Architecture ##

Following the Model-View-Controller (MVC) model, the system is split into User Interface (UI) and System Logic Layer (SLL) and some models classes. The UI is completely separated from SLL. The SLL uses a facade pattern. It is further divided into several sub-components, all of which work independently of one another.

Communication between the UI and the SLL is done through a Service Locator and an Interface. This allows us to change the SLL without affecting the UI. On top of that, this also allows other developers to create their own user interfaces to work with SLL.

![![](http://big5sync.googlecode.com/files/System%20Overview%20-%20v2%20-%20thumb.png)](http://big5sync.googlecode.com/files/System%20Overview%20-%20v2.png)

### Interaction among Sub-Components ###

To understand how the SLL interacts with the other components to perform the main operations, refer to sequence diagrams [here](http://code.google.com/p/big5sync/wiki/DeveloperSLLSequenceDiagrams).

## System Components ##

### Data Storing ###

Syncless uses various XML files to store information. XML was chosen as it is more readable and portable. Examples are tagging.xml and profiling.xml, which are stored in the root folder of the application. On top of that, some XML is also stored on the removable devices that the user has tagged. These XML files are use to communicate information between 2 computers.

Finally, each folder that is tagged also has metadata stored in them. At the moment, the XML is stored in a folder `.syncless` and is hidden. In the future, we will move this metadata elsewhere as most users do not like seeing "unwanted" files appearing all over the place.

### Core ###

The Core component provides the main functionality of the system and provides an interface for the User Interface component to interact with the system.

To understand more about Core component, please click
[here](http://code.google.com/p/big5sync/wiki/DeveloperComponentCore).

### Profiling ###

The Profiling component performs drive/computer recognization. It makes Syncless truly portable because of its ability to recognize different drives/computers by creating a Globally Unique IDentifier (GUID) in each of them. It also performs conversion of a physical directory/file path to a logical address, and vice versa. This conversion is necessary, because CompareAndSync only recognizes physical path while Tagging recognizes a logical path.

To understand more about Profiling component, please click [here](http://code.google.com/p/big5sync/wiki/DeveloperComponentProfiling).

### Tagging ###

The Tagging component provides the underlying operations of creating and removing tags, as well as tagging and untagging folders from tags.It will store the logical address of the path.

To understand more about Tagging component, please click [here](http://code.google.com/p/big5sync/wiki/DeveloperComponentTagging).

### CompareAndSync ###

The CompareAndSync component in Syncless is the part of the system that handles the comparison and syncing of files and folders. From a logical point of view, it can be briefly split into 2 subcomponents; the manual subcomponent that handles all manual operations, and the seamless subcomponent that handles all the automated synchronization requests from another component, the Monitor.

To understand more about CompareAndSync component, please click [here](http://code.google.com/p/big5sync/wiki/DeveloperComponentCompareAndSync).

### Monitor ###

The Monitor component provides a set of classes to detect file system changes. There are 2 roles for the Monitor component. The first role is to detect the insertion and removal of USB removable drives, such as external hard disks and flash drives, and the second role is to capture any changes to a file or folder within a tagged folder.

To understand more about Monitor component, please click [here](http://code.google.com/p/big5sync/wiki/DeveloperComponentMonitor).

### Logging ###

The Logging component is in charge of handling all user and debug logging actions. It is implemented using log4net with the Decorator pattern.

To understand more about Logging component, please click [here](http://code.google.com/p/big5sync/wiki/DeveloperComponentLogging).

### Filter ###

The Filter component provides a set of classes to cater to filtering folders and files. They can be used to filter default .syncless configuration files as well as archive files.

To understand more about Filter component, please click [here](http://code.google.com/p/big5sync/wiki/DeveloperComponentFilter).

### Notification ###

The Notification component provides a low coupling medium for the control layer to pass information to the User Interface. It also allows the lower facade layer to pass information to the Logic layer.

To understand more about Notification component, please click [here](http://code.google.com/p/big5sync/wiki/DeveloperComponentNotification).

### User Interface ###

The User Interface (UI) component is in charge of handling all interactions between the user and the control layer. It is implemented using WPF for its powerful framework which allows for the creation of clean, professional look-and-feel. It also allows for the true separation of the UI from the System Logic Layer, allowing other developers to create their own UI to interact with our Logic Layer.

To understand more about User Interface component, please click [here](http://code.google.com/p/big5sync/wiki/DeveloperComponentUI).

# Known Issues #

There are some known issues with V1.0, and they can be found [here](http://code.google.com/p/big5sync/wiki/KnownIssues10).

# Special Cases #
Syncless aims not to throw any kind of file or folder synchronization, and although it generally works as expected in most cases, there may be some sequence of actions that are handled differently. A list of these special cases can be found [here](http://code.google.com/p/big5sync/wiki/SpecialCases).

# Testing Syncless #

## Using Test Driver ##
It is a difficult process to test the seamless (real-time) mode in Syncless, as it would require a the tester to simulate a sequence of events randomly. Moreover, seamless mode depends on FileSystemWatcher, and the events thrown by FileSystemWatcher are largely undeterministic, and depending on the application that caused FileSystemChange, the events thrown can be completely different, even for a simple action such as "Save".

Thus, we have come up with this seamless tester, which is quite different from a conventional test driver. It takes in a list of source folders, and a list of destination folders, and through a lot of randomization, changes will be simulated in the destination folders. At the end of the whole simulation (duration can be specified), all the files/folders in all the destination folders will be checked to see if the content is equal.

To understand more about using the tester, please click [here](http://code.google.com/p/big5sync/wiki/TestSeamlessSyncerTester).

## Test Cases ##
Alternatively, if you want to see the real results of the synchronization, you can go through a step-by-step test case to test Syncless.

You can see a set of test cases [here](http://code.google.com/p/big5sync/wiki/TestCases).

# API #

You can download the Combined HTML Help File [here](http://big5sync.googlecode.com/files/SynclessCompleteAPIV2.0.zip)!

# Recommended Resources #
[These](http://code.google.com/p/big5sync/wiki/DeveloperGuideRecommendations) are additional resources which may help the developer in understanding how to develop for Syncless further.

# References #
Throughout the development of this project, we referred to codes and solutions found in various places. All credits and references are given [here](DeveloperGuideReferences.md).

# Glossary #
Syncless uses a few terms which may not be common in other synchronizations tools. For definitions of these terms, please check them out [here](http://code.google.com/p/big5sync/wiki/SynclessGlossary).