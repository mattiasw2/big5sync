

# Overview #

The **Tagging** component provides the underlying operations of creating and removing tags, as well as tagging and untagging folders from tags. It also manages other settings such as the filter settings per tag.

# Classes #

![![](http://big5sync.googlecode.com/files/Tagging%20-%20v2%20-%20thumb.png)](http://big5sync.googlecode.com/files/Tagging%20-%20v2.png)

The Tagging component consists of several important sub-components, including a main logic layer - `TaggingLayer`, model objects - `TaggingProfile`, `Tag`, `TaggedPath` and `TagConfig`, helper components - `TagMerger`, `TaggingHelper` and `TaggingXMLHelper`.

## TaggingLayer ##

This is the main logic layer which is called by the `SystemLogicLayer` to perform tag-related operations. When **Syncless** is started up, `SystemLogicLayer` will call `TaggingLayer` for initialization.

When initialized, `TaggingLayer` will call `TaggingXMLHelper` to load the current tagging profile from _tagging.xml_ in the computer. If there are more than one _tagging.xml_ files available (this may happen if there are removable drives plugged into the computer), `TaggingLayer` will call `TagMerger` to perform merging of several tagging profiles based on various sources of _tagging.xml_.

### Sequence Diagram of Init ###

![http://big5sync.googlecode.com/files/TaggingInitResized.jpg](http://big5sync.googlecode.com/files/TaggingInitResized.jpg)

After initialization, `TaggingLayer` can be called to perform tag-related operations.

The main operations which will trigger interactions among the various classes in Tagging component include:
  1. `CreateTag`
  1. `DeleteTag`
  1. `TagFolder`
  1. `UntagFolder`
  1. `UpdateFilter`

### Sequence Diagram of CreateTag ###

![http://big5sync.googlecode.com/files/TaggingCreateTag.jpg](http://big5sync.googlecode.com/files/TaggingCreateTag.jpg)

### Sequence Diagram of DeleteTag ###

![http://big5sync.googlecode.com/files/TaggingDeleteTag.jpg](http://big5sync.googlecode.com/files/TaggingDeleteTag.jpg)

### Sequence Diagram of TagFolder ###

![http://big5sync.googlecode.com/files/TaggingTagFolder.jpg](http://big5sync.googlecode.com/files/TaggingTagFolder.jpg)

### Sequence Diagram of UntagFolder ###

![http://big5sync.googlecode.com/files/TaggingUntagFolder.jpg](http://big5sync.googlecode.com/files/TaggingUntagFolder.jpg)

### Sequence Diagram of UpdateFilter ###

![http://big5sync.googlecode.com/files/TaggingUpdateFilter.jpg](http://big5sync.googlecode.com/files/TaggingUpdateFilter.jpg)

## TaggingXMLHelper ##

This is a helper which performs XML-related operations. It reads from and writes to _tagging.xml_ which stores information of all tags and their configuration.

The conversions from a tagging profile to _tagging.xml_, and vice versa, are easily extensible. There are separate methods which take care of reading/writing each tagging profile property. If there are more properties to be added to `TaggingProfile`, only `ConvertTaggingProfileToXml` and `ConvertXmlToTaggingProfile` need to be changed, and then just create additional methods which read/write each of the new properties.

Conversion from _tagging.xml_ to `TaggingProfile`:
  1. `ConvertXmlToTaggingProfile`(`XmlDocument`)
  1. `CreateTaggingProfile`(`XmlElement`)
  1. `CreateTagFromXml`(`XmlElement`)
  1. `CreateFolders`(`XmlElement`)
  1. `CreatePath`(`XmlElement`)
  1. `CreateTagConfig`(`XmlElement`)
  1. `LoadFilterList`(`XmlElement`)
  1. `LoadFilter`(`XmlElement`)

Conversion from `TaggingProfile` to _tagging.xml_:
  1. `ConvertTaggingProfileToXml`(`TaggingProfile`)
  1. `CreateTagElement`(`XmlDocument`, `Tag`)
  1. `CreateFoldersElements`(`XmlDocument`, `Tag`)
  1. `CreateTaggedFolderElement`(`XmlDocument`, `TaggedPath`)
  1. `CreateConfigElement`(`XmlDocument`, `Tag`)
  1. `CreateFilterElementList`(`XmlDocument`, `Tag`)
  1. `CreateFilterElement`(`XmlDocument`, `Filter`)

# Description of Metadata File #

The Tagging component makes use of metadata with file name _tagging.xml_ to keep track of the list of tags user has modified.

## General Structure ##

```
<tagging>
  <profile name="School" createdDate="120420101755" lastUpdatedDate="120420101819">
    <tag name="Lecture" createdDate="120420101803" lastUpdatedDate="120420101819" isDeleted="False" deletedDate="0">
      <folders>
        <taggedFolder createdDate="120420101809" lastUpdatedDate="120420101819" isDeleted="False" deletedDate="0">
          <path>001:\Lecture Notes</path>
        </taggedFolder>
        <taggedFolder createdDate="120420101813" lastUpdatedDate="120420101813" isDeleted="False" deletedDate="0">
          <path>001:\Notes</path>
        </taggedFolder>
      </folders>
      <filters lastUpdatedDate="120420101819">
        <filter mode="Include" type="Extension">
          <pattern>*.pdf</pattern>
        </filter>
        <filter mode="Include" type="Extension">
          <pattern>*.doc</pattern>
        </filter>
        <filter mode="Exclude" type="Extension">
          <pattern>*.htm</pattern>
        </filter>
      </filters>
      <config>
        <seamless>False</seamless>
      </config>
    </tag>
  </profile>
</tagging>
```

## Explanation of Xml Tag Names ##

| **Tag name/value** | **Description** |
|:-------------------|:----------------|
| tagging            | Represents the tag name for tagging.xml root element |
| profile            | Represents the tag name for profile root element |
| name               | Represents the attribute name for profile name attribute |
| createdDate        | Represents the attribtue name for profile created date attribute |
| lastUpdatedDate    | Represents the attribute name for profile last updated date attribute |
| tag                | Represents the tag name for tag root element |
| name               | Represents the attribute name for tag name attribute |
| createdDate        | Represents the attribute name for tag created date attribute |
| lastUpdatedDate    | Represents the attribute name for tag last updated date attribute |
| isDeleted          | Represents the attribute name for tag is deleted attribute |
| deletedDate        | Represents the attribute name for tag deleted date attribute |
| folders            | Represents the tag name for folders root element |
| taggedFolder       | Represents the tag name for tagged folder root element |
| createdDate        | Represents the attribute name for tagged folder created date attribute |
| lastUpdatedDate    | Represents the attribute name for tagged folder last updated date attribute |
| isDeleted          | Represents the attribute name for tagged folder is deleted date attribute |
| deletedDate        | Represents the attribute name for tagged folder deleted date attribute |
| path               | Represents the tag name for tagged folder path element |
| filters            | Represents the tag name for filter root element |
| filter             | Represents the tag name for filter child element |
| lastUpdatedDate    | Represents the attribute name for filter last updated date attribute |
| type               | Represents the attribute name for filter type attribute |
| Extension          | Represents the attribute value for filter type extension attribute |
| mode               | Represents the attribute name for filter mode attribute |
| Include            | Represents the attribute value for include filter mode attribute |
| Exclude            | Represents the attribute value for exclude filter mode attribute |
| pattern            | Represents the tag name for filter type extension pattern element |
| config             | Represents the tag name for config root element |
| seamless           | Represents the tag name for config seamless element |

# Explanation of Algorithms #

## Merging Multiple _tagging.xml_ ##

Merging of multiple _tagging.xml_ files is necessary when the user maintains tags which contain folders across multiple devices. For example, the user may tag a folder in his home workstation and a folder in his office workstation to the same tag, and uses a thumbdrive as the intermediary device to sync the two folders. In this case, both his home workstation and office workstation will contain _tagging.xml_ files, as well as in the thumbdrive.

## What Properties are Checked ##

The system recognizes the differences in _tagging.xml_ files found in various sources through the following properties:
  1. `ProfileName` in `TaggingProfile`
  1. `TagName` in `Tag`
  1. `CreatedDate` in `Tag`
  1. `LastUpdatedDate` in `Tag`
  1. `IsDeleted` in `Tag`
  1. `CreatedDate` in `TaggedPath`
  1. `LastUpdatedDate` in `TaggedPath`
  1. `IsDeleted` in `TaggedPath`

## When to Merge ##

Merging of different _tagging.xml_ files will be done only if they contain the same tagging profile, ie. the name of the profile contained in all files is the same.

## How to Merge ##

### Merging the Profile ###

We denote the profile that **Syncless** is currently working on as _currentProfile_, and any other profile detected during drive-in/drive-out event will be denoted as _newProfile_. _currentProfile_ and _newProfile_ should share the same profile name.

For each _newProfile_, the system will attempt to merge _currentProfile_ with _newProfile_.

Let's take a look at the pseudo-code for the merging of profiles:

```
  if currentProfile is null or newProfile is null
      return -1
  if currentProfile and newProfile have different profile names
      return -1
  for each newTag in taglist of newProfile
      find the currentTag with the same name in taglist of currentProfile
      if currentTag is found
          merge currentTag with newTag
          if merge is successful
              increment counter
      else
          add newTag to taglist of TaggingLayer
          add newTag to monitorList
          increment counter
  for each tag in monitorList
      enqueue AddTagNotification to LogicLayerNotificationQueue
  return counter
```

The counter indicates the number of tags merged or added. The `LogicLayerNotificationQueue` is a queue in the main logic layer accessed through the `ServiceLocator`. For merging of tags, the algorithm will be explained in the following section.

### Merging the Tag ###

For clearer illustration, we denote the tag found in _newProfile_ as _newTag_ and the tag found in _currentProfile_ as _currentTag_.

For each _newTag_, if it has not already existed in _currentProfile_, it indicates that _newTag_ has been created elsewhere and should be added to _currentProfile_ so that details of _newTag_ can be displayed to the user. If _newTag_ exists in _currentProfile_, the system will do further check whether any modification has been made to the equivalent _currentTag_.

Let's take a look at the pseudo-code for the merging the tags:

```
  if currentTag and newTag share the same name && both tags were modified at different time
      if newTag was deleted && currentTag exists
          if deleted date of newTag is later than created date of currentTag
              enqueue RemoveTagNotification for newTag to LogicLayerNotificationQueue
              return true
          else
              return false
      if currentTag was deleted && newTag exists
          if created date of newTag is later than deleted date of currentTag
              add newTag to TaggingLayer
              enqueue AddTagNotification for newTag to LogicLayerNotificationQueue
              return true
      for each tagged path in newTag
          merge paths from currentTag and newTag
      return true
  else
      return false
```

Basically, the system will check whether a tag is marked as deleted in one profile but exists in another profile. If that is the case, then further checks will be done on the creation time and deletion time in both tags to decide whether to add the _newTag_ to the _currentProfile_ or to delete the _currentTag_ from the _currentProfile_. If _newTag_ is added to _currentProfile_, the system needs to check whether there are any changes to the list of tagged folders in both tags.

For each tag added, a `AddTagNotification` will be enqueued containing the name of the tag. Likewise, for each tag removed, a `RemoveTagNotification` will be enqueued containing the name of the tag.

For merging of tagged paths, the algorithm will be explained in the following section.

### Merging the TaggedPath ###

For clearer illustration, we will denote the path found in _newTag_ as _newPath_ and the path found in _currentTag_ as _currentPath_.

Let's take a look at the pseudo-code for the merging of paths:

```
  if newPath is not found in currentTag
      enqueue MonitorPathNotification for newPath in currentTag to LogicLayerNotificationQueue
      add newPath to currentTag
  else
     if newPath is updated at a later time than the updated time of currentPath
         if newPath was deleted && currentPath exists
             if newPath was deleted later than the creation of currentPath
                 remove currentPath from currentTag
                 enqueue UnMonitorPathNotification for currentPath in currentTag to LogicLayerNotificationQueue
         else if currentPath was deleted && newPath exists
             if newPath was created later than the deletion of currentPath
                 add newPath to currentTag
                 enqueue MonitorPathNotification for newPath in currentTag to LogicLayerNotificationQueue
```

The system first checks whether _newPath_ exists in _currentTag_, if it does not exist, then _newPath_ should be added to _currentTag_. If _newPath_ exists in _currentTag_, then the system needs to check whether there are changes made to the tagged path.

If the updated time of _newPath_ is earlier than that of _currentPath_, the system will not do anything. Otherwise, further checks will be done on the deletion time and creation time of both paths and perform addition/removal of path accordingly.

For each tagged path added, a `MonitorPathNotification` will be enqueued. The notification will contain the path name to be monitored and the tag name of the tag it belongs to. Likewise, for each tagged path removed, a `UnmonitorPathNotification` will be enqueued. The notification will contain the path name to be unmonitored and the tag name of the tag it belongs to.

# Extending Tagging #

As a tag object is configurable, developers can extend the `TagConfig` classes to include more properties. Currently, existing property includes `IsSeamless`.