`[`**[Prev - Syncless.Profiling.Exceptions](DeveloperAPIProfilingExceptions.md)** `|` **[Next - Syncless.Tagging.Exceptions](DeveloperAPITaggingExceptions.md)**`]`

Provides classes necessary for creating/removing tags, tagging/untagging folders, managing other settings such as the filter settings per tag.

# Classes Summary #

| | **Class** | **Description** |
|:|:----------|:----------------|
| ![http://big5sync.googlecode.com/files/pubclass.gif](http://big5sync.googlecode.com/files/pubclass.gif) | [Tag](#Tag.md) | Represents a container for a list of TaggedPath objects.<br>Each Tag object has its properties that uniquely identifies itself from other Tag objects. <br>
<tr><td> <img src='http://big5sync.googlecode.com/files/pubclass.gif' /> </td><td> <a href='#TaggingLayer.md'>TaggingLayer</a> </td><td> The main logic controller of the Tagging namespace. </td></tr>
<tr><td> <img src='http://big5sync.googlecode.com/files/pubclass.gif' /> </td><td> <a href='#TaggingProfile.md'>TaggingProfile</a> </td><td> Represents a container for a list of Tag objects.<br>Each TaggingProfile has its properties that uniquely identifies itself from other TaggingProfile objects. </td></tr>
<tr><td> <img src='http://big5sync.googlecode.com/files/pubclass.gif' /> <img src='http://big5sync.googlecode.com/files/static.gif' /> </td><td> <a href='#TagMerger.md'>TagMerger</a> </td><td> Performs merging of several tagging profiles which have the same name. </td></tr></tbody></table>


---


## Tag ##

| | **Constructor** | **Description** |
|:|:----------------|:----------------|
| ![http://big5sync.googlecode.com/files/pubmethod.gif](http://big5sync.googlecode.com/files/pubmethod.gif) | Tag(string, long) | Creates a new Tag object. |

| | **Property** | **Description** |
|:|:-------------|:----------------|
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | CreatedDate  | Gets or sets the created date of the tag. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | DeletedDate  | Gets or sets the deleted date of the tag. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | FilteredPathList | Gets a clone of the list of tagged paths whose IsDeleted property is set to false. Sets the list of tagged paths. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | FilteredPathListString | Gets a clone of the list of full path name of tagged paths whose IsDeleted property is set to false. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | Filters      | Gets or sets the list of filters of the tag. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | FiltersUpdatedDate | Gets or sets the updated date of the filters of the tag. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | IsDeleted    | Gets or sets the boolean value that represents whether the tag is deleted. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | IsSeamless   | Gets or sets the boolean value which represents whether the tag is in seamless mode. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | LastUpdatedDate | Gets or sets the last updated date of the tag. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | ReadOnlyFilters | Gets a clone of the list of filters of the tag. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | TagName      | Gets or sets the name of the tag. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | UnfilteredPathList | Gets or sets the list of tagged paths. |

[Back to top](#Classes_Summary.md)


---


## TaggedPath ##

| | **Constructor** | **Description** |
|:|:----------------|:----------------|
| ![http://big5sync.googlecode.com/files/pubmethod.gif](http://big5sync.googlecode.com/files/pubmethod.gif) | TaggedPath(string, long) | Creates a new TaggedPath object. |

| | **Property** | **Description** |
|:|:-------------|:----------------|
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | CreatedDate  | Gets or sets the created date of the tagged path. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | DeletedDate  | Gets or sets the deleted date of the tagged path. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | IsDeleted    | Gets or sets the boolean value that represents whether the tagged path is deleted. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | LastUpdatedDate | Gets or sets the last updated date of the tagged path. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | LogicalDriveId | Gets or sets the logical drive ID of the tagged path. |
| ![http://big5sync.googlecode.com/files/pubproperty.gif](http://big5sync.googlecode.com/files/pubproperty.gif) | PathName     | Gets or sets the full path name of the tagged path. |

| | **Method** | **Description** |
|:|:-----------|:----------------|
| ![http://big5sync.googlecode.com/files/pubmethod.gif](http://big5sync.googlecode.com/files/pubmethod.gif) | Append(string) | Combines the full path name of the tagged path with the trailing path that is passed as parameter. |
| ![http://big5sync.googlecode.com/files/pubmethod.gif](http://big5sync.googlecode.com/files/pubmethod.gif) | Remove(long) | Sets the boolean value that represents whether the tagged path is deleted to true. |
| ![http://big5sync.googlecode.com/files/pubmethod.gif](http://big5sync.googlecode.com/files/pubmethod.gif) | Rename(string) | Sets the full path name of the tagged path to the new path that is passed as parameter. |
| ![http://big5sync.googlecode.com/files/pubmethod.gif](http://big5sync.googlecode.com/files/pubmethod.gif) | Replace(string, string) | Sets part of the tagged path represented by the old path that is passed as parameter to the new path that is passed as parameter. |

[Back to top](#Classes_Summary.md)


---


## TaggingLayer ##

| **Method** | **Description** |
|:-----------|:----------------|
| AddTag(Tag) |                 |
| AppendProfile(List`<string>`) |                 |
| CheckIDExists(string) |                 |
| CreateTag(string) |                 |
| DeleteTag(string) |                 |
| FindSimilarPathForFolder(string) |                 |
| GetAllPaths() |                 |
| Init(List`<string>`) |                 |
| Merge(string) |                 |
| RenameFolder(string, string) |                 |
| RenameTag(string, string) |                 |
| RetrieveAllTags(bool) |                 |
| RetrieveAncestors(string) |                 |
| RetrieveDescendants(string) |                 |
| RetrieveFilteredTagByLogicalId(string) |                 |
| RetrieveParentTagByPath(string) |                 |
| RetrievePathByLogicalId(string) |                 |
| RetrieveTag(string) |                 |
| RetrieveTag(string, bool) |                 |
| RetrieveTagByLogicalId(string) |                 |
| RetrieveTagByPath(string) |                 |
| SaveTo(List`<string>`) |                 |
| TagFolder(string, string) |                 |
| UntagFolder(string, string) |                 |
| UntagFolder(string) |                 |
| UpdateFilter(string, List`<Filter>`) |                 |

[Back to top](#Classes_Summary.md)


---


## TaggingProfile ##

[Back to top](#Classes_Summary.md)


---


## TagMerger ##

[Back to top](#Classes_Summary.md)

`[`**[Prev - Syncless.Profiling.Exceptions](DeveloperAPIProfilingExceptions.md)** `|` **[Next - Syncless.Tagging.Exceptions](DeveloperAPITaggingExceptions.md)**`]`