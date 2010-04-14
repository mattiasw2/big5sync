/*
 * 
 * Author: Koh Cher Guan
 * 
 */

using System.IO;

namespace Syncless.Monitor.DTO
{
    /// <summary>
    /// A Data Transfer Object to pass information regarding a drive event
    /// </summary>
    public class DriveChangeEvent
    {
        private DriveChangeType _type;
        /// <summary>
        /// Gets or sets a value for the drive change type.
        /// </summary>
        public DriveChangeType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private DriveInfo _info;
        /// <summary>
        /// Gets or sets a value for the drive info.
        /// </summary>
        public DriveInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.DriveChangeEvent" /> class.
        /// </summary>
        public DriveChangeEvent(DriveChangeType type, DriveInfo info)
        {
            this._type = type;
            this._info = info;
        }
    }
}
