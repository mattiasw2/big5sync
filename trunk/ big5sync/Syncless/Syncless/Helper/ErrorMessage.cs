using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Helper
{
    public static class ErrorMessage
    {
        #region Core Error Messages (Error Code : 000~099)
        public const string LOGGER_NOT_FOUND = "000";
        public const string INVALID_PATH = "001";
        #endregion
        #region Tagging Error Messages (Error Code : 100~199)
        /// <summary>
        /// The string value that represents the error of path not found
        /// </summary>
        public const string PATH_NOT_FOUND_EXCEPTION = "100";

        /// <summary>
        /// The string value that represents the error of path already exists
        /// </summary>
        public const string PATH_ALREADY_EXISTS_EXCEPTION = "110";

        /// <summary>
        /// The string value that represents the error of tag not found
        /// </summary>
        public const string TAG_NOT_FOUND_EXCEPTION = "101";

        /// <summary>
        /// The string value that represents the error of tag already exists
        /// </summary>
        public const string TAG_ALREADY_EXISTS_EXCEPTION = "111";

        /// <summary>
        /// The string value that represents the error of recursive tagging
        /// </summary>
        public const string RECURSIVE_DIRECTORY_EXCEPTION = "120";

        //public const string TAG_TYPE_CONFLICT_EXCEPTION = "130";
        
        /// <summary>
        /// The string value that represents the error of filter not found
        /// </summary>
        public const string FILTER_NOT_FOUND_EXCEPTION = "140";

        /// <summary>
        /// The string value that represents the error of filter already exists
        /// </summary>
        public const string FILTER_ALREADY_EXISTS_EXCEPTION = "141";
        #endregion
        #region Profiling Error Messages (Error Code : 200~299)
        public const string PROFILE_CONFLICT_EXCEPTION = "210";
        public const string PROFILE_MAPPING_CONFLICT_EXCEPTION = "211";
        public const string PROFILE_MAPPING_EXIST_EXCEPTION = "220";
        #endregion
        #region CompareAndSync Error Messages (Error Code : 300~499)
        public const string CAS_UNABLE_TO_HASH_EXCEPTION = "300";
        public const string CAS_UNABLE_TO_COPY_FILE_EXCEPTION = "301";
        public const string CAS_UNABLE_TO_DELETE_FILE_EXCEPTION = "302";
        public const string CAS_UNABLE_TO_MOVE_FILE_EXCEPTION = "303";
        public const string CAS_UNABLE_TO_ARCHIVE_FILE_EXCEPTION = "304";

        public const string CAS_UNABLE_TO_COPY_FOLDER_EXCEPTION = "350";
        public const string CAS_UNABLE_TO_CREATE_FOLDER_EXCEPTION = "351";
        public const string CAS_UNABLE_TO_DELETE_FOLDER_EXCEPTION = "352";
        public const string CAS_UNABLE_TO_MOVE_FOLDER_EXCEPTION = "353";        
        public const string CAS_UNABLE_TO_ARCHIVE_FOLDER_EXCEPTION = "354";

        public const string CAS_INCOMPATIBLE_TYPE_EXCEPTION = "400";
        #endregion
        #region Monitor Error Messages (Error Code : 500~599)
        public const string PATH_NOT_FOUND = "500";
        public const string DRIVE_NOT_FOUND = "501";
        #endregion
        #region Logging Error Messages (Error Code : 600~699)
        public const string LOG_FILE_CORRUPTED = "600";
        #endregion
    }
}
