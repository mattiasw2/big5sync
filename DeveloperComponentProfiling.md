

# Overview #

The **Profiling** component performs drive/computer recognization. It makes **Syncless** truly portable because of its ability to recognize different drives/computers by creating a Globally Unique IDentifier (GUID) in each of them. It also performs conversion of a physical directory/file path to a logical address, and vice versa. This conversion is necessary, because **CompareAndSync** only recognizes physical path while **Tagging** recognizes a logical path.

# Classes #

![![](http://big5sync.googlecode.com/files/Profiling%20-%20v2%20-%20thumb.png)](http://big5sync.googlecode.com/files/Profiling%20-%20v2.png)

The Profiling component consists of several important sub-components, including a main logic layer - `ProfilingLayer`, model objects - `Profile` and `ProfileMapping`, helper components - `ProfileMerger`, `ProfilingHelper`,`ProfilingXMLHelper` and `ProfilingGUIDHelper`.

## ProfilingLayer ##

This is the main logic layer which is called by the `SystemLogicLayer` to perform profiling-related operations. When Syncless is started up, `SystemLogicLayer` will call `ProfilingLayer` for initialization.

When initialized, `ProfilingLayer` will call `ProfileXMLHelper` to load the default drive/computer profile from _profiling.xml_ in the computer. If there are more than one _profiling.xml_ files available, `ProfilingLayer` will call `ProfileMerger` to perform merging of several drive/computer profiles based on the various sources of _profiling.xml_.

After initilization, `ProfilingLayer` can be called to perform profiling-related operations when a drive-in/drive out event is detected.

When a drive-in event is detected, `ProfilingLayer` will create a mapping profile based on the _guid.id_ contained in the new drive, and perform a merge of the new mapping profile with the current working profile.

Likewise, when a drive-out event is detected, `ProfilingLayer` will remove the mapping profile of that particular drive and mark it as unavailable.

# Description of Metadata File #
## General Structure ##

```
<profiling> 
  <profile profilename="" last_updated="0">
    <drive guid="ba7c9b4c-5c72-409b-9455-422f192a79a6" drivename="-" last_updated="0" />
  </profile>
</profiling>
```

## Explanation of Xml Tag Names ##
| **Tag name/value** | **Description** |
|:-------------------|:----------------|
| profiling          | the root node of the xml |
| profile            | the node representing a profile object. can contain multiple profile |
| profilename        | the name of the profile |
| last\_updated      | the last updated time of the profile |
| drive              | the node representing a drive captured by the system |
| guid               | the guid of the drive. (A unique Identifier) |
| drivename          | the name of the profiledrive |
| last\_updated      | the updated time of the drive |


# Explanation of Algorithms #

## Merging Multiple _profiling.xml_ ##

Profile from many computers will be merged. Currently, logicalId and GUID is the same. Therefore we will do a direct merge. As long as the guid and logicalid does not have conflict with the existing mapping, it will be added to the current profile.