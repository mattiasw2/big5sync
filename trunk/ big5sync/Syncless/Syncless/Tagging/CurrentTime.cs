using System;

namespace Syncless.Tagging
{
    /// <summary>
    /// CurrentTime class allows for repeated use of the same DateTime.Now instance
    /// </summary>
    internal class CurrentTime
    {
        /// <summary>
        /// Private instance of a <see cref="DateTime">DateTime</see> object
        /// </summary>
        private static DateTime _currentDateTime;

        /// <summary>
        /// Gets the current time in long format
        /// </summary>
        internal long CurrentTimeLong
        {
            get { return GetCurrentTime(); }
        }

        /// <summary>
        /// Gets the current time in string format
        /// </summary>
        /// <remarks>DD/MM/YY HH:MM:SS</remarks>
        internal string CurrentTimeString
        {
            get { return GetCurrentTimeString(); }
        }

        /// <summary>
        /// Creates a new CurrentTime object
        /// </summary>
        internal CurrentTime()
        {
            _currentDateTime = DateTime.Now;
        }

        /// <summary>
        /// Gets the number of ticks of the _currentDateTime <see cref="DateTime">DateTime</see> attribute
        /// </summary>
        /// <returns>The number of ticks that represents the _currentDateTime attribute</returns>
        private static long GetCurrentTime()
        {
            return _currentDateTime.Ticks;
        }

        /// <summary>
        /// Gets the string format of the _currentDateTime <see cref="DateTime">DateTime</see> attribute
        /// </summary>
        /// <returns>The string format that represents the _currentDateTime attribute</returns>
        /// <remarks>DD/MM/YY HH:MM:SS</remarks>
        private static string GetCurrentTimeString()
        {
            string day = (_currentDateTime.Day < 10) ? ("0" + _currentDateTime.Day.ToString()) : _currentDateTime.Day.ToString();
            string month = (_currentDateTime.Month < 10) ? ("0" + _currentDateTime.Month.ToString()) : _currentDateTime.Month.ToString();
            string year = _currentDateTime.Year.ToString();
            string hour = (_currentDateTime.Hour < 10) ? ("0" + _currentDateTime.Hour.ToString()) : _currentDateTime.Hour.ToString();
            string minute = (_currentDateTime.Minute < 10) ? ("0" + _currentDateTime.Minute.ToString()) : _currentDateTime.Minute.ToString();
            string second = (_currentDateTime.Second < 10) ? ("0" + _currentDateTime.Second.ToString()) : _currentDateTime.Second.ToString();
            string datestring = string.Format("{0}/{1}/{2} {3}:{4}:{5}", day, month, year, hour, minute, second);
            return datestring;
        }
    }
}
