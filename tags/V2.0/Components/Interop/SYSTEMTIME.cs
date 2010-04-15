using System;
using System.Runtime.InteropServices;

namespace DaveyM69.Components.Interop
{
    // http://msdn.microsoft.com/en-us/library/ms724950(VS.85).aspx
    /// <summary>
    /// Specifies a date and time, using individual members for the month, day, year, weekday, hour, minute, second, and millisecond.
    /// The time is either in coordinated universal time (UTC) or local time, depending on the function that is being called.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEMTIME
    {
        /// <summary>
        /// The year. The valid values for this member are 1601 through 30827.
        /// </summary>
        public UInt16 wYear;

        /// <summary>
        /// The month. The valid values for this member are 1 through 12.
        /// </summary>
        public UInt16 wMonth;

        /// <summary>
        /// The day of the week. The valid values for this member are 0 through 6.
        /// </summary>
        public UInt16 wDayOfWeek;

        /// <summary>
        /// The day of the month. The valid values for this member are 1 through 31.
        /// </summary>
        public UInt16 wDay;

        /// <summary>
        /// The hour. The valid values for this member are 0 through 23.
        /// </summary>
        public UInt16 wHour;

        /// <summary>
        /// The minute. The valid values for this member are 0 through 59.
        /// </summary>
        public UInt16 wMinute;

        /// <summary>
        /// The second. The valid values for this member are 0 through 59.
        /// </summary>
        public UInt16 wSecond;

        /// <summary>
        /// The millisecond. The valid values for this member are 0 through 999.
        /// </summary>
        public UInt16 wMilliseconds;
    }
}
