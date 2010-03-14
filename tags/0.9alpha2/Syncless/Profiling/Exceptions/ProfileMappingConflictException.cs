using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;
namespace Syncless.Profiling.Exceptions
{
    public class ProfileMappingConflictException : Exception
    {
        public ProfileMapping mapping1;
        public ProfileMapping mapping2;
        public ProfileMappingConflictException(ProfileMapping mapping1, ProfileMapping mapping2):base(""+ErrorMessage.PROFILE_MAPPING_CONFLICT_EXCEPTION)
        {
            this.mapping1 = mapping1;
            this.mapping2 = mapping2;
        }
    }
}
