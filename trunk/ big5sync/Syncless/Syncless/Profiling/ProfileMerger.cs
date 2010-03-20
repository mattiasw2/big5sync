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
            return currentProfile.ProfileName.ToLower().Equals(newProfile.ProfileName.ToLower());
        }
        public static Profile Merge(Profile currentProfile, List<Profile> profileList)
        {
            foreach (Profile profile in profileList)
            {
                if(profile.ProfileName.ToLower().Equals(currentProfile.ProfileName.ToLower())){
                    currentProfile  = Merge(currentProfile, profile);
                }
            }
            return currentProfile;
        }
        public static Profile Merge(Profile currentProfile, Profile newProfile)
        {
            foreach (ProfileDrive drive in newProfile.ProfileDriveList)
            {
                ProfileDrive curDrive = currentProfile.FindProfileDriveFromGUID(drive.Guid);
                if (curDrive == null)
                {
                    currentProfile.AddProfileDrive(drive);
                }
                else
                {
                    if (drive.DriveName.ToLower().Equals(curDrive.DriveName.ToLower()))
                    {
                        continue;
                    }
                    if (drive.LastUpdated > curDrive.LastUpdated)
                    {
                        curDrive.DriveName = drive.DriveName;
                    }
                }
            }
            return currentProfile;


        }
    }
}
