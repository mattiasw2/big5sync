using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync
{
    public enum FinalState
    {
        Deleted,
        Updated,
        Created,
        Renamed,
        Propagated,
        Unchanged
    }
}
