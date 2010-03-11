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
        public const string PATH_NOT_FOUND_EXCEPTION = "100";
        public const string PATH_ALREADY_EXISTS_EXCEPTION = "110";
        public const string TAG_NOT_FOUND_EXCEPTION = "101";
        public const string TAG_ALREADY_EXISTS_EXCEPTION = "111";
        public const string RECURSIVE_DIRECTORY_EXCEPTION = "120";
        public const string TAG_TYPE_CONFLICT_EXCEPTION = "130";
        #endregion
        #region Profiling Error Messages (Error Code : 200~299)
        public const string PROFILE_CONFLICT_EXCEPTION = "210";
        public const string PROFILE_MAPPING_CONFLICT_EXCEPTION = "211";
        public const string PROFILE_MAPPING_EXIST_EXCEPTION = "220";
        #endregion
        #region CompareAndSync Error Messages (Error Code : 300~499)
        public const string CAS_UNABLE_TO_HASH_EXCEPTION = "300";
        #endregion
        #region Monitor Error Messages (Error Code : 500~599)
        public const string PATH_NOT_FOUND = "500";
        public const string DRIVE_NOT_FOUND = "501";
        #endregion
    }
}
