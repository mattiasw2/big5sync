/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    /// <summary>
    /// The exception is thrown when there is an error hashing a file.
    /// </summary>
    public class HashFileException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>HashFileException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>HashFileException</c> to be thrown.</param>
        public HashFileException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_HASH_EXCEPTION, innerException)
        {
        }
    }
}
