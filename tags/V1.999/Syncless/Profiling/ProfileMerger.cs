/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System;
using System.Collections.Generic;

namespace Syncless.Profiling
{
    /// <summary>
    /// ProfileMerger class performs merging of several profiles which have the same name.
    /// </summary>
    public static class ProfileMerger
    {
        /// <summary>
        /// Check whether the current profile has the same name as the new profile
        /// </summary>
        /// <param name="currentProfile">The current profile</param>
        /// <param name="newProfile">The new profile</param>
        /// <returns>True if current profile has the same name as new profile, else false</returns>
        public static bool CanMerge(Profile currentProfile, Profile newProfile)
        {
            return currentProfile.ProfileName.ToLower().Equals(newProfile.ProfileName.ToLower());
        }
        
        /// <summary>
        /// Merge the current profile to the list of profiles if they have the same profile name
        /// </summary>
        /// <param name="currentProfile">The current profile</param>
        /// <param name="profileList">The list of profiles for which the current profile is to be merged with</param>
        /// <returns>The current profile</returns>
        public static Profile Merge(Profile currentProfile, List<Profile> profileList)
        {
            foreach (Profile profile in profileList)
            {
                if (profile.ProfileName.ToLower().Equals(currentProfile.ProfileName.ToLower())){
                    currentProfile = Merge(currentProfile, profile);
                }
            }
            return currentProfile;
        }
        
        /// <summary>
        /// Merge the current profile with the new profile
        /// </summary>
        /// <param name="currentProfile">The current profile</param>
        /// <param name="newProfile">The new profile</param>
        /// <returns>The current profile</returns>
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
                        curDrive.LastUpdated = DateTime.UtcNow.Ticks;
                    }
                }
            }
            return currentProfile;
        }
    }
}
