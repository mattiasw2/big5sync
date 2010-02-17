using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Syncless.Monitor
{
    public class DriveChangeEvent
    {
        private DriveChangeType _type;
        public DriveChangeType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private DriveInfo _info;
        public DriveInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }

        public DriveChangeEvent(DriveChangeType type, DriveInfo info)
        {
            this._type = type;
            this._info = info;
        }
    }
}
