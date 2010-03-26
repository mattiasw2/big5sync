using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
using Syncless.CompareAndSync;
using System.IO;
using Syncless.Filters;
using Syncless.CompareAndSync.CompareObject;
using Syncless.Core.View;
using Syncless.Logging;
namespace Syncless.Core
{
    public interface IUIControllerInterface
    {
        List<string> GetAllTags();
        List<string> GetTags(DirectoryInfo folder);

        TagView GetTag(string tagname);       

        bool DeleteTag(string tagname);

        TagView CreateTag(string tagname);
        TagView Tag(string tagname, DirectoryInfo folder);
                
        int Untag(string tagname, DirectoryInfo folder);
        
        bool MonitorTag(string tagname, bool mode);

        bool PrepareForTermination();
        bool Terminate();

        bool Initiate(IUIInterface inf);

        //bool RenameTag(string oldtagname, string newtagname);

        bool StartManualSync(string tagname);
        
        bool UpdateFilterList(string tagname, List<Filter> filterlist);

        List<Filter> GetAllFilters(string tagname);

        RootCompareObject PreviewSync(string tagname);

        bool AllowForRemoval(DriveInfo drive);

        int Clean(string path);

        bool SetProfileName(string name);
        string GetProfileName();

        List<LogData> ReadLog();

        //bool SetTagMultiDirectional(string tagname);

        // To be Implemented

        // bool DeleteAllTags(); // Delete all existing tags (This one is like a general reset, might not need)
        // bool DeleteAllTags(FileInfo file); // delete all tags associated with a file
        // bool DeleteAllTags(DirectoryInfo folder); // delete all tags associated with a directory

		//Log ViewLog(LogSettings log);

		// 8) Set Uni-direction/Source (Tentative - 0.9)
		// bool SetUnidirectional(FileTag, FileInfo file);
		// bool SetUnidirectional(FolderTag, DirectoryInfo folder);

		// 11) Rename Tag (Tentative - 0.9)
		// FolderTag RenameTag(Folder tag);
		// FileTag RenameTag(File tag);

		// 12) Schedule Sync (Tentative - 2.0)
		// bool Sync(FolderTag tag, ScheduleSettings settings); 
		// bool Sync(FileTag tag, ScheduleSettings settings);

		// 13) Update Tag Settings (Like Inclusion-Exclusion/) (Tentative - 0.9)
		// bool UpdateTag(FolderTag tag, TagSettings settings);
		// bool UpdateTag(FileTag tag, TagSettings settings);

		// 17) View Versioning (Tentative - 0.9)
		// ViewAllVersion();
		// RestoreVersion();
	
        // TO CHANGE LATER
        /*
        void GetAllConnectedDrives();
        void Preview(Tag tag);

        */
    }
}
