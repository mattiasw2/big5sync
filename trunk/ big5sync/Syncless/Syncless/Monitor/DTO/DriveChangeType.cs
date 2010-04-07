using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Monitor.DTO
{
    /// <summary>
    /// This enum specifies the type of drive change.
    /// </summary>
    public enum DriveChangeType
    {
        /// <summary>
        /// A drive is connected to the computer.
        /// </summary>
        DRIVE_IN,
        /// <summary>
        /// A drive is disconnected from the computer.
        /// </summary>
        DRIVE_OUT
    }

}
