using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public abstract class Result
    {
        private FileChangeType _changeType;
        private string _from, _to = null;

        public Result(FileChangeType changeType, string from)
        {
            _changeType = changeType;
            _from = from;
        }

        public Result(FileChangeType changeType, string from, string to) :
            this(changeType, from)
        {            
            _to = to;
        }

        public FileChangeType ChangeType
        {
            get
            {
                return _changeType;
            }
            set
            {
                _changeType = value;
            }
        }

        public string From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
            }
        }

        public string To
        {
            get
            {
                return _to;
            }
            set
            {
                _to = value;
            }
        }
    }
}
