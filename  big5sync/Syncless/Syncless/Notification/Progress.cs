using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public abstract class Progress
    {
        public SyncState State
        {
            get;
            protected set;
        }

        public Progress(string tagName)
        {
            State = SyncState.Started;
            TagName = tagName;
        }

        public string TagName
        {
            get;
            set;
        }
        public string Message
        {
            get;
            set;
        }
        public abstract void Complete();
        public abstract void Fail();
        public abstract void Cancel();
        public abstract void Update();
        public bool IsCancel
        {
            get { return State == SyncState.Cancelled; }
        }

       
    }
}
