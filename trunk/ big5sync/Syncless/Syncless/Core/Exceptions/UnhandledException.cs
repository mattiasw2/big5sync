/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System;

namespace Syncless.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the system catch a exception that is not handled.
    /// </summary>
    public class UnhandledException :Exception
    {
        /// <summary>
        /// Initialize a UnhandledException with a inner exception
        /// </summary>
        /// <param name="inner">The inner exception</param>
        public UnhandledException(Exception inner):base("UnhandledException",inner)
        {

        }

    }
}
