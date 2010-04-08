
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class PreviewProgress : Progress
    {
        public PreviewProgress(string tagName):base(tagName)
        {


        }

        public override void Complete()
        {
            //Do nothing
        }

        public override void Fail()
        {
            //Do nothing
        }

        public override void Cancel()
        {
            State = SyncState.Cancelled;
        }
        public override void Update()
        {
            //DO nothing
        }
    }
}
