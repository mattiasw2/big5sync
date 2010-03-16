using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Profiling.Exceptions;

namespace Syncless.Profiling
{
    public static class ProfileMerger
    {
        public static bool CanMerge(Profile currentProfile,Profile newProfile)
        {
            try
            {
                foreach (ProfileMapping mapping in newProfile.Mappings)
                {
                    currentProfile.Contains(mapping);
                }
                foreach (ProfileMapping mapping in currentProfile.Mappings)
                {
                    newProfile.Contains(mapping);
                }
            }
            catch (ProfileMappingConflictException pmce)
            {
                throw new ProfileConflictException(pmce);
            }

            return true;
        }
        public static Profile Merge(Profile currentProfile, Profile newProfile)
        {
            if (!currentProfile.ProfileName.Equals(newProfile.ProfileName))
            {
                throw new ProfileNameDifferentException();
            }
            if (CanMerge(currentProfile,newProfile))
            {
                foreach (ProfileMapping mapping in newProfile.Mappings)
                {
                    if (!currentProfile.Contains(mapping))
                    {
                        currentProfile.CreateMapping(mapping);
                    }
                }
            }
            return currentProfile;
        }
    }
}
