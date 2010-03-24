using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Syncless.Monitor.DTO
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

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public DriveChangeEvent(DriveChangeType type, DriveInfo info)
        {
            this._type = type;
            this._info = info;
        }

        public DriveChangeEvent(DriveChangeType type, DriveInfo info, string name)
        {
            this._type = type;
            this._info = info;
            this._name = name;
        }
    }
}
