using System.Runtime.InteropServices;

namespace DaveyM69.Components.Interop
{
    internal static class Functions
    {
        // http://msdn.microsoft.com/en-us/library/ms724936(VS.85).aspx
        /// <summary>
        /// Sets the current local time and date.
        /// </summary>
        /// <param name="lpSystemTime">A SYSTEMTIME structure that contains the new local date and time.</param>
        /// <returns>true if the function succeeds, otherwise false.</returns>
        [DllImport("kernel32.dll")]
        public static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);
    }
}
