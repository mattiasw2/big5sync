![http://big5sync.googlecode.com/files/mainpagelogosmall.png](http://big5sync.googlecode.com/files/mainpagelogosmall.png)


---





---


# Preface #

This user's guide provides a detailed insight into the functionality available in **_Syncless_**. If you wish to have a quick overview of the basic functions provided in **_Syncless_**, you can refer to the **Getting Started** section [here](#Getting_Started.md). If you wish to print out this guide, you may download the print-friendly version [here](http://big5sync.googlecode.com/files/UserGuideV2-0.pdf)!

# Welcome to Syncless #

![http://big5sync.googlecode.com/files/Welcome_Window.png](http://big5sync.googlecode.com/files/Welcome_Window.png)

Synchronization is taken on a whole new level with _**Syncless**_. This user's guide familiarizes you with the basic features in _**Syncless**_ and introduces you to the concepts of **tagging** and **n-way** _seamless synchronization_ in _**Syncless**_.

_**Syncless**_ allows you to synchronize your files and folders automatically through the use of tags, even across removable drives. With _**Syncless**_, you can quickly tag multiple folders and configure them to be either in _seamless_ or _manual_ synchronization mode. Syncless is designed to have the finest usability that caters to each individual's workflow. Lastly, Syncless is a portable application, so it will run off any removable drive with no installation required.

## Why Use Syncless? ##

Why choose Syncless over other synchronization tools? The reason is simple. Syncless provides many powerful features which are rarely found in one single synchronization tool in the market.

  * **N-way Synchronization**: Why restrict yourself to just syncing two folders? Let Syncless perform n-way synchronization between multiple folders for you.

  * **Seamless Synchronization**: Any changes in the tagged folders can be detected and synchronized without any human intervention.

  * **Filtering**: Use Syncless's filtering features for the ultimate control in synchronization. Currently, Syncless is able to include or exclude files and folders by extension and path.

  * **Archiving**: Syncless archives files and folders which have been removed. Every file and folder can either be sent to the Recycle Bin or archived up to 10 copies.

  * **Time Synchronization**: Syncless can automatically keep your computer clock in sync with an internet time server to ensure accurate synchronisation.

  * **Portability**: Be the ultimate on-the-go warrior and carry Syncless on your removable drive instead of leaving it on your computer.

### N-way Synchronization ###
The traditional way to keep a few folders in sync is to create multiple jobs because many tools provide synchronization between 2 folders only. Syncless is capable of synchronizing multiple folders without you having to create multiple jobs. Simply tag multiple folders with the same tag name and Syncless will keep them all in sync.

### Seamless Synchronization ###
Seamless synchronization allows folders that are tagged to be synchronized automatically if there are changes to the files within the folders. You can just leave the program running while attending to other tasks at hand. At the end of the day, you will find that all your folders are in sync!

### Filtering ###
Syncless currently provides basic filtering capabilities for files. There are 2 main kinds of filters, inclusion and exclusion filters. Exclusion filters will always have a higher priority over inclusion filters. In this user guide, we are going to take a look at how you can construct filters to suit your needs.

### Archiving ###
With the archiving feature, you do not need to worry about losing an important file if you accidentally delete it and the change is propagated to other folders in the same tag. Syncless will either send the deleted file to the Recycle Bin, or keep up to 10 copies of the deleted file in an archive folder, depending on your preference. This archive folder will be created in all the other folders in the same tag. Note that the file itself will not be archived in the folder where the file deletion is initiated by the user.

To illustrate, let's say you tagged D:\Lecture and E:\Notes to a tag named "Lecture", both containing the file Chapter1.pdf. You accidentally deleted Chapter1.pdf under D:\Lecture. Syncless will keep a copy of Chapter1.pdf in the archive folder under E:\Notes. If you need to restore that file, simply copy the archived file back to D:\Lecture.

### Time Synchronization ###
Files may be out-of-sync if the system clocks are not consistent across multiple computers. Why worry that your most updated file will be overwritten because the system clock is earlier than the actual time? Simply allow Syncless to synchronize your system clock with the Internet Time Server and you can be rest assured that the file will remain as the most updated version across all your computers.

### Portability ###
Syncless can run from a removable drive such as a thumbdrive. If you frequently use your removable storage drive for the purpose of synchronizing files and folders on various computers, Syncless can automate this job for you.

For example, a student has a home computer, as well as a regular workstation in his school lab. He wishes to keep his school work synchronized across both computers. By tagging his school work folders on both his home computer and school workstation with the same tag, say "Lectures", Syncless will auto-detect and synchronize each time the removable storage drive is inserted. See the diagrams below for a clearer illustration:

![http://big5sync.googlecode.com/files/merging.png](http://big5sync.googlecode.com/files/merging.png)


## What is a Tag? ##

The concept of **tagging** is not uncommon today. In Syncless's context, it is simply an identity or a label associated with folders you want to keep in sync. Associate several folders to the same tag name and Syncless will keep them all in sync for you. It is as simple as that. You simply tag the folders you want to keep in sync and Syncless will handle everything else. We will discuss the various ways of tagging folders in the subsequent section of this tutorial.


## What You'll Need ##
Before continuing, make sure you have the following:
  * Windows Operating System (XP and above)
  * .NET 3.5 SP1 Framework (Get it from http://msdn.microsoft.com/en-us/netframework/cc378097.aspx)

To download Syncless, please visit http://code.google.com/p/big5sync/downloads/list to get the latest version (ver 2.0) of Syncless. After that, unzip the package to a location of your choice and run SynclessUI.exe. Absolutely no installation of Syncless is required.

## What Is Recommended ##
Internet access is needed to use the Time Synchronization Feature. However, it is not needed to run the program.

## The Syncless Interface ##
When you run Syncless, you will be greeted with the main window, which allows you to choose what you want to do next, such as creating a tag, tagging a folder or viewing tag details. The interface also incorporates a lot of useful shortcuts to speed up productivity.

![http://big5sync.googlecode.com/files/Main_Window_Overview.png](http://big5sync.googlecode.com/files/Main_Window_Overview.png)


---


# Getting Started #

## Understanding the Syncless Interface ##

Before you continue, you may want to find out what each button in Syncless does.

### Window Toolbar ###

  * ![http://big5sync.googlecode.com/files/ShortcutButton.png](http://big5sync.googlecode.com/files/ShortcutButton.png) : Displays the list of shortcut keys in Syncless

  * ![http://big5sync.googlecode.com/files/ApplicationSettingsButton.png](http://big5sync.googlecode.com/files/ApplicationSettingsButton.png) : Configure Syncless settings

  * ![http://big5sync.googlecode.com/files/MinimizeWindowButton.png](http://big5sync.googlecode.com/files/MinimizeWindowButton.png) : Minimize Syncless main window

  * ![http://big5sync.googlecode.com/files/ExitWindowButton.png](http://big5sync.googlecode.com/files/ExitWindowButton.png) : Exit Syncless

### Main Toolbar ###

  * ![http://big5sync.googlecode.com/files/CreateTagButton.png](http://big5sync.googlecode.com/files/CreateTagButton.png) : Create a tag

  * ![http://big5sync.googlecode.com/files/RemoveTagButton.png](http://big5sync.googlecode.com/files/RemoveTagButton.png) : Remove a tag

  * ![http://big5sync.googlecode.com/files/TagFolderButton.png](http://big5sync.googlecode.com/files/TagFolderButton.png) : Tag a folder

  * ![http://big5sync.googlecode.com/files/UntagFolderButton.png](http://big5sync.googlecode.com/files/UntagFolderButton.png) : Untag a folder

  * ![http://big5sync.googlecode.com/files/TagDetailsButton.png](http://big5sync.googlecode.com/files/TagDetailsButton.png) : View tag properties

  * ![http://big5sync.googlecode.com/files/ViewLogButton.png](http://big5sync.googlecode.com/files/ViewLogButton.png) : View log

  * ![http://big5sync.googlecode.com/files/UnmonitorButton.png](http://big5sync.googlecode.com/files/UnmonitorButton.png) : Unmonitor removable drives

  * ![http://big5sync.googlecode.com/files/TimeSyncButton.png](http://big5sync.googlecode.com/files/TimeSyncButton.png) : Synchronize computer time

### Tag Info Panel ###

  * ![http://big5sync.googlecode.com/files/Manual_Sync_Button.png](http://big5sync.googlecode.com/files/Manual_Sync_Button.png) : Indicates that the current synchronization is in manual mode, click to switch to seamless mode

  * ![http://big5sync.googlecode.com/files/Seamless_Button.png](http://big5sync.googlecode.com/files/Seamless_Button.png) : Indicates that the current synchronization is in seamless mode, click to switch to manual mode

  * ![http://big5sync.googlecode.com/files/Please_Wait_Button.png](http://big5sync.googlecode.com/files/Please_Wait_Button.png) : Indicates that the selected tag is switching from manual mode to seamless mode

  * ![http://big5sync.googlecode.com/files/Preview_Sync_Button.png](http://big5sync.googlecode.com/files/Preview_Sync_Button.png) : Preview synchronization result, only available in manual mode

  * ![http://big5sync.googlecode.com/files/Sync_Now_Button.png](http://big5sync.googlecode.com/files/Sync_Now_Button.png) : Start synchronization for currently selected tag, only available in manual mode

  * ![http://big5sync.googlecode.com/files/Cancel_Sync_Button.png](http://big5sync.googlecode.com/files/Cancel_Sync_Button.png) : Cancel synchronization, only available during analyzing or queued stage in manual mode

  * ![http://big5sync.googlecode.com/files/Cancelling_Sync_Button.png](http://big5sync.googlecode.com/files/Cancelling_Sync_Button.png) : Preparing to cancel synchronization

## How to Tag and Synchronize? ##

You have folders on your computer and removable drives that you wish to synchronize automatically. Let us get started on using Syncless!

### 1. Tag Folders ###

To synchronize 2 or more folders, you must first tag them with the same name. This tagging process can be initiated in multiple ways.

#### Tagging from Syncless ####

Tag a folder through the ![http://big5sync.googlecode.com/files/TagFolderButton.png](http://big5sync.googlecode.com/files/TagFolderButton.png) button (You can also use the keyboard shortcut equivalent Ctrl-T.)
> ![http://big5sync.googlecode.com/files/Tag_Thru_Syncless.png](http://big5sync.googlecode.com/files/Tag_Thru_Syncless.png)

#### Drag-and-Drop Folder(s) into the Main Window ####
> ![http://big5sync.googlecode.com/files/Drag_And_Drop_Tag.png](http://big5sync.googlecode.com/files/Drag_And_Drop_Tag.png)

#### Tag Directly from Windows Explorer ####
> ![http://big5sync.googlecode.com/files/Context_Menu_Tag.png](http://big5sync.googlecode.com/files/Context_Menu_Tag.png)

### 2. View Tag Information ###
> ![http://big5sync.googlecode.com/files/Tag_Information_Panel.png](http://big5sync.googlecode.com/files/Tag_Information_Panel.png)

After tagging the folders, you may view the details under the tag information panel through the ![http://big5sync.googlecode.com/files/TagDetailsButton.png](http://big5sync.googlecode.com/files/TagDetailsButton.png) button. You will see a list of folder paths associated with the selected tag. On the right of the folder path list, there are a few options for synchronization. During synchronization, the progress bar will show the percentage of completion and status of the synchronization.

### 3. Synchronize the Folders ###
> ![http://big5sync.googlecode.com/files/Tagged_Path_List.png](http://big5sync.googlecode.com/files/Tagged_Path_List.png)

After you have tagged the required folders, click on the ![http://big5sync.googlecode.com/files/Sync_Now_Button.png](http://big5sync.googlecode.com/files/Sync_Now_Button.png) button. Syncless will take care of the rest and keep the folders in sync.

### 4. Switch to Seamless Mode ###

![http://big5sync.googlecode.com/files/Switching_To_Seamless_Window.png](http://big5sync.googlecode.com/files/Switching_To_Seamless_Window.png)

Switching to seamless mode is a simple matter of pressing the button. After clicking the synchronization mode button, Syncless will automatically perform an initial synchronization. At the moment, ![http://big5sync.googlecode.com/files/Please_Wait_Button.png](http://big5sync.googlecode.com/files/Please_Wait_Button.png) button is shown to indicate that switching is in progress. After which, it will monitor any changes in the tagged folders and keep them in sync without you having to do it manually.

### 5. Unmonitor Removable Drive to Prepare for Removal ###

![http://big5sync.googlecode.com/files/Unmonitor_Drive.png](http://big5sync.googlecode.com/files/Unmonitor_Drive.png)

You can access this through the ![http://big5sync.googlecode.com/files/UnmonitorButton.png](http://big5sync.googlecode.com/files/UnmonitorButton.png) button or by using its keyboard shortcut equivalent (Ctrl-I).

Syncless cares about the safety of your removable drives. Unmonitoring will stop all monitoring actions on any folders that are currently in seamless mode, thus allowing the removable drive to be safely removed.

### 6. Configure Filter Properties ###

#### Create a Filter ####
Click the ![http://big5sync.googlecode.com/files/TagDetailsButton.png](http://big5sync.googlecode.com/files/TagDetailsButton.png) button in the toolbar to open up the tag properties panel. Inside the properties panel, choose the **Filtering** property. You should see a list of filters and it will likely be blank since no filters have been added yet.

![http://big5sync.googlecode.com/files/Filtering.png](http://big5sync.googlecode.com/files/Filtering.png)

Now, click on the ![http://big5sync.googlecode.com/files/Add_Filter_Button.png](http://big5sync.googlecode.com/files/Add_Filter_Button.png) button to add a new filter. You can now set the "Ext. Mask" and the "Mode".

#### Construct a Filter ####
Ext. mask can be an actual filename, or simply a name with wildcards. Some examples will be "Storyboard.ppt", "`*`.exe" or "File?.txt".

  * **`*`** - Wildcard for any number of characters. For example, s`*`t will mean sit, sat, shoot, skit, etc.

  * **?** Wildcard for exactly one character. For example, s?t  will mean sit, sat, etc.

Mode can be set to exclusion or inclusion mode. If a filter is set to exclusion mode, then all files that match the ext. mask will be excluded. On the contrary, if a filter is set to inclusion mode, then all files matching the ext. mask will be included.

Now, let us look at some real-world examples. For example, if you want to include all Powerpoint files, but exclude a particular one, say "Introduction.ppt", you will create 2 filters:

  1. Ext. Mask: `*`.ppt, Mode: Inclusion
  1. Ext. Mask: Introduction.ppt, Mode: Exclusion

As seen from the above, creating a filter is a simple and intuitive task in Syncless.


---


# Continue to Explore Syncless #

## How to Configure Syncless? ##

You can personalize Syncless through the ![http://big5sync.googlecode.com/files/ApplicationSettingsButton.png](http://big5sync.googlecode.com/files/ApplicationSettingsButton.png) button. After which, you will see a window with two settings to choose from, **General and User Interface Settings** and **Archiving Settings**.

### General and User Interface Settings ###

![http://big5sync.googlecode.com/files/Default_General_Settings_Window.png](http://big5sync.googlecode.com/files/Default_General_Settings_Window.png)

_Options enabled by default are prefixed with `*`_

#### `*`Show Welcome Screen on Application Start ####

Syncless has an informative welcome screen which highlights the unique features of Syncless. Each feature has a link to the Synclses online User Guide where the feature is explained in more details. You can choose to show the welcome screen on startup by checking the option.


#### `*`Enable Shell Integration ####

Syncless makes tagging easy for you by integrating directly into Windows Explorer. You can simply right-click on any folder you wish to tag. In the case where you rather not have this function, you can uncheck the option.


#### `*`Enable Window Animation on Application Start/Exit ####

Syncless's main window will animate upon start up or exit. You may choose to disable the function by unchecking the option.


#### `*`Enable Tray Notifications ####

Syncless notifies you through tray notification upon initialization/completion of synchronization. Enable/disable the function by checking/unchecking the option.


#### `*`Enable Notification Sounds ####

Syncless not only notifies you of synchronization initialization/completion through tray notification. It also plays notification sounds to notify you when you are not viewing the screen. Enable the function by checking the option.


#### `*`When Minimizing, Minimize to Tray ####

Syncless can minimize to tray so that your task bar will be kept as clean as possible. However, if you wish to access Syncless more quickly through the task bar, you can disable the function by unchecking the option.


#### Attempt to Synchronize Computer Clock On Application Startup ####

Worried that your files are out-of-sync because the system clocks are not consistent across your computers? Syncless provides the option of synchronizing your system clock with the Internet Time Server on startup so that the system clock is always consistent. Enable time synchronization by checking the option. Note that Administrative Rights is required to synchronize your system clock.


### Archiving Settings ###

Syncless makes synchronization even safer by providing archiving option. Just in case you accidentally delete a file and perform synchronization on this file, you can still retrieve it from either the Recycle Bin or the Syncless' archive folder.

![http://big5sync.googlecode.com/files/Default_Archive_Settings_Window.png](http://big5sync.googlecode.com/files/Default_Archive_Settings_Window.png)

_Options enabled by default are prefixed with `*`_

#### Send to Recycle Bin ####

To send deleted files to Recycle Bin, simply check the option. Uncheck the option if you do not wish to send any file to Recycle Bin.


#### `*`Move Last N Changes to `_`synclessArchive ####

To move deleted files to Syncless' archive folder (`_`synclessArchive), simply check the option. You can select up to 10 archive files to keep by moving the slider. Uncheck the option if you do not wish to move any file to Syncless' archive folder.

## How is Syncless User-friendly? ##

Syncless makes your user experience as pleasant as possible by providing several user-friendly functionalities.

### Preview Synchronization Results ###

Want to know what changes will be made to your files and folders before committing the changes? Syncless allows you to preview the synchronization result so that you can confirm the changes you want to make, just in case unintended operation is performed on your files and folders.

Access the preview result window through the ![http://big5sync.googlecode.com/files/Preview_Sync_Button.png](http://big5sync.googlecode.com/files/Preview_Sync_Button.png) button.

![http://big5sync.googlecode.com/files/Preview_Window.png](http://big5sync.googlecode.com/files/Preview_Window.png)

Each operation is explained by the arrow legend as shown below:

![http://big5sync.googlecode.com/files/Preview_New.png](http://big5sync.googlecode.com/files/Preview_New.png)

![http://big5sync.googlecode.com/files/Preview_Updated.png](http://big5sync.googlecode.com/files/Preview_Updated.png)

![http://big5sync.googlecode.com/files/Preview_Delete.png](http://big5sync.googlecode.com/files/Preview_Delete.png)

![http://big5sync.googlecode.com/files/Preview_Rename.png](http://big5sync.googlecode.com/files/Preview_Rename.png)

### Viewing Logs ###

You do not have to remember the changes you have made to your folders and files. Syncless keeps a log of the changes in your folders and files and lets you view them easily.

Access the event log through the ![http://big5sync.googlecode.com/files/ViewLogButton.png](http://big5sync.googlecode.com/files/ViewLogButton.png) button.

![http://big5sync.googlecode.com/files/View_Log_Window.png](http://big5sync.googlecode.com/files/View_Log_Window.png)

You can filter out log messages by checking/unchecking the following options.

![http://big5sync.googlecode.com/files/Application_Log_Option.png](http://big5sync.googlecode.com/files/Application_Log_Option.png) ![http://big5sync.googlecode.com/files/Synchronization_Log_Option.png](http://big5sync.googlecode.com/files/Synchronization_Log_Option.png) ![http://big5sync.googlecode.com/files/Filesystem_Log_Option.png](http://big5sync.googlecode.com/files/Filesystem_Log_Option.png)

### Tag Name Auto Completion ###

Do you have a tag with a long name? Syncless understands the inconvenience you experience when trying to tag folders to a tag with a long name. As such, Syncless provides an auto-completion function when you type in a tag name.

![http://big5sync.googlecode.com/files/Auto_Complete_Tag_Name.png](http://big5sync.googlecode.com/files/Auto_Complete_Tag_Name.png)

Simply type in part of the tag name and Syncless will display the full name for you.

### Filter Tag List ###

Do you have many tags to maintain and have difficulty finding that exact tag from the long list? Syncless supports filtering of tags so that you can search for the tag you want just by entering the tag name.

Let's say you have many tags in the tag list.

![http://big5sync.googlecode.com/files/Multiple_Tag_Names.png](http://big5sync.googlecode.com/files/Multiple_Tag_Names.png)

You want to filter out only tags related to your photo folders. Simply enter the word 'Photos' in ![http://big5sync.googlecode.com/files/Search_Tag_Text_Box.png](http://big5sync.googlecode.com/files/Search_Tag_Text_Box.png) text box and only tag names containing 'Photos' will be displayed.

![http://big5sync.googlecode.com/files/Filtered_Tag_Names.png](http://big5sync.googlecode.com/files/Filtered_Tag_Names.png)

### Keyboard Shortcuts ###

Syncless helps you to save time and increase productivity by providing keyboard shortcuts to the many commonly used operations. Below is the list of keyboard shortcuts provided by Syncless.

![http://big5sync.googlecode.com/files/Shortcut_List_Window.png](http://big5sync.googlecode.com/files/Shortcut_List_Window.png)

If you forget the shortcut keys, simply click on  ![http://big5sync.googlecode.com/files/ShortcutButton.png](http://big5sync.googlecode.com/files/ShortcutButton.png) at the window toolbar or use the keyboard shortcuts `?` or `Ctrl-S`

### Accessing Syncless Through Tray ###

If you have chosen the option to minimize Syncless to tray, you can still access the application easily through tray options. Simply go to the tray, right-click on the Syncless icon, and select the operation you wish to perform. You can modify application settings, tag a folder, unmonitor a drive or exit Syncless.

![http://big5sync.googlecode.com/files/Access_Thru_Tray.png](http://big5sync.googlecode.com/files/Access_Thru_Tray.png)

### Accessing Tags Through Right-clicking ###

If you prefer right-clicking to clicking buttons, Syncless provides you the option to access your tags through context menu.

![http://big5sync.googlecode.com/files/Tag_Context_Menu.png](http://big5sync.googlecode.com/files/Tag_Context_Menu.png)

Simply right-click on the tag under the tag list and you can choose to tag more folders, view tag properties or remove the tag.

### Accessing Tagged Folders Through Right-clicking ###

Suppose you have many folders tagged with the same tag. When you want to view the folders in Windows Explorer, you will most probably have to go through "Start Menu -> My Computer -> D: -> Your folders" many times to open up the various folders. Why go through the hassle when you can simply open up the folders in Syncless?

![http://big5sync.googlecode.com/files/Tagged_Path_Context_Menu.png](http://big5sync.googlecode.com/files/Tagged_Path_Context_Menu.png)

Simply right-click on the tagged path in the tag info panel and choose to Open in Windows Explorer or just double-click on it. The folder will be opened in Explorer Window.

### Tray Notification ###

Syncless allows you to perform other tasks as well when you synchronize large amount of files. You will still be kept updated of the synchronization status even when you are not viewing the Syncless main window.

Whenever a synchronization task is initiated or completed, Syncless will notify you through tray notifications.

![http://big5sync.googlecode.com/files/Tray_Notification.png](http://big5sync.googlecode.com/files/Tray_Notification.png)


---


# Ending Note #

![http://big5sync.googlecode.com/files/Exit_Window_Message.png](http://big5sync.googlecode.com/files/Exit_Window_Message.png)

Thank you for taking time to read through the User Guide! We hope that you find it informative in guiding you through using Syncless' unique features.

Please report any bugs you find to http://code.google.com/p/big5sync/issues/list, thank you! You may wish to take a look at http://code.google.com/p/big5sync/wiki/KnownIssues20 for the known bugs in version 2.0. You may also contact us at **_big5.syncless@gmail.com_**.

Interested in what is coming after version 2.0? Take a look [here](http://code.google.com/p/big5sync/wiki/ComingSoon)!

Are you a developer? Interested to further improve Syncless? Have a [look](http://code.google.com/p/big5sync/wiki/DeveloperGuide)!

For complete and up-to-date information about Syncless, go to Syncless's website at
http://code.google.com/p/big5sync/