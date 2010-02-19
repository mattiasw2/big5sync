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
        List<Tag> GetAllTags();
        List<Tag> GetAllTags(FileInfo file);
        List<Tag> GetAllTags(DirectoryInfo info);

        FileTag CreateFileTag(string tagname);
        FolderTag CreateFolderTag(string tagname);

        FileTag TagFile(string tagname, FileInfo file);
        FileTag TagFile(FileTag tag, FileInfo file);
        FolderTag TagFolder(string tagname, DirectoryInfo folder);
        FolderTag TagFolder(FolderTag tag, DirectoryInfo file);

        int UntagFile(FileTag tag, FileInfo file);
        int UntagFolder(FolderTag tag, DirectoryInfo folder);

        bool DeleteTag(Tag tag);

        // deprecated
        //bool DeleteTag(FolderTag tag);
        //bool DeleteTag(FileTag tag);

        bool DeleteAllTags(); // Delete all existing tags (This one is like a general reset, might not need)
        bool DeleteAllTags(FileInfo file); // delete all tags associated with a file
        bool DeleteAllTags(DirectoryInfo folder); // delete all tags associated with a directory

        bool StartManualSync(Tag tagname);

        bool MonitorTag(Tag tag, bool mode);

        bool SetTagBidirectional(FileTag tag);
        bool SetTagBidirectional(FolderTag tag);

        List<CompareResult> PreviewSync(FolderTag tag);
        List<CompareResult> PreviewSync(FileTag tag);

        //Log ViewLog(LogSettings log);

        bool PrepareForTermination();
        bool Terminate();

        bool Initiate();

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
