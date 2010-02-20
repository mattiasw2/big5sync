using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;
namespace Syncless.Profiling.Exceptions
{
    public class ProfileMappingExistException:Exception
    {
        private ProfileMapping _mapping;
        public ProfileMapping Mapping
        {
            get { return _mapping; }
            set { _mapping = value; }
        }

        public ProfileMappingExistException(ProfileMapping mapping)
            : base(ErrorMessage.PROFILE_MAPPING_EXIST_EXCEPTION)
        {

        }

    }
}
