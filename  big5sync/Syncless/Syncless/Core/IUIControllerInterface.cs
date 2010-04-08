using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core.Exceptions;
using Syncless.Tagging;
using Syncless.CompareAndSync;
using System.IO;
using Syncless.Filters;
using Syncless.Core.View;
using Syncless.Logging;
using Syncless.Tagging.Exceptions;

namespace Syncless.Core
{
    public interface IUIControllerInterface
    {
        /// <summary>
        /// Return a list contains only the name of all the tags.
        /// </summary>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The List containing the name of the tags</returns>
        List<string> GetAllTags();
        /// <summary>
        /// Return a list of tag name that a folder is tagged to.
        /// </summary>
        /// <param name="folder">The folder to find</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The List containing the name of the tags that the folder is tag to.</returns>
        List<string> GetTags(DirectoryInfo folder);
        /// <summary>
        /// Get a <see cref="TagView"/> Representation of a <see cref="Tag"/>
        /// </summary>
        /// <param name="tagname">The name of the tag to get.</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The <see cref="TagView"/> representing the <see cref="Tag"/> Object.</returns>
        TagView GetTag(string tagName);
        /// <summary>
        /// Delete a tag
        /// </summary>
        /// <param name="tagName">Name of the tag to delete</param>
        /// <returns>true if a tag is removed. false if the tag cannot be removed(i.e Currently Synchronizing)</returns>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        bool DeleteTag(string tagName);
        /// <summary>
        /// Create a Tag.
        /// </summary>
        /// <param name="tagName">name of the tag.</param>
        /// <returns>The Detail of the Tag.</returns>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <exception cref="TagAlreadyExistsException">Tag with tagname already exist.</exception>
        TagView CreateTag(string tagName);
        /// <summary>
        /// Tag a Folder to a Tag based on the tag name
        /// </summary>
        /// <param name="tagname">name of the tag</param>
        /// <param name="folder">the folder to tag</param>
        /// <exception cref="InvalidPathException">The Path is invalid</exception>
        /// <exception cref="RecursiveDirectoryException">Tagging the folder will cause a recursive during Synchronization.</exception>
        /// <exception cref="PathAlreadyExistsException">The Path already exist in the Tag.</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The Tag View representing the tag. return null , if the tag fail.</returns>
        TagView Tag(string tagName, DirectoryInfo folder);
        /// <summary>
        /// Untag the folder from a tag. 
        /// </summary>
        /// <param name="tagname">The name of the tag</param>
        /// <param name="folder">The folder to untag</param>
        /// <exception cref="TagNotFoundException">The tag is not found.</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The number of path untag</returns>  
        int Untag(string tagName, DirectoryInfo folder);
        /// <summary>
        /// Switch the mode of a particular Tag to a mode.
        /// Valid Mode are <see cref="TagMode.Seamless"/> and <see cref="TagMode.Manual"/>
        /// </summary>
        /// <param name="name">The name of the tag to switch</param>
        /// <param name="mode">The mode to switch to</param>
        /// <exception cref="ArgumentOutOfRangeException">If the TagMode Specified is not a valid TagMode</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if the mode can be switch.</returns>
        bool SwitchMode(string tagName , TagMode mode);
        /// <summary>
        /// Get the current Tag of a particular tag.
        /// See <see cref="TagState"/> for a list of TagState
        /// </summary>
        /// <param name="tagname">The name of the tag</param>
        /// <exception cref="TagNotFoundException"><see cref="Tag"/> with the given tagname  is not found.</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns><see cref="TagState"/> representing the state of the Tag.</returns>
        TagState GetTagState(string tagName);
        /// <summary>
        /// Prepare the core for termination.
        /// </summary>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if the program can terminate, false if the program is not ready for termination</returns>
        bool PrepareForTermination();
        /// <summary>
        /// Terminate the program. 
        /// This is to kill the threads created and release the resources.
        /// Everything will be terminated. 
        /// If CompareAndSyncController have a job running, it will be completed before the program send a Terminate Notification to the UI.
        /// </summary>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        void Terminate();
        /// <summary>
        /// Initiate the program. This is the first command that needs to be run
        /// If this method is not run before any other method is call, the system might fail.
        /// </summary>
        /// <param name="inf">The User Interface that implements the <see cref="IUIInterface"/></param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if the Logic Layer successfully initialized. false if the program fail to initialize</returns>
        bool Initiate(IUIInterface inf);
        /// <summary>
        /// Starts a Manual Sync. The Sync will be queued and will be processed when it is its turn.
        /// </summary>
        /// <param name="tagname">Tagname of the Tag to sync</param>
        /// <returns>true if the sync is successfully queued. false if the tag is currently being queued/sync or the tag does not exist.  </returns>        
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        bool StartManualSync(string tagName);
        /// <summary>
        /// Cancel a Manual Sync.
        /// </summary>
        /// <param name="tagName">Tagname of the Tag to sync</param>
        /// <returns>true if the sync is cancel. false if the tag is not currently being queued/sync or the request cannot be cancel.</returns>
        /// <exception cref="UnhandledException">Unhandled Exception</exception> 
        bool CancelManualSync(string tagName);
        /// <summary>
        /// Update the filterlist of a <see cref="Tag"/>
        /// </summary>
        /// <param name="tagname">tagname of the <see cref="Tag"/></param>
        /// <param name="filterlist">the list of filter to set to the tag.</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if succeed, false if fail.</returns>
        bool UpdateFilterList(string tagName, List<Filter> filterlist);
        /// <summary>
        /// Get all the filters for a particular <see cref="Tag"/>
        /// </summary>
        /// <param name="tagname">the name of the tag.</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>the list of <see cref="Filter">Filters</see></returns>
        List<Filter> GetAllFilters(string tagName);
        /// <summary>
        /// Preview a Sync of a Tag 
        /// </summary>
        /// <param name="tagName">The name of the tag to preview</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The RootCompareObject representing the Preview Result. null if the tag does not exist.</returns>
        RootCompareObject PreviewSync(string tagName);
        void CancelPreview(string tagName);
        /// <summary>
        /// Call to release a drive so that it can be safety remove.
        /// </summary>
        /// <param name="drive">the Drive to remove</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if succeess , false if fail.</returns>
        bool AllowForRemoval(DriveInfo drive);
        /// <summary>
        /// Clean the .syncless metadata in the selected path
        /// </summary>
        /// <param name="path">the path of the directory</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>number of .syncless removed.</returns>
        int Clean(string path);
        /// <summary>
        /// Return the user log 
        /// </summary>
        /// <exception cref="LogFileCorruptedException">The log file is corrupted.</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>the list of log data.</returns>
        List<LogData> ReadLog();
        /// <summary>
        /// Clear the user log
        /// </summary>
        void ClearLog();
    }

}
