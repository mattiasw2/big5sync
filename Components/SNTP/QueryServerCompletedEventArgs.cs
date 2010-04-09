using System;

namespace DaveyM69.Components.SNTP
{
    /// <summary>
    /// Arguments that are passed along with the SNTPClient.QueryServerCompleted event.
    /// </summary>
    public class QueryServerCompletedEventArgs : EventArgs
    {
		#region Constructors 

        internal QueryServerCompletedEventArgs()
        {
            ErrorData = new ErrorData();
        }

		#endregion Constructors 

		#region Properties

        /// <summary>
        /// Gets the data that was returned by the server.
        /// </summary>
        public SNTPData Data
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets data relating to any error that occurred.
        /// </summary>
        public ErrorData ErrorData
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets whether the local date and time was updated.
        /// </summary>
        public bool LocalDateTimeUpdated
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets whether the server query completed successfully.
        /// NB: It is possible that other errors occurred (not related to the querying of the server) after the query,
        /// so ErrorData should still be examined regardless of the value of this property.
        /// </summary>
        public bool Succeeded
        {
            get;
            internal set;
        }

		#endregion Properties 
    }
}
