using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Logging
{
    /// <summary>
    /// This enum specifies the type of event change.
    /// </summary>
    public enum LogCategoryType
    {
        /// <summary>
        /// Synchronization Category
        /// </summary>
        SYNC,
        /// <summary>
        /// Application Event Category
        /// </summary>
        APPEVENT,
        /// <summary>
        /// File System Changes Category
        /// </summary>
        FSCHANGE,
        /// <summary>
        /// A measure to prevent exception when the log file is tampered.
        /// Do not use this when logging.
        /// </summary>
        UNKNOWN
    }
}
