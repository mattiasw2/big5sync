/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System;

namespace Syncless.Profiling.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an attempt to load a profile fails.
    /// </summary>
    public class ProfileLoadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ProfileLoadException class with its message 
        /// string set to a system-supplied message 
        /// </summary>
        /// <param name="e">The inner exception that causes this exception</param>
        public ProfileLoadException(Exception e)
            : base(e.Message, e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ProfileLoadException class with its message 
        /// string set to a system-supplied message
        /// </summary>
        public ProfileLoadException()
        {

        }
    }
}
