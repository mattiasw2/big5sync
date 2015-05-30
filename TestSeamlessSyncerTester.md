

# Introduction #

It is a difficult process to test the seamless (real-time) mode in Syncless, as it would require a the tester to simulate a sequence of events randomly. Moreover, seamless mode depends on FileSystemWatcher, and the events thrown by FileSystemWatcher are largely undeterministic, and depending on the application that caused FileSystemChange, the events thrown can be completely different, even for a simple action such as "Save".

Thus, we have come up with this seamless tester, which is quite different from a conventional test driver. It takes in a list of source folders, and a list of destination folders, and through a lot of randomization, changes will be simulated in the destination folders. At the end of the whole simulation (duration can be specified), all the files/folders in all the destination folders will be checked to see if the content is equal.

![http://big5sync.googlecode.com/files/SeamlessTester.png](http://big5sync.googlecode.com/files/SeamlessTester.png)

# UI Description #
To begin, simply launch the test program. You will notice that there is a GUI as well as a console, since the program is still in it's early stages. On the GUI, you will see that the main area is divided into 3 parts: Source, Destination, Test Settings. There are basically 4 events that will occur; create, delete, modify, rename.

## Repository ##
Repository folders are folders from which files or folders will be pulled, and then thrown into one of the destination folders. The folder, as well as the content, that is chosen each time an event occurs is random. To add folders to the list, you can simply:

  1. Type in the path, then click Add
  1. Browse, then click Add
  1. Drag any number of folders you want into the ListBox

To remove the repository folders, click "Clear". There is no way to remove just a single folder for now, this will be implemented in future if time permits.


## Sync Folders ##
Sync folders are folders from which files or folders will be thrown to. In addition, the delete, modify, and rename events involve only the destination folders. To put it simply, sync folders are the folders you wish to keep in sync in Syncless for a particular tag, or even for a list of tags (depending on how they are tagged). The folder in which a create, delete, modify or rename event will occur each time is random. This includes all subfolders as well. To add folders to the list, follow the same steps as for Source.


## Propagation Settings ##
Duration is how long you want the propagation to run. The default value is 1 hour (3600 seconds). Minimum wait time is the minimum amount of seconds between each event, while maximum wait time is the maximum amount of seconds between each event. The time itself is a randomly generated value between the minimum and maximum. For now, the generator follows a normal distribution, so most of the time you will hit the mean of minimum and maximum. More timing patterns will be implemented in future.


# Start Testing! #
Once all the necessary info has been filled in, click on Propagate to begin simulation of user actions. The console will output the events, as well as a lot of exceptions (since a rename event might happen before the destination folders are populated), and it is mostly for information only. Upon completion of propagation, you can click Verify for verification to begin. Please expect to wait for a long while if the number and size of files synchronized are large.

The verifier basically takes the relative path and hash of each file and the relative path of each folder within each destination folder, and compares them. At the end of the test, you will see "RESULTS: PASSED" or "FAIL" in the console. You can also check the Log output for test results and conflicted files or folders. Note that some percentage lines might be printed after that, they are safe to ignore.

Note: You can also use Verify without propagating first by adding folders to Sync Folders section.