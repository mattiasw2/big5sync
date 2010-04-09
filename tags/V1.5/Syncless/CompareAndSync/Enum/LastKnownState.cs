namespace Syncless.CompareAndSync.Enum
{
    /// <summary>
    /// This enum specifies the last known state of a file that no longer exists in name, eg. a deleted or renamed file.
    /// </summary>
    public enum LastKnownState
    {
        Renamed,
        Deleted
    }
}
