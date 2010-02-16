using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Helper
{
    public static class ErrorMessage
    {
        #region Tagging Error Messages (Error Code : 100~199)

        #endregion
        #region Profiling Error Messages (Error Code : 200~299)
        public const string PROFILE_CONFLICT_EXCEPTION = "210";
        public const string PROFILE_MAPPING_CONFLICT_EXCEPTION = "211";
        #endregion
        #region CompareAndSync Error Messages (Error Code : 300~499)

        #endregion
        #region Monitor Error Messages (Error Code : 500~599)
        public const string PATH_NOT_FOUND = "500";
        public const string DRIVE_NOT_FOUND = "501";
        #endregion
    }
}
