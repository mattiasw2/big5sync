using System;

namespace DaveyM69.Components.SNTP
{
    /// <summary>
    /// Class that holds data relating to any errors that occurred.
    /// </summary>
    public class ErrorData
    {
		#region Constructors

        /// <summary>
        /// Creates a new instance of ErrorData.
        /// </summary>
        /// <param name="errorText">A textual representation of the error that ocurred.</param>
        internal ErrorData(string errorText)
        {
            ErrorText = errorText;
            Error = true;
        }

        /// <summary>
        /// Creates a new instance of ErrorData.
        /// </summary>
        /// <param name="exception">The exception (if any) that was caught.</param>
        internal ErrorData(Exception exception)
        {
            ErrorText = exception.Message;
            Exception = exception;
            Error = true;
        }

        /// <summary>
        /// Creates a new instance of ErrorData.
        /// </summary>
        internal ErrorData() { }

		#endregion Constructors 

		#region Properties 

        /// <summary>
        /// Gets whether an error occurred.
        /// </summary>
        public bool Error
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a textual representation of the error that ocurred.
        /// </summary>
        public string ErrorText
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the exception (if any) that was caught.
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

		#endregion Properties 
    }
}
