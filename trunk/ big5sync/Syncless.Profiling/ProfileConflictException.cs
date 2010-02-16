using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Profiling
{
    public class ProfileConflictException : Exception
    {
        private ProfileMappingConflictException _conflict;
        public ProfileMappingConflictException Conflict{
            get { return _conflict; }
        }

        public ProfileConflictException(ProfileMappingConflictException conflict):base("puterrormessagehere")
        {
            this._conflict = conflict;
        }
    }
}
