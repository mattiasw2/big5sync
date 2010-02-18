using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public abstract class Result
    {
        private FileChangeType changeType;
        private string from;
        private string to = null;

        public FileChangeType ChangeType
        {
            get
            {
                return changeType;
            }
            set
            {
                changeType = value;
            }
        }

        public string From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
            }
        }

        public string To
        {
            get
            {
                return to;
            }
            set
            {
                to = value;
            }
        }
    }
}
