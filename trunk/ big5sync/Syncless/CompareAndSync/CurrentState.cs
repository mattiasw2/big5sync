using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync
{
    public enum CurrentState
    {
        PopulatedWithoutMeta = 0,
        PopulatedWithMeta = 1,
        Compared = 2,
        Synced = 3,
        SavedToMeta = 4
    }
}
