public class CASCFile
{
    private string _fullPath { get; set; }

    public string FullPath
    {
        get
        {
            return _fullPath;
        }
        set
        {
            _fullPath = value;
        }
    }

    private string _fileName { get; set; }

    public string FileName {
        get {
            return _fileName;
        }
        set {
            _fileName = value;
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

    private int _fileDataID { get; set; }
    
    public int FileDataID
    {
        get
        {
            return _fileDataID;
        }
        set {
            _fileDataID = value;
        }
    }

    private long _size { get; set; }

    public long Size
    {
        get
        {
            return _size;
        }
        set
        {
            _size = value;
        }
    }

    private string _isLocal { get; set; }

    public string IsLocal
    {
        get
        {
            return _isLocal;
        }
        set
        {
            _isLocal = value;
        }
    }    

    public byte[] cKey;
    public byte[] eKey;

    public CASCFile(string fullPath, string fileName, int parentDirectoryID, int fileDataID, long fileSize, bool isLocal, byte[] _cKey, byte[] _eKey)
    {
        FullPath = fullPath;
        FileName = fileName;
        ParentDirectoryID = parentDirectoryID;
        FileDataID = fileDataID;
        Size = fileSize;
        IsLocal = isLocal ? "Yes" : "No";
        cKey = _cKey;
        eKey = _eKey;
    }
}

