public class CASCFile
{
    private string _FileName { get; set; }

    public string FileName
    {
        get
        {
            return _FileName;
        }
        set
        {
            _FileName = value;
        }
    }

    private long _Size { get; set; }

    public long Size
    {
        get
        {
            return _Size;
        }
        set
        {
            _Size = value;
        }
    }

    private string _IsLocal { get; set; }

    public string IsLocal
    {
        get
        {
            return _IsLocal;
        }
        set
        {
            _IsLocal = value;
        }
    }

    public byte[] cKey;
    public byte[] eKey;

    public CASCFile(string fileName, long fileSize, bool isLocal,byte[] _cKey,byte[] _eKey)
    {
        FileName = fileName;
        Size = fileSize;
        IsLocal = isLocal ? "Yes" : "No";
        cKey = _cKey;
        eKey = _eKey;
    }




}

