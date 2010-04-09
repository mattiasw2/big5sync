namespace Syncless.Core.View
{
    /// <summary>
    /// Define the list of possible State for a Tag.
    /// </summary>
    public enum TagState
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined , 
        /// <summary>
        /// Tag is in Seamless Mode
        /// </summary>
        Seamless,
        /// <summary>
        /// Tag is in Manual Mode
        /// </summary>
        Manual,
        /// <summary>
        /// The Tag is Currently in Seamless but is switching to Manual
        /// </summary>
        SeamlessToManual,
        /// <summary>
        /// The Tag is Currently in Manual but is switching to Seamless
        /// </summary>
        ManualToSeamless
    }
}
