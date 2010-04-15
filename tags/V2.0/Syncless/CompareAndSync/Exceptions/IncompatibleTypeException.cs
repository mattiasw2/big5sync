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
    /// The exception is thrown when incompatible types are detected.
    /// </summary>
    /// <remarks>Incompatible types refer to a folder and file with the exact same name, inclusive of the extension, if any.</remarks>
    public class IncompatibleTypeException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>IncompatibleTypeException</c>.
        /// </summary>
        public IncompatibleTypeException()
            : base(ErrorMessage.CAS_INCOMPATIBLE_TYPE_EXCEPTION)
        {
        }
    }
}
