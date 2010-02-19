using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
using Syncless.CompareAndSync;
using System.IO;
namespace Syncless.Core
{
    public interface IUIControllerInterface
    {
        List<string> GetAllTags();

        TagView GetTag(String tagname);

        bool StartManualSync(String tagname);

        bool DeleteTag(String tagname);

        FileTagView CreateFileTag(String tagname);

        FolderTagView CreateFolderTag(String tagname);

        FileTagView TagFile(string tagname, FileInfo file);
        FolderTagView TagFolder(string tagname, DirectoryInfo folder);

        int UntagFile(string tagname, FileInfo file);
        int UntagFolder(string tagname, DirectoryInfo folder);

        bool MonitorTag(string tagname, bool mode);

        bool SetTagMultiDirectional(string tagname);

        bool PrepareForTermination();
        bool Terminate();

        bool Initiate();

		List<CompareResult> PreviewSync(FolderTag tag);
		List<CompareResult> PreviewSync(FileTag tag);

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
