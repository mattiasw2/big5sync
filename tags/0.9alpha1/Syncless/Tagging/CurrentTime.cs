using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    internal class CurrentTime
    {
        private static DateTime _currentDateTime;

        internal long CurrentTimeLong
        {
            get { return GetCurrentTime(); }
        }

        internal string CurrentTimeString
        {
            get { return GetCurrentTimeString(); }
        }

        internal CurrentTime()
        {
            _currentDateTime = DateTime.Now;
        }

        private static long GetCurrentTime()
        {
            return _currentDateTime.Ticks;
        }

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
