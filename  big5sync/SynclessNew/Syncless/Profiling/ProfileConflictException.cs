using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;
namespace Syncless.Profiling
{
    public class ProfileConflictException : Exception
    {
        private ProfileMappingConflictException _conflict;
        public ProfileMappingConflictException Conflict{
            get { return _conflict; }
        }

        public ProfileConflictException(ProfileMappingConflictException conflict):base(ErrorMessage.PROFILE_CONFLICT_EXCEPTION)
        {
            this._conflict = conflict;
        }
    }
}
