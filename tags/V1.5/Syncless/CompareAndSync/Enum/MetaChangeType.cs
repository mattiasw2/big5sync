namespace Syncless.CompareAndSync.Enum
{
    /// <summary>
    /// This enum specifies the change of a file or folder with respect to its own metadata.
    /// </summary>
    public enum MetaChangeType
    {
        Delete, //File, Folder
        New, //File, Folder
        NoChange, //File, Folder
        Rename, //File, Folder
        Update //File
    }
}
