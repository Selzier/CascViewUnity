public class CASCFolder
{
    private int _directoryID { get; set; }
    
    public int DirectoryID {
        get
        {
            return _directoryID;
        }
        set {
            _directoryID = value;
        }
    }

    private int _parentDirectoryID { get; set; }

    public int ParentDirectoryID {
        get {
            return _parentDirectoryID;
        }
        set {
            _parentDirectoryID = value;
        }
    }

    private string _value { get; set; }

    public string Value {
        get {
            return _value;
        }
        set {
            _value = value;
        }
    }
    /// <summary>
    /// You can store a directory tree in a single table using any database, by making the table self-referential.
    /// Your file table would then have a foreign key referencing the Directory id. To find the full path,
    /// you must follow it up the chain and build up the path from the end (right), tacking each parent directory onto the front (left). 
    /// </summary>
    /// <param name="directoryID">"Primary key" id field</param>
    /// <param name="parentDirectoryID">"Foreign key" id field, which points to the id of another directory</param>
    /// <param name="value">String containing the directory name</param>
    // https://stackoverflow.com/questions/3343087/storing-a-directory-structure-in-database
    public CASCFolder(int directoryID, int parentDirectoryID, string value) {
        DirectoryID = directoryID;
        ParentDirectoryID = parentDirectoryID;
        Value = value;
    }

}
