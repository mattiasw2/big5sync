using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
namespace Syncless.Monitor
{
    public class MonitorLayer
    {
        private static MonitorLayer _instance;
        public static MonitorLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MonitorLayer();
                }
                return _instance;
            }
        }
        private MonitorLayer()
        {
        }
        /// <summary>
        /// Start monitoring a path. If the path is already monitored, raise an exception.
        ///    001:/Lectures
        ///    001:/Lectures/lecture1.pdf require only 1 monitor (*important) 
        ///      however, if 001:/Lectures is unmonitored, 001:/Lectures/lecture1.pdf have to be monitored. 
        ///      this is to ensure that if a change is made to lecture1.pdf , it will not be notified twice.
        /// </summary>
        /// <param name="path">The Path to be monitored</param>
        /// <returns>Boolean stating if the monitor can be started</returns
        public bool MonitorPath(string path)
        {
            return true;
        }
        /// <summary>
        /// Unmonitor a path. If the Path does not exist, raise an exception
        ///    if 001:/Lectures is monitored, and i try to unmonitor 001:/Lectures/lecture1.pdf , it should fail.
        ///    if 
        ///     001:/Lectures and 001:/Lectures/lecture1.pdf is being monitored , if i remove 001/Lectures/lecture1.pdf, the next time i remove 001:/Lectures, then 001:/Lectures/lecture1.pdf should not be monitored.
        /// </summary>
        /// <param name="path">The Path to be monitored</param>
        /// <returns>Boolean stating if the monitor can be stopped</returns>
        public bool UnMonitorPath(string path)
        {
            return true;
        }
        /// <summary>
        /// Unmonitor all files that is contained in a physical drive 
        /// </summary>
        /// <param name="driveLetter">The drive letter (i.e 'C') </param>
        /// <returns>The number of paths unmonitored</returns>
        public int UnMonitorDrive(string driveLetter)
        {
            return 0;
        }
    }
}
