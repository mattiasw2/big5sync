using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    static class LogMessage
    {
        internal static string FOLDER_TAG_CREATED = "Folder tag \'{0}\' created.";
        internal static string FOLDER_TAG_RENAMED = "Folder tag \'{0}\' renamed to \'{1}\'.";
        internal static string FOLDER_TAG_REMOVED = "Folder tag \'{0}\' removed.";
        internal static string FOLDER_TAGGED = "Folder \'{0}\' tagged to folder tag \'{1}\'.";
        internal static string FOLDER_UNTAGGED = "Folder \'{0}\' untagged from folder tag \'{1}\'.";
        internal static string FOLDER_NOT_UNTAGGED = "Unable to untag folder \'{0}\' from folder tag \'{1}\'.";
        internal static string FOLDER_RENAMED = "Folder \'{0}\' renamed to \'{1}\'.";
        internal static string FOLDER_TAG_NOT_FOUND = "Folder tag \'{0}\' cannot be found.";
        internal static string FOLDER_TAG_ALREADY_EXISTS = "Folder tag \'{0}\' already exists.";
        internal static string FOLDER_TAG_CONFLICT = "Attempt to tag file \'{0}\' to folder tag \'{1}\'.";
        internal static string PATH_NOT_FOUND_IN_FOLDER_TAG = "Path \'{0}\' not found in folder tag \'{1}\'.";
        internal static string PATH_ALREADY_EXISTS_IN_FOLDER_TAG = "Path \'{0}\' already exists in folder tag \'{1}\'.";
        internal static string RECURSIVE_DIRECTORY = "Attempt to tag parent/sub directory \'{0}\' of existing directory.";

        internal static string FILE_TAG_CREATED = "File tag \'{0}\' created.";
        internal static string FILE_TAG_RENAMED = "File tag \'{0}\' renamed to \'{1}\'.";
        internal static string FILE_TAG_REMOVED = "File tag \'{0}\' removed.";
        internal static string FILE_TAGGED = "File \'{0}\' tagged to file tag \'{1}\'.";
        internal static string FILE_UNTAGGED = "File \'{0}\' untagged from file tag \'{1}\'.";
        internal static string FILE_NOT_UNTAGGED = "Unable to untag file \'{0}\' from file tag \'{1}\'.";
        internal static string FILE_RENAMED = "File \'{0}\' renamed to \'{1}\'.";
        internal static string FILE_TAG_NOT_FOUND = "File tag \'{0}\' cannot be found.";
        internal static string FILE_TAG_ALREADY_EXISTS = "File tag \'{0}\' already exists.";
        internal static string FILE_TAG_CONFLICT = "Attempt to tag folder \'{0}\' to file tag \'{1}\'.";
        internal static string PATH_NOT_FOUND_IN_FILE_TAG = "Path \'{0}\' not found in file tag \'{1}\'.";
        internal static string PATH_ALREADY_EXISTS_IN_FILE_TAG = "Path \'{0}\' already exists in file tag \'{1}\'.";

        internal static string PATH_NOT_FOUND = "Path \'{0}\' does not exist.";
        internal static string PATH_ALREADY_EXISTS = "Path \'{0}\' already exists.";
    }
}
