using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public interface QueueObserver
    {
        void Update();
    }
}
