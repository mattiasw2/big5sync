

### What is Syncless ? ###

Syncless is a tagged-based file Synchronization tool that provides seamless synchronization for multiple folders.


### My Syncless fails to initialize. ###

There are many causes for Syncless to fail to initialize. Some of the possible cases are :
  1. Syncless does not have read/write access to some of the hard disk drives. Syncless requires read/write access on the root folder of the disk that you tag.
  1. Syncless requires .Net Framework 3.5 **SP1** to run. Please ensure that you have the required version installed.

### The profiles refuse to die after deleting the profile files. ###

Syncless saves profile in many areas to assist in merging of the profiles across multiple computers and thumbdrives. It also allows Syncless to recover some of your tags in case profiles are not saved properly. Profiles are saved in the following locations:
  1. Application folder of Syncless;
  1. All drives that are identified by Syncless. It is contained in the folder .syncless (i.e C:\.syncless)

The list of profiles contains tagging.xml and profiling.xml.

However, you should not delete the profile directly.

### OMG The tags are merging when I don't want them to ###

Syncless currently does not provide the user an option to change the profile name. However, if you are interested in helping us test the feature, you can modify tagging.xml and profiling.xml directly and changing the profile name to the one you like. It will then mean that only profiles with the same name will be merged. However, this feature is still in testing mode and may cause unexpected results.

### Who are we ? ###

We are a group of students from [National University of Singapore](http://www.nus.edu.sg) who took a software engineering module, and this is our school project. We decide that it should not be just a school project, so we took a step ahead and created Syncless.