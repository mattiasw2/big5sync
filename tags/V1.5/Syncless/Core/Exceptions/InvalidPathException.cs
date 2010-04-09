using System;
using Syncless.Helper;
namespace Syncless.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the path is invalid
    /// </summary>
    public class InvalidPathException :Exception
    {
        /// <summary>
        /// The path that is invalid
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Initialize the InvalidPathException
        /// </summary>
        /// <param name="path">The path that is invalid</param>
        public InvalidPathException(string path):base(ErrorMessage.INVALID_PATH)
        {
            Path = path;
        }
    }
}
